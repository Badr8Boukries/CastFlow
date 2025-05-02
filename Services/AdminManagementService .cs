// Services/AdminManagementService.cs
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CastFlow.Api.Data;
using CastFlow.Api.Dtos.Request;
using CastFlow.Api.Dtos.Response;
using CastFlow.Api.Models;
using CastFlow.Api.Repository;
using CastFlow.Api.Services.Interfaces;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;

namespace CastFlow.Api.Services
{
    public class AdminManagementService : IAdminManagementService
    {
        private readonly IUserAdminRepository _userAdminRepo;
        private readonly IUserTalentRepository _userTalentRepo;
        private readonly ApplicationDbContext _context; 
        private readonly ILogger<AdminManagementService> _logger;
        private readonly IConfiguration _configuration;

        public AdminManagementService(
            IUserAdminRepository userAdminRepo,
            IUserTalentRepository userTalentRepo,
            ApplicationDbContext context,
            ILogger<AdminManagementService> logger,
            IConfiguration configuration)
        {
            _userAdminRepo = userAdminRepo ?? throw new ArgumentNullException(nameof(userAdminRepo));
            _userTalentRepo = userTalentRepo ?? throw new ArgumentNullException(nameof(userTalentRepo));
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<bool> InviteAdminAsync(InviteAdminRequestDto inviteDto, long invitingAdminId)
        {
            _logger.LogInformation("Invitation admin pour {Email} par Admin ID {InvitingAdminId}", inviteDto.Email, invitingAdminId);

            bool adminExists = await _userAdminRepo.EmailExistsAsync(inviteDto.Email);
            bool talentExists = await _userTalentRepo.ActiveEmailExistsAsync(inviteDto.Email);

            if (adminExists || talentExists)
            {
                _logger.LogWarning("Échec invitation admin pour {Email}: Email déjà utilisé.", inviteDto.Email);
                return false;
            }

            bool pendingInvitationExists = await _context.AdminInvitationTokens
                .AnyAsync(t => t.Email == inviteDto.Email && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);

            if (pendingInvitationExists)
            {
                _logger.LogWarning("Échec invitation admin pour {Email}: Une invitation valide existe déjà.", inviteDto.Email);
                return false;
            }

            string activationToken = GenerateSecureToken();
            DateTime expiresAt = DateTime.UtcNow.AddDays(1);

            var invitation = new AdminInvitationToken
            {
                Email = inviteDto.Email,
                ActivationToken = activationToken,
                ExpiresAt = expiresAt,
                IsUsed = false,
                InvitedByAdminId = invitingAdminId,
                CreatedAt = DateTime.UtcNow
            };

            try
            {
                var oldTokens = _context.AdminInvitationTokens.Where(t => t.Email == inviteDto.Email);
                if (await oldTokens.AnyAsync())
                {
                    _context.AdminInvitationTokens.RemoveRange(oldTokens);
                    await _context.SaveChangesAsync(); 
                }

                _context.AdminInvitationTokens.Add(invitation);
                await _context.SaveChangesAsync(); 

                await SendAdminActivationEmailAsync(inviteDto.Email, inviteDto.Prenom, activationToken);

                _logger.LogInformation("Invitation admin envoyée avec succès à {Email}", inviteDto.Email);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la création/envoi de l'invitation admin pour {Email}", inviteDto.Email);
                return false;
            }
        }

        public async Task<AuthResponseDto> SetupAdminAccountAsync(SetupAdminAccountRequestDto setupDto)
        {
            _logger.LogInformation("Tentative d'activation de compte admin via token");

            var invitation = await _context.AdminInvitationTokens
                .FirstOrDefaultAsync(t => t.ActivationToken == setupDto.ActivationToken && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);

            if (invitation == null)
            {
                _logger.LogWarning("Échec activation admin: Token invalide, expiré ou déjà utilisé.");
                throw new InvalidOperationException("Lien d'activation invalide ou expiré.");
            }

            bool emailExists = await _userAdminRepo.EmailExistsAsync(invitation.Email) || await _userTalentRepo.ActiveEmailExistsAsync(invitation.Email);
            if (emailExists)
            {
                _logger.LogError("Erreur activation admin: Email {Email} déjà utilisé.", invitation.Email);
                invitation.IsUsed = true;
                await _context.SaveChangesAsync();
                throw new InvalidOperationException("Cet email a été enregistré entre temps.");
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(setupDto.MotDePasse);
            var newAdmin = new UserAdmin
            {
                Prenom = setupDto.Prenom,
                Nom = setupDto.Nom,
                Email = invitation.Email,
                MotDePasseHash = passwordHash,
                CreeLe = DateTime.UtcNow
            };

            using var transaction = await _context.Database.BeginTransactionAsync(); 
            try
            {
                await _userAdminRepo.AddAsync(newAdmin);
                await _userAdminRepo.SaveChangesAsync(); 

                invitation.IsUsed = true;
                invitation.CreatedAdminId = newAdmin.AdminId; 
                _context.AdminInvitationTokens.Update(invitation); 
                await _context.SaveChangesAsync(); 

                await transaction.CommitAsync();
                _logger.LogInformation("Compte admin {Email} activé avec succès.", newAdmin.Email);

                string token = GenerateJwtToken(newAdmin.AdminId, "Admin", newAdmin.Email, newAdmin.Prenom, newAdmin.Nom);
                return new AuthResponseDto("Compte admin créé et activé.", token, newAdmin.AdminId, AuthenticatedUserType.Admin, newAdmin.Prenom, newAdmin.Nom, newAdmin.Email);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Erreur lors de la finalisation de l'activation admin pour {Email}", invitation.Email);
                throw new InvalidOperationException("Une erreur est survenue lors de la création du compte admin.");
            }
        }

        private string GenerateSecureToken(int length = 32)
        {
            byte[] randomNumber = new byte[length];
            using (var rng = RandomNumberGenerator.Create()) { rng.GetBytes(randomNumber); }
            return Convert.ToBase64String(randomNumber).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }

        private async Task SendAdminActivationEmailAsync(string email, string prenom, string activationToken)
        {
            var frontendBaseUrl = _configuration["Urls:FrontendBaseUrl"] ?? "http://localhost:3000"; // URL du FRONT React
            var activationLink = $"{frontendBaseUrl}/activate-admin?token={activationToken}"; // URL que le front doit gérer

            var smtpSettings = _configuration.GetSection("SmtpSettings");
            string? server = smtpSettings["Server"]; string? portStr = smtpSettings["Port"]; string? senderName = smtpSettings["SenderName"];
            string? senderEmail = smtpSettings["SenderEmail"]; string? senderPassword = smtpSettings["SenderPassword"]; string? enableSslStr = smtpSettings["EnableSsl"];

            if (string.IsNullOrWhiteSpace(server) || !int.TryParse(portStr, out int port) || string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(senderPassword) || !bool.TryParse(enableSslStr, out bool enableSsl)) { _logger.LogCritical("Cfg SMTP incomplète !"); return; }
            senderName ??= "CastFlow";

            try
            {
                using var client = new SmtpClient(server) { Port = port, Credentials = new NetworkCredential(senderEmail, senderPassword), EnableSsl = enableSsl };
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = "Invitation à rejoindre CastFlow Admin",
                    Body = $"Bonjour {prenom},\n\nVous avez été invité à rejoindre l'équipe d'administration de CastFlow.\n\nCliquez sur le lien suivant pour activer votre compte et définir votre mot de passe (ce lien expire dans 24h) :\n{activationLink}\n\nL'équipe CastFlow",
                    IsBodyHtml = false,
                };
                mailMessage.To.Add(email);
                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email d'activation admin envoyé à {email}.");
            }
            catch (Exception ex) { _logger.LogError(ex, $"Échec envoi email activation admin à {email}."); }
        }

        private string GenerateJwtToken(long userId, string userType, string email, string firstName, string lastName)
        {
            var secret = _configuration["Jwt:Secret"]; var issuer = _configuration["Jwt:Issuer"]; var audience = _configuration["Jwt:Audience"];
            var expireHours = int.Parse(_configuration["Jwt:ExpireHours"] ?? "1");
            if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience)) { throw new InvalidOperationException("Cfg JWT incomplète."); }
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[] { new Claim("Id", userId.ToString()), new Claim(ClaimTypes.Email, email), new Claim(ClaimTypes.GivenName, firstName), new Claim(ClaimTypes.Surname, lastName), new Claim("userType", userType) };
            var expires = DateTime.UtcNow.AddHours(expireHours);
            var token = new JwtSecurityToken(issuer: issuer, audience: audience, claims: claims, expires: expires, signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
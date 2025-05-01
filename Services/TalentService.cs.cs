// Services/TalentService.cs
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
// using CastFlow.Api.Data; // Normalement pas besoin du DbContext ici
using CastFlow.Api.Dtos.Request;
using CastFlow.Api.Dtos.Response;
using CastFlow.Api.Models;
using CastFlow.Api.Repository;
using CastFlow.Api.Services.Interfaces;
using BCrypt.Net;

namespace CastFlow.Api.Services
{
    public class TalentService : ITalentService
    {
        private readonly IUserTalentRepository _userTalentRepo;
        private readonly IUserAdminRepository _userAdminRepo;
        private readonly ILogger<TalentService> _logger;
        private readonly IConfiguration _configuration;

        public TalentService(
            IUserTalentRepository userTalentRepo,
            IUserAdminRepository userAdminRepo,
            ILogger<TalentService> logger,
            IConfiguration configuration)
        {
            _userTalentRepo = userTalentRepo ?? throw new ArgumentNullException(nameof(userTalentRepo));
            _userAdminRepo = userAdminRepo ?? throw new ArgumentNullException(nameof(userAdminRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<AuthResponseDto> InitiateTalentRegistrationAsync(RegisterTalentRequestDto registerDto)
        {
            bool talentExists = await _userTalentRepo.EmailExistsAsync(registerDto.Email);
            bool adminExists = await _userAdminRepo.EmailExistsAsync(registerDto.Email);

            if (talentExists || adminExists)
            {
                return new AuthResponseDto(false, "Cet email est déjà utilisé.");
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.MotDePasse);

            var newUserTalent = new UserTalent
            {
                Prenom = registerDto.Prenom,
                Nom = registerDto.Nom,
                Email = registerDto.Email,
                MotDePasseHash = passwordHash,
                DateNaissance = registerDto.DateNaissance,
                Sex = registerDto.Sex,
                Telephone = registerDto.Telephone,
                IsEmailVerified = false,
                CreeLe = DateTime.UtcNow,
                ModifieLe = DateTime.UtcNow
            };

            try
            {
                await _userTalentRepo.AddAsync(newUserTalent);
                await _userTalentRepo.SaveChangesAsync();

                string verificationCode = GenerateVerificationCode();
                DateTime expiresAt = DateTime.UtcNow.AddHours(1);

                var emailVerifier = new EmailVerifier
                {
                    Email = newUserTalent.Email,
                    VerificationCode = verificationCode,
                    ExpiresAt = expiresAt,
                    UserId = newUserTalent.TalentId,
                    IsVerified = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _userTalentRepo.AddEmailVerificationAsync(emailVerifier);
                await _userTalentRepo.SaveChangesAsync();

                await SendVerificationEmailAsync(newUserTalent.Email, verificationCode);

                _logger.LogInformation($"Inscription initiée pour {newUserTalent.Email}. Code envoyé.");
                return new AuthResponseDto(true, "Inscription initiée. Veuillez vérifier votre email pour le code de vérification.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors de l'initiation de l'inscription pour {registerDto.Email}");
                // Envisager une suppression du UserTalent créé si l'envoi échoue (transaction)
                return new AuthResponseDto(false, "Une erreur est survenue lors de l'inscription.");
            }
        }

        public async Task<bool> VerifyTalentEmailAsync(VerificationRequestDto verificationDto)
        {
            var verification = await _userTalentRepo.GetValidEmailVerificationAsync(verificationDto.Email, verificationDto.Code);

            if (verification == null)
            {
                _logger.LogWarning($"Échec de vérification pour {verificationDto.Email}. Code invalide ou expiré.");
                return false;
            }

            var userTalent = await _userTalentRepo.GetByIdAsync(verification.UserId);
            if (userTalent == null)
            {
                _logger.LogError($"Utilisateur Talent non trouvé (ID: {verification.UserId}) pour une vérification valide ! Email: {verificationDto.Email}.");
                _userTalentRepo.MarkEmailVerificationAsUsed(verification);
                await _userTalentRepo.SaveChangesAsync();
                return false;
            }

            if (userTalent.IsEmailVerified)
            {
                _logger.LogInformation($"Tentative de re-vérification pour l'email {userTalent.Email} qui est déjà vérifié.");
                _userTalentRepo.MarkEmailVerificationAsUsed(verification); // Marquer l'ancien code comme utilisé quand même
                await _userTalentRepo.SaveChangesAsync();
                return true; // Déjà vérifié, c'est un succès fonctionnel
            }

            userTalent.IsEmailVerified = true;
            userTalent.ModifieLe = DateTime.UtcNow;
            _userTalentRepo.Update(userTalent);

            _userTalentRepo.MarkEmailVerificationAsUsed(verification);

            int changes = await _userTalentRepo.SaveChangesAsync();

            if (changes > 0)
            {
                _logger.LogInformation($"Email vérifié avec succès pour {userTalent.Email}.");
                return true;
            }
            else
            {
                _logger.LogError($"Aucun changement n'a été sauvegardé lors de la vérification de l'email pour {userTalent.Email}.");
                return false;
            }
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto loginDto)
        {
            var admin = await _userAdminRepo.GetByEmailAsync(loginDto.Email);
            if (admin != null)
            {
                if (BCrypt.Net.BCrypt.Verify(loginDto.MotDePasse, admin.MotDePasseHash))
                {
                    _logger.LogInformation($"Connexion réussie pour l'Admin {admin.Email}");
                    string token = GenerateJwtToken(admin.AdminId, "Admin", admin.Email, admin.Prenom, admin.Nom);
                    return new AuthResponseDto("Connexion Admin réussie.", token, admin.AdminId, AuthenticatedUserType.Admin, admin.Prenom, admin.Nom, admin.Email);
                }
                else
                {
                    _logger.LogWarning($"Tentative de connexion échouée (mdp incorrect) pour l'Admin {loginDto.Email}");
                    throw new UnauthorizedAccessException("Email ou mot de passe incorrect.");
                }
            }

            var talent = await _userTalentRepo.GetByEmailAsync(loginDto.Email);
            if (talent != null)
            {
                if (BCrypt.Net.BCrypt.Verify(loginDto.MotDePasse, talent.MotDePasseHash))
                {
                    if (!talent.IsEmailVerified)
                    {
                        _logger.LogWarning($"Tentative de connexion pour Talent non vérifié: {talent.Email}");
                        throw new UnauthorizedAccessException("Votre compte existe mais votre email n'est pas vérifié. Veuillez entrer le code reçu par email.");
                    }

                    _logger.LogInformation($"Connexion réussie pour le Talent {talent.Email}");
                    string token = GenerateJwtToken(talent.TalentId, "Talent", talent.Email, talent.Prenom, talent.Nom);
                    return new AuthResponseDto("Connexion Talent réussie.", token, talent.TalentId, AuthenticatedUserType.Talent, talent.Prenom, talent.Nom, talent.Email);
                }
                else
                {
                    _logger.LogWarning($"Tentative de connexion échouée (mdp incorrect) pour le Talent {loginDto.Email}");
                    throw new UnauthorizedAccessException("Email ou mot de passe incorrect.");
                }
            }

            _logger.LogWarning($"Tentative de connexion échouée (utilisateur inconnu) pour {loginDto.Email}");
            throw new UnauthorizedAccessException("Email ou mot de passe incorrect.");
        }

        private string GenerateVerificationCode()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString();
        }

        private async Task SendVerificationEmailAsync(string email, string code)
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            string? server = smtpSettings["Server"];
            string? portStr = smtpSettings["Port"];
            string? senderName = smtpSettings["SenderName"];
            string? senderEmail = smtpSettings["SenderEmail"];
            string? senderPassword = smtpSettings["SenderPassword"];
            string? enableSslStr = smtpSettings["EnableSsl"];

            if (string.IsNullOrWhiteSpace(server) || !int.TryParse(portStr, out int port) ||
                string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(senderPassword) ||
                !bool.TryParse(enableSslStr, out bool enableSsl))
            {
                _logger.LogCritical("Configuration SMTP incomplète ou invalide. Vérifiez Server, Port, SenderEmail, SenderPassword, EnableSsl.");
                return;
            }
            senderName ??= "CastFlow";

            try
            {
                using var client = new SmtpClient(server)
                {
                    Port = port,
                    Credentials = new NetworkCredential(senderEmail, senderPassword),
                    EnableSsl = enableSsl
                };
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = "Votre code de vérification CastFlow",
                    Body = $"Utilisez ce code pour vérifier votre email : {code}\nCe code expire bientôt.",
                    IsBodyHtml = false,
                };
                mailMessage.To.Add(email);
                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email de vérification envoyé à {email}.");
            }
            catch (ArgumentNullException anex)
            {
                _logger.LogCritical(anex, "L'adresse email de l'expéditeur ({SenderEmail}) est invalide dans la configuration SMTP.", senderEmail);
            }
            catch (SmtpException smtpEx)
            {
                _logger.LogError(smtpEx, "Erreur SMTP lors de l'envoi de l'email de vérification à {Email}.", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur inattendue lors de l'envoi de l'email de vérification à {Email}.", email);
            }
        }

        private string GenerateJwtToken(long userId, string userType, string email, string firstName, string lastName)
        {
            var secret = _configuration["Jwt:Secret"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var expireHours = int.Parse(_configuration["Jwt:ExpireHours"] ?? "1");

            if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
            {
                _logger.LogCritical("Configuration JWT incomplète dans appsettings/secrets. Vérifiez Jwt:Secret, Jwt:Issuer, Jwt:Audience.");
                throw new InvalidOperationException("Configuration JWT incomplète.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.GivenName, firstName),
                new Claim(ClaimTypes.Surname, lastName),
                new Claim("userType", userType)
            };

            var expires = DateTime.UtcNow.AddHours(expireHours);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
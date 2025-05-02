using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
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
                IsDeleted = false,
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
                return new AuthResponseDto(false, "Une erreur est survenue lors de l'inscription.");
            }
        }

        public async Task<bool> VerifyTalentEmailAsync(VerificationRequestDto verificationDto)
        {
            var verification = await _userTalentRepo.GetValidEmailVerificationAsync(verificationDto.Email, verificationDto.Code);
            if (verification == null) return false;

            var userTalent = await _userTalentRepo.GetByIdAsync(verification.UserId);
            if (userTalent == null || userTalent.IsDeleted)
            {
                _userTalentRepo.MarkEmailVerificationAsUsed(verification);
                await _userTalentRepo.SaveChangesAsync();
                return false;
            }

            if (userTalent.IsEmailVerified)
            {
                _userTalentRepo.MarkEmailVerificationAsUsed(verification);
                await _userTalentRepo.SaveChangesAsync();
                return true;
            }

            userTalent.IsEmailVerified = true;
            userTalent.ModifieLe = DateTime.UtcNow;
            _userTalentRepo.Update(userTalent);
            _userTalentRepo.MarkEmailVerificationAsUsed(verification);
            int changes = await _userTalentRepo.SaveChangesAsync();
            return changes > 0;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto loginDto)
        {
            var admin = await _userAdminRepo.GetByEmailAsync(loginDto.Email);
            if (admin != null)
            {
                if (BCrypt.Net.BCrypt.Verify(loginDto.MotDePasse, admin.MotDePasseHash))
                {
                    string token = GenerateJwtToken(admin.AdminId, "Admin", admin.Email, admin.Prenom, admin.Nom);
                    return new AuthResponseDto("Connexion Admin réussie.", token, admin.AdminId, AuthenticatedUserType.Admin, admin.Prenom, admin.Nom, admin.Email);
                }
                else { throw new UnauthorizedAccessException("Email ou mot de passe incorrect."); }
            }

            var talent = await _userTalentRepo.GetByEmailAsync(loginDto.Email);
            if (talent != null && !talent.IsDeleted)
            {
                if (BCrypt.Net.BCrypt.Verify(loginDto.MotDePasse, talent.MotDePasseHash))
                {
                    if (!talent.IsEmailVerified) { throw new UnauthorizedAccessException("Votre compte existe mais votre email n'est pas vérifié. Veuillez entrer le code reçu par email."); }
                    string token = GenerateJwtToken(talent.TalentId, "Talent", talent.Email, talent.Prenom, talent.Nom);
                    return new AuthResponseDto("Connexion Talent réussie.", token, talent.TalentId, AuthenticatedUserType.Talent, talent.Prenom, talent.Nom, talent.Email);
                }
                else { throw new UnauthorizedAccessException("Email ou mot de passe incorrect."); }
            }

            throw new UnauthorizedAccessException("Email ou mot de passe incorrect.");
        }

        public async Task<TalentProfileResponseDto?> GetTalentProfileByIdAsync(long talentId)
        {
            var userTalent = await _userTalentRepo.GetActiveByIdAsync(talentId); // Suppose que cette méthode filtre IsDeleted=false
            if (userTalent == null) return null;

            int age = DateTime.Today.Year - userTalent.DateNaissance.Year;
            if (userTalent.DateNaissance.Date > DateTime.Today.AddYears(-age)) age--;

            return new TalentProfileResponseDto
            {
                TalentId = userTalent.TalentId,
                Prenom = userTalent.Prenom,
                Nom = userTalent.Nom,
                Email = userTalent.Email,
                DateNaissance = userTalent.DateNaissance,
                Age = age,
                Sex = userTalent.Sex,
                Telephone = userTalent.Telephone,
                UrlPhoto = userTalent.UrlPhoto,
                UrlCv = userTalent.UrlCv
            };
        }

        public async Task<IEnumerable<TalentProfileResponseDto>> GetAllActiveTalentsAsync()
        {
            var activeTalents = await _userTalentRepo.GetAllActiveAsync(); 
            return activeTalents.Select(userTalent => {
                int age = DateTime.Today.Year - userTalent.DateNaissance.Year;
                if (userTalent.DateNaissance.Date > DateTime.Today.AddYears(-age)) age--;
                return new TalentProfileResponseDto
                {
                    TalentId = userTalent.TalentId,
                    Prenom = userTalent.Prenom,
                    Nom = userTalent.Nom,
                    Email = userTalent.Email,
                    DateNaissance = userTalent.DateNaissance,
                    Age = age,
                    Sex = userTalent.Sex,
                    Telephone = userTalent.Telephone,
                    UrlPhoto = userTalent.UrlPhoto,
                    UrlCv = userTalent.UrlCv
                };
            }).ToList();
        }

        public async Task<TalentProfileResponseDto?> UpdateTalentProfileAsync(long talentId, TalentProfileUpdateRequestDto updateDto) // Utilise le bon DTO
        {
            _logger.LogInformation("Mise à jour du profil pour Talent ID {TalentId}", talentId);
            var userTalent = await _userTalentRepo.GetActiveByIdAsync(talentId);

            if (userTalent == null)
            {
                _logger.LogWarning("Profil Talent actif non trouvé pour mise à jour ID {TalentId}", talentId);
                return null;
            }

            // On applique les mises à jour depuis le DTO spécifique
            userTalent.Prenom = updateDto.Prenom;
            userTalent.Nom = updateDto.Nom;
            userTalent.DateNaissance = updateDto.DateNaissance;
            userTalent.Sex = updateDto.Sex;
            userTalent.Telephone = updateDto.Telephone; 


            userTalent.ModifieLe = DateTime.UtcNow;

            _userTalentRepo.Update(userTalent);
            await _userTalentRepo.SaveChangesAsync();

            _logger.LogInformation("Profil Talent ID {TalentId} mis à jour avec succès.", talentId);
            return await GetTalentProfileByIdAsync(talentId);
        }

        public async Task<bool> DeactivateTalentAccountAsync(long talentId)
        {
            _logger.LogWarning("Tentative de désactivation/anonymisation du compte Talent ID {TalentId}", talentId);
           
            var userTalent = await _userTalentRepo.GetByIdAsync_IncludeDeleted_TEMP(talentId); 

            if (userTalent == null)
            {
                _logger.LogWarning("Compte Talent ID {TalentId} non trouvé pour désactivation.", talentId);
                return false;
            }

            if (userTalent.IsDeleted) 
            {
                _logger.LogInformation("Compte Talent ID {TalentId} est déjà désactivé.", talentId);
                return true; 
            }

            
            _logger.LogInformation("Anonymisation des données pour Talent ID {TalentId}", talentId);
            string? photoUrlToDelete = userTalent.UrlPhoto; 
            string? cvUrlToDelete = userTalent.UrlCv;       

            userTalent.Prenom = "Utilisateur";
            userTalent.Nom = "Supprimé";
            userTalent.Email = null; 
            userTalent.MotDePasseHash = $"DELETED_{Guid.NewGuid()}";
            userTalent.DateNaissance = new DateTime(1900, 1, 1);
            userTalent.Sex = null;           
            userTalent.Telephone = null;
            userTalent.UrlPhoto = null;
            userTalent.UrlCv = null;
            userTalent.IsEmailVerified = false; 
            userTalent.IsDeleted = true;        
            userTalent.ModifieLe = DateTime.UtcNow;

            _userTalentRepo.Update(userTalent); 

            try
            {
                if (!string.IsNullOrWhiteSpace(photoUrlToDelete))
                {
                    _logger.LogInformation("Fichier photo pour Talent ID {TalentId} marqué pour suppression (URL: {Url})", talentId, photoUrlToDelete); // Log même si commenté
                }
                if (!string.IsNullOrWhiteSpace(cvUrlToDelete))
                {
                    _logger.LogInformation("Fichier CV pour Talent ID {TalentId} marqué pour suppression (URL: {Url})", talentId, cvUrlToDelete); // Log même si commenté
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression des fichiers associés au Talent ID {TalentId} lors de l'anonymisation.", talentId);
            }

            await _userTalentRepo.SaveChangesAsync(); 
            _logger.LogInformation("Compte Talent ID {TalentId} désactivé et anonymisé avec succès.", talentId);
            return true;
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

            if (string.IsNullOrWhiteSpace(server) || !int.TryParse(portStr, out int port) || string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(senderPassword) || !bool.TryParse(enableSslStr, out bool enableSsl)) { _logger.LogCritical("Configuration SMTP incomplète ou invalide."); return; }
            senderName ??= "CastFlow";

            try
            {
                using var client = new SmtpClient(server) { Port = port, Credentials = new NetworkCredential(senderEmail, senderPassword), EnableSsl = enableSsl };
                var mailMessage = new MailMessage { From = new MailAddress(senderEmail, senderName), Subject = "Votre code de vérification CastFlow", Body = $"Utilisez ce code pour vérifier votre email : {code}\nCe code expire bientôt.", IsBodyHtml = false, };
                mailMessage.To.Add(email);
                await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email de vérification envoyé à {email}.");
            }
            catch (ArgumentNullException anex) { _logger.LogCritical(anex, "L'adresse email expéditeur ({SenderEmail}) est invalide.", senderEmail); }
            catch (SmtpException smtpEx) { _logger.LogError(smtpEx, "Erreur SMTP lors de l'envoi à {Email}.", email); }
            catch (Exception ex) { _logger.LogError(ex, "Erreur inattendue lors de l'envoi à {Email}.", email); }
        }

        private string GenerateJwtToken(long userId, string userType, string email, string firstName, string lastName)
        {
            var secret = _configuration["Jwt:Secret"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var expireHours = int.Parse(_configuration["Jwt:ExpireHours"] ?? "1");

            if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
            {
                _logger.LogCritical("Configuration JWT incomplète.");
                throw new InvalidOperationException("Configuration JWT incomplète.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(JwtRegisteredClaimNames.Sub, email),           
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                // --- MODIFICATION ICI ---
                new Claim("Id", userId.ToString()),                       
                // --- FIN MODIFICATION ---
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
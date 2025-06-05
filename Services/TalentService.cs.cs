using AutoMapper;
using Microsoft.AspNetCore.Http; 
using Microsoft.AspNetCore.Identity; 
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
using BCryptNet = BCrypt.Net.BCrypt; 

namespace CastFlow.Api.Services
{
    public class TalentService : ITalentService
    {
        private readonly IUserTalentRepository _userTalentRepo;
        private readonly IUserAdminRepository _userAdminRepo;
        private readonly IRoleRepository _roleRepo; 
        private readonly ILogger<TalentService> _logger;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TalentService(
            IUserTalentRepository userTalentRepo,
            IUserAdminRepository userAdminRepo,
            IRoleRepository roleRepo, 
            ILogger<TalentService> logger,
            IConfiguration configuration,
            IMapper mapper,
            IFileStorageService fileStorageService,
            IHttpContextAccessor httpContextAccessor)
        {
            _userTalentRepo = userTalentRepo ?? throw new ArgumentNullException(nameof(userTalentRepo));
            _userAdminRepo = userAdminRepo ?? throw new ArgumentNullException(nameof(userAdminRepo));
            _roleRepo = roleRepo ?? throw new ArgumentNullException(nameof(roleRepo)); // Gardé
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        }

        private string GetBaseApiUrl()
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            return request != null ? $"{request.Scheme}://{request.Host}" : string.Empty;
        }

        public async Task<AuthResponseDto> InitiateTalentRegistrationAsync(RegisterTalentRequestDto registerDto)
        {
            bool talentExists = await _userTalentRepo.ActiveEmailExistsAsync(registerDto.Email);
            bool adminExists = await _userAdminRepo.EmailExistsAsync(registerDto.Email);
            if (talentExists || adminExists) { return new AuthResponseDto(false, "Cet email est déjà utilisé par un compte actif."); }

            string passwordHash = BCryptNet.HashPassword(registerDto.MotDePasse);
            var newUserTalent = _mapper.Map<UserTalent>(registerDto);
            newUserTalent.MotDePasseHash = passwordHash;
            newUserTalent.IsEmailVerified = false; newUserTalent.IsDeleted = false;
            newUserTalent.CreeLe = DateTime.UtcNow; newUserTalent.ModifieLe = DateTime.UtcNow;
            newUserTalent.DateNaissance = registerDto.DateNaissance;

            string? photoUrlRelatif = null; string? cvUrlRelatif = null;
            try
            {
                if (registerDto.PhotoFile != null && registerDto.PhotoFile.Length > 0)
                {
                    photoUrlRelatif = await _fileStorageService.SaveFileAsync(registerDto.PhotoFile, "profile-photos", $"talent_inscription_{Guid.NewGuid()}");
                    newUserTalent.UrlPhoto = photoUrlRelatif;
                }
                if (registerDto.CvFile != null && registerDto.CvFile.Length > 0)
                {
                    cvUrlRelatif = await _fileStorageService.SaveFileAsync(registerDto.CvFile, "cvs", $"talent_inscription_{Guid.NewGuid()}");
                    newUserTalent.UrlCv = cvUrlRelatif;
                }
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur sauvegarde fichiers inscription pour {Email}", registerDto.Email); return new AuthResponseDto(false, "Erreur traitement fichiers."); }

            try
            {
                await _userTalentRepo.AddAsync(newUserTalent); await _userTalentRepo.SaveChangesAsync();
                string verificationCode = GenerateVerificationCode(); DateTime expiresAt = DateTime.UtcNow.AddHours(1);
                var emailVerifier = new EmailVerifier { Email = newUserTalent.Email!, VerificationCode = verificationCode, ExpiresAt = expiresAt, UserId = newUserTalent.TalentId, IsVerified = false, CreatedAt = DateTime.UtcNow };
                await _userTalentRepo.AddEmailVerificationAsync(emailVerifier); await _userTalentRepo.SaveChangesAsync();
                await SendVerificationEmailAsync(newUserTalent.Email!, verificationCode);
                _logger.LogInformation($"Inscription initiée pour {newUserTalent.Email}. Photo: {newUserTalent.UrlPhoto}, CV: {newUserTalent.UrlCv}");
                return new AuthResponseDto(true, "Inscription initiée. Veuillez vérifier votre email pour le code de vérification.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur finalisation inscription pour {registerDto.Email}");
                if (!string.IsNullOrWhiteSpace(photoUrlRelatif)) await _fileStorageService.DeleteFileAsync(photoUrlRelatif);
                if (!string.IsNullOrWhiteSpace(cvUrlRelatif)) await _fileStorageService.DeleteFileAsync(cvUrlRelatif);
                return new AuthResponseDto(false, "Erreur inscription.");
            }
        }

        public async Task<bool> VerifyTalentEmailAsync(VerificationRequestDto verificationDto)
        {
            var verification = await _userTalentRepo.GetValidEmailVerificationAsync(verificationDto.Email, verificationDto.Code);
            if (verification == null) { _logger.LogWarning($"Échec vérification (Code Invalide/Expiré) pour {verificationDto.Email}."); return false; }
            var userTalent = await _userTalentRepo.GetByIdAsync(verification.UserId);
            if (userTalent == null) { _logger.LogError($"UserTalent non trouvé (ID:{verification.UserId}) pour vérification {verificationDto.Email}."); _userTalentRepo.MarkEmailVerificationAsUsed(verification); await _userTalentRepo.SaveChangesAsync(); return false; }
            if (userTalent.IsDeleted) { _logger.LogWarning($"Tentative vérification pour compte désactivé: {verificationDto.Email}"); _userTalentRepo.MarkEmailVerificationAsUsed(verification); await _userTalentRepo.SaveChangesAsync(); return false; }
            if (userTalent.IsEmailVerified) { _logger.LogInformation($"Email déjà vérifié pour {userTalent.Email}."); _userTalentRepo.MarkEmailVerificationAsUsed(verification); await _userTalentRepo.SaveChangesAsync(); return true; }
            userTalent.IsEmailVerified = true; userTalent.ModifieLe = DateTime.UtcNow;
            _userTalentRepo.Update(userTalent); _userTalentRepo.MarkEmailVerificationAsUsed(verification);
            int changes = await _userTalentRepo.SaveChangesAsync();
            if (changes > 0) { _logger.LogInformation($"Email vérifié pour {userTalent.Email}."); return true; }
            else { _logger.LogError($"Echec sauvegarde lors vérification email pour {verificationDto.Email}."); return false; }
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto loginDto)
        {
            var admin = await _userAdminRepo.GetByEmailAsync(loginDto.Email);
            if (admin != null)
            {
                if (BCryptNet.Verify(loginDto.MotDePasse, admin.MotDePasseHash)) { if (admin.Prenom == null || admin.Nom == null || admin.Email == null) { throw new InvalidOperationException("Données admin incomplètes."); } _logger.LogInformation($"Connexion réussie pour Admin {admin.Email}"); string token = GenerateJwtToken(admin.AdminId, "Admin", admin.Email, admin.Prenom, admin.Nom); return new AuthResponseDto("Connexion Admin réussie.", token, admin.AdminId, AuthenticatedUserType.Admin, admin.Prenom, admin.Nom, admin.Email); }
                else { _logger.LogWarning($"Mdp incorrect pour Admin {loginDto.Email}"); throw new UnauthorizedAccessException("Email ou mot de passe incorrect."); }
            }
            var talent = await _userTalentRepo.GetActiveByEmailAsync(loginDto.Email);
            if (talent != null)
            {
                if (talent.MotDePasseHash == null) { _logger.LogError("Hash de mot de passe manquant pour Talent ID {TalentId}", talent.TalentId); throw new InvalidOperationException("Compte talent corrompu."); }
                if (BCryptNet.Verify(loginDto.MotDePasse, talent.MotDePasseHash)) { if (!talent.IsEmailVerified) { _logger.LogWarning($"Connexion tentée pour Talent non vérifié: {talent.Email}"); throw new UnauthorizedAccessException("Votre compte existe mais votre email n'est pas vérifié."); } if (talent.Prenom == null || talent.Nom == null || talent.Email == null) { _logger.LogError("Données Talent incomplètes pour ID {TalentId}", talent.TalentId); throw new InvalidOperationException("Données talent incomplètes."); } _logger.LogInformation($"Connexion réussie pour Talent {talent.Email}."); string token = GenerateJwtToken(talent.TalentId, "Talent", talent.Email, talent.Prenom, talent.Nom); return new AuthResponseDto("Connexion Talent réussie.", token, talent.TalentId, AuthenticatedUserType.Talent, talent.Prenom, talent.Nom, talent.Email); }
                else { _logger.LogWarning($"Mdp incorrect pour Talent {loginDto.Email}"); throw new UnauthorizedAccessException("Email ou mot de passe incorrect."); }
            }
            _logger.LogWarning($"Utilisateur inconnu ou inactif pour {loginDto.Email}"); throw new UnauthorizedAccessException("Email ou mot de passe incorrect.");
        }

        public async Task<TalentProfileResponseDto?> GetTalentProfileByIdAsync(long talentId)
        {
            var userTalent = await _userTalentRepo.GetActiveByIdAsync(talentId);
            if (userTalent == null) return null;
            var dto = _mapper.Map<TalentProfileResponseDto>(userTalent);
            string baseUrl = GetBaseApiUrl();
            if (!string.IsNullOrWhiteSpace(dto.UrlPhoto)) dto.UrlPhoto = $"{baseUrl}{dto.UrlPhoto}";
            if (!string.IsNullOrWhiteSpace(dto.UrlCv)) dto.UrlCv = $"{baseUrl}{dto.UrlCv}";
            return dto;
        }

        public async Task<IEnumerable<TalentProfileResponseDto>> GetAllActiveTalentsAsync()
        {
            var activeTalents = await _userTalentRepo.GetAllActiveAsync();
            _logger.LogInformation("Récupération de {Count} talents actifs.", activeTalents.Count());
            var dtos = _mapper.Map<List<TalentProfileResponseDto>>(activeTalents);
            string baseUrl = GetBaseApiUrl();
            foreach (var dto in dtos)
            {
                if (!string.IsNullOrWhiteSpace(dto.UrlPhoto)) dto.UrlPhoto = $"{baseUrl}{dto.UrlPhoto}";
                if (!string.IsNullOrWhiteSpace(dto.UrlCv)) dto.UrlCv = $"{baseUrl}{dto.UrlCv}";
            }
            return dtos;
        }

        public async Task<TalentProfileResponseDto?> UpdateTalentProfileAsync(long talentId, TalentProfileUpdateRequestDto updateDto)
        {
            var userTalent = await _userTalentRepo.GetActiveByIdAsync(talentId);
            if (userTalent == null) { _logger.LogWarning("Profil Talent actif non trouvé pour MàJ ID {TalentId}", talentId); return null; }

            _mapper.Map(updateDto, userTalent);

            if (updateDto.PhotoFile != null && updateDto.PhotoFile.Length > 0)
            {
                if (!string.IsNullOrWhiteSpace(userTalent.UrlPhoto)) await _fileStorageService.DeleteFileAsync(userTalent.UrlPhoto);
                userTalent.UrlPhoto = await _fileStorageService.SaveFileAsync(updateDto.PhotoFile, "profile-photos", $"talent_{talentId}_photo");
            }
            if (updateDto.CvFile != null && updateDto.CvFile.Length > 0)
            {
                if (!string.IsNullOrWhiteSpace(userTalent.UrlCv)) await _fileStorageService.DeleteFileAsync(userTalent.UrlCv);
                userTalent.UrlCv = await _fileStorageService.SaveFileAsync(updateDto.CvFile, "cvs", $"talent_{talentId}_cv");
            }

            userTalent.ModifieLe = DateTime.UtcNow;
            await _userTalentRepo.SaveChangesAsync();
            _logger.LogInformation("Profil Talent ID {TalentId} mis à jour.", talentId);
            return await GetTalentProfileByIdAsync(talentId);
        }

        public async Task<bool> DeactivateTalentAccountAsync(long talentId)
        {
            var userTalent = await _userTalentRepo.GetByIdAsync_IncludeDeleted_TEMP(talentId);
            if (userTalent == null) return false; if (userTalent.IsDeleted) return true;
            _logger.LogInformation("Anonymisation et suppression fichiers pour Talent ID {TalentId}", talentId);
            string? photoUrlToDelete = userTalent.UrlPhoto; string? cvUrlToDelete = userTalent.UrlCv;
            userTalent.Prenom = "Utilisateur"; userTalent.Nom = "Supprimé"; userTalent.Email = null;
            userTalent.MotDePasseHash = $"DELETED_{Guid.NewGuid()}"; userTalent.DateNaissance = null;
            userTalent.Sex = null; userTalent.Telephone = null; userTalent.UrlPhoto = null; userTalent.UrlCv = null;
            userTalent.IsEmailVerified = false; userTalent.IsDeleted = true; userTalent.ModifieLe = DateTime.UtcNow;
            _userTalentRepo.Update(userTalent);
            try
            {
                if (!string.IsNullOrWhiteSpace(photoUrlToDelete)) await _fileStorageService.DeleteFileAsync(photoUrlToDelete);
                if (!string.IsNullOrWhiteSpace(cvUrlToDelete)) await _fileStorageService.DeleteFileAsync(cvUrlToDelete);
            }
            catch (Exception ex) { _logger.LogError(ex, "Erreur suppression fichiers associés Talent ID {TalentId} lors désactivation.", talentId); }
            await _userTalentRepo.SaveChangesAsync();
            _logger.LogInformation("Compte Talent ID {TalentId} désactivé et anonymisé.", talentId);
            return true;
        }

        public async Task<IdentityResult> ChangePasswordAsync(long talentId, string currentPassword, string newPassword)
        {
            var user = await _userTalentRepo.GetActiveByIdAsync(talentId); // Doit être actif pour changer le mdp
            if (user == null) return IdentityResult.Failed(new IdentityError { Description = "Utilisateur non trouvé ou inactif." });
            if (string.IsNullOrEmpty(user.MotDePasseHash)) return IdentityResult.Failed(new IdentityError { Description = "Le compte n'a pas de mot de passe défini." });
            if (!BCryptNet.Verify(currentPassword, user.MotDePasseHash)) return IdentityResult.Failed(new IdentityError { Description = "Mot de passe actuel incorrect." });
            user.MotDePasseHash = BCryptNet.HashPassword(newPassword);
            user.ModifieLe = DateTime.UtcNow;
            // _userTalentRepo.Update(user); // EF Core suit les changements
            await _userTalentRepo.SaveChangesAsync();
            _logger.LogInformation("Mot de passe changé pour Talent ID {TalentId}", talentId);
            return IdentityResult.Success;
        }

        private string GenerateVerificationCode() { Random r = new Random(); return r.Next(100000, 999999).ToString(); }
        private async Task SendVerificationEmailAsync(string email, string code)
        {
            var smtpSettings = _configuration.GetSection("SmtpSettings");
            string? server = smtpSettings["Server"]; string? portStr = smtpSettings["Port"]; string? senderName = smtpSettings["SenderName"];
            string? senderEmail = smtpSettings["SenderEmail"]; string? senderPassword = smtpSettings["SenderPassword"]; string? enableSslStr = smtpSettings["EnableSsl"];
            if (string.IsNullOrWhiteSpace(server) || !int.TryParse(portStr, out int port) || string.IsNullOrWhiteSpace(senderEmail) || string.IsNullOrWhiteSpace(senderPassword) || !bool.TryParse(enableSslStr, out bool enableSsl)) { _logger.LogCritical("Cfg SMTP incomplète."); return; }
            senderName ??= "CastFlow";
            try
            {
                using var client = new SmtpClient(server) { Port = port, Credentials = new NetworkCredential(senderEmail, senderPassword), EnableSsl = enableSsl };
                var mailMessage = new MailMessage { From = new MailAddress(senderEmail, senderName), Subject = "Votre code de vérification CastFlow", Body = $"Utilisez ce code pour vérifier votre email : {code}\nCe code expire bientôt.", IsBodyHtml = false, };
                mailMessage.To.Add(email); await client.SendMailAsync(mailMessage);
                _logger.LogInformation($"Email de vérification envoyé à {email}.");
            }
            catch (Exception ex) { _logger.LogError(ex, $"Échec envoi email vérification à {email}."); }
        }

        private string GenerateJwtToken(long userId, string userType, string email, string firstName, string lastName)
        {
            var secret = _configuration["Jwt:Secret"]; var issuer = _configuration["Jwt:Issuer"]; var audience = _configuration["Jwt:Audience"];
            var expireHours = int.Parse(_configuration["Jwt:ExpireHours"] ?? "1");
            if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience)) { _logger.LogCritical("Cfg JWT incomplète."); throw new InvalidOperationException("Cfg JWT incomplète."); }
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[] { new Claim("Id", userId.ToString()), new Claim(ClaimTypes.Email, email), new Claim(ClaimTypes.GivenName, firstName), new Claim(ClaimTypes.Surname, lastName), new Claim("userType", userType), new Claim(JwtRegisteredClaimNames.Sub, email), new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) };
            var expires = DateTime.UtcNow.AddHours(expireHours);
            var token = new JwtSecurityToken(issuer: issuer, audience: audience, claims: claims, expires: expires, signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<IEnumerable<RoleDetailResponseDto>> GetPublishedRolesAsync()
        {
            _logger.LogInformation("Récupération des rôles publiés et actifs pour les talents.");
            var publishedRoles = await _roleRepo.GetAllPublishedActiveRolesWithProjectAsync();
            return _mapper.Map<List<RoleDetailResponseDto>>(publishedRoles);
        }
    }
}
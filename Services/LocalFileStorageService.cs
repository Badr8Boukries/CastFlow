using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO; 
using System.Threading.Tasks;
using CastFlow.Api.Services.Interfaces;
using Microsoft.AspNetCore.Hosting; 
namespace CastFlow.Api.Services
{
    public class LocalFileStorageService : IFileStorageService 
    {
        private readonly IWebHostEnvironment _env; 
        private readonly ILogger<LocalFileStorageService> _logger;
        private readonly string _uploadsFolderName = "uploads"; 

        public LocalFileStorageService(IWebHostEnvironment env, ILogger<LocalFileStorageService> logger)
        {
            _env = env;
            _logger = logger;
        }

        public async Task<string?> SaveFileAsync(IFormFile file, string containerName, string fileNamePrefix)
        {
            if (file == null || file.Length == 0)
                return null;

            try
            {
                var containerPath = Path.Combine(_env.WebRootPath, _uploadsFolderName, containerName);
                if (!Directory.Exists(containerPath))
                {
                    Directory.CreateDirectory(containerPath);
                }

                var fileExtension = Path.GetExtension(file.FileName);
                var uniqueFileName = $"{fileNamePrefix}{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(containerPath, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation("Fichier {FileName} sauvegardé à {FilePath}", uniqueFileName, filePath);

               
                return $"/{_uploadsFolderName}/{containerName}/{uniqueFileName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la sauvegarde du fichier {FileName}", file.FileName);
                return null;
            }
        }

        public Task DeleteFileAsync(string fileIdentifier)
        {
            if (string.IsNullOrWhiteSpace(fileIdentifier))
                return Task.CompletedTask;

            try
            {
   
                var filePath = Path.Combine(_env.WebRootPath, fileIdentifier.TrimStart('/'));

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Fichier {FilePath} supprimé.", filePath);
                }
                else
                {
                    _logger.LogWarning("Tentative de suppression d'un fichier inexistant: {FilePath}", filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la suppression du fichier {FileIdentifier}", fileIdentifier);
            }
            return Task.CompletedTask;
        }
    }
}
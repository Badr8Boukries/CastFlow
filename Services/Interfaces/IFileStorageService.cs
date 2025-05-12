using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CastFlow.Api.Services.Interfaces
{
    public interface IFileStorageService
    {
     
        Task<string?> SaveFileAsync(IFormFile file, string containerName, string fileNamePrefix);

        Task DeleteFileAsync(string fileIdentifier);
    }
}
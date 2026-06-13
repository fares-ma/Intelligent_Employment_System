using Microsoft.AspNetCore.Http;
using System.IO;
using System.Threading.Tasks;

namespace Services.Abstractions;

public interface IRecruiterService
{
    Task<string> UpdateProfilePictureAsync(string recruiterId, string fileName, Stream fileStream);
}

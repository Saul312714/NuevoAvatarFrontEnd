using NuevoAvatarFront.Entities;

namespace NuevoAvatarFront.Services
{
    public interface IModuloApiClient
    {
        Task<List<ModuloDto>> GetAllAsync();
        Task<ModuloDto?> GetByIdAsync(int id);
        Task<ModuloDto?> CreateAsync(ModuloDto dto);
        Task<ModuloDto?> UpdateAsync(ModuloDto dto);
        Task<(bool ok, string? errorMessage)> DeleteAsync(int id);
    }
}


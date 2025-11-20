using NuevoAvatarFront.Entities;

namespace NuevoAvatarFront.Services
{
    public interface IParametroApiClient
    {
        Task<List<ParametroDto>> GetAllAsync(int idUsuario, string token);
        Task<ParametroDto?> GetByIdAsync(string id, int idUsuario, string token);
        Task<(bool ok, string? error)> CreateAsync(ParametroDto dto, int idUsuario, string token);
        Task<(bool ok, string? error)> UpdateAsync(ParametroDto dto, int idUsuario, string token);
        Task<(bool ok, string? error)> DeleteAsync(string id, int idUsuario, string token);
    }
}

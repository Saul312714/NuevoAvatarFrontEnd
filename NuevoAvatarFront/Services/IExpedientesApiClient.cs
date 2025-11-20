using NuevoAvatarFront.Entities;

namespace NuevoAvatarFront.Services
{
    public interface IExpedienteApiClient
    {
        Task<List<ExpedienteDto>> GetAllAsync(int idUsuario, string token);
        Task<ExpedienteDto?> GetByIdAsync(int id, int idUsuario, string token);
        Task<(bool ok, string? error)> CreateAsync(ExpedienteDto dto, int idUsuario, string token);
        Task<(bool ok, string? error)> UpdateAsync(ExpedienteDto dto, int idUsuario, string token);
        Task<(bool ok, string? error)> DeleteAsync(int id, int idUsuario, string token);
    }
}

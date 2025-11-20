using NuevoAvatarFront.Entities;

namespace NuevoAvatarFront.Services
{
    public interface IGrupoApiClient
    {
        Task<List<GrupoDto>> GetAllAsync(string token);
        Task<GrupoDto?> GetByIdAsync(int id, int userId, string token);
        Task<(bool ok, string? error)> CreateAsync(GrupoDto dto, int userId, string token);
        Task<(bool ok, string? error)> UpdateAsync(GrupoDto dto, int userId, string token);
        Task<(bool ok, string? error)> DeleteAsync(int id, int userId, string token);

        // Catálogos necesarios
        Task<List<CursoDto>> GetCursosAsync(string token);
        Task<List<ProfesorDto>> GetProfesoresAsync(string token);
        Task<List<PeriodoDto>> GetPeriodosAsync(int userId, string token);
    }
}

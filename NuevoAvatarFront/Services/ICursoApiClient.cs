using NuevoAvatarFront.DTOs;

namespace NuevoAvatarFront.Services
{
    public interface ICursoApiClient
    {
        Task<List<CursoDto>> GetAllAsync(int? nivel = null, int? idCarrera = null);
        Task<CursoDto?> GetByIdAsync(int id);
        Task<List<CursoDto>> GetByCarreraAsync(int idCarrera);
        Task<(bool Ok, string Msg)> CreateAsync(CursoCreateDto dto);
        Task<(bool Ok, string Msg)> UpdateAsync(int id, CursoUpdateDto dto);
        Task<(bool Ok, string Msg)> DeleteAsync(int id);
    }
}

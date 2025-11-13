using NuevoAvatarFront.DTOs;

namespace NuevoAvatarFront.Services
{
    public interface ICursoApiClient
    {
        Task<List<CursoDto>> GetAllAsync();
        Task<List<CursoDto>> GetByCarreraAndNivelAsync(int carreraId, int? nivel);
        Task<CursoDto?> GetByIdAsync(int id);
        Task<bool> CreateAsync(CursoCreateUpdateDto dto);
        Task<bool> UpdateAsync(int id, CursoCreateUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}

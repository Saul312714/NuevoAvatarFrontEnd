using NuevoAvatarFront.DTOs;

namespace NuevoAvatarFront.Services
{
    public interface ICarreraApiClient
    {
        Task<List<CarreraDto>> GetByInstitucionAsync(int institucionId, string? search);
        Task<CarreraDto?> GetByIdAsync(int id);
        Task<bool> CreateAsync(CarreraCreateUpdateDto dto);
        Task<bool> UpdateAsync(int id, CarreraCreateUpdateDto dto);
        Task<bool> DeleteAsync(int id);
        Task<List<CarreraDto>> GetAllAsync();
    }
}

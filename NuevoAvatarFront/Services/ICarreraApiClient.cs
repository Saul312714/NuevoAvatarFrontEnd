using NuevoAvatarFront.DTOs;

namespace NuevoAvatarFront.Services
{
    public interface ICarreraApiClient
    {
        Task<List<CarreraDto>> GetAllAsync();
        Task<List<CarreraDto>> GetByInstitucionAsync(int idInstitucion);
        Task<CarreraDto?> GetByIdAsync(int id);
        Task<bool> CreateAsync(CarreraCreateDto dto);
        Task<bool> UpdateAsync(int id, CarreraUpdateDto dto);
        Task<bool> DeleteAsync(int id);
    }
}

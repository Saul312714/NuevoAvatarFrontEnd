using NuevoAvatarFront.DTOs;

namespace NuevoAvatarFront.Services
{
    public interface ICarreraApiClient
    {
        Task<List<CarreraDto>> GetAllAsync();
        Task<List<CarreraDto>> GetByInstitucionAsync(int idInstitucion);
        Task<CarreraDto?> GetByIdAsync(int id);
        Task<bool> CreateAsync(CarreraDto carrera);
        Task<bool> UpdateAsync(CarreraDto carrera);
        Task<bool> DeleteAsync(int id);
    }
}

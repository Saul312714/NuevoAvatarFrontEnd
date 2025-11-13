using NuevoAvatarFront.DTOs;

namespace NuevoAvatarFront.Services
{
    public interface IInstitucionApiClient
    {
       
        Task<List<InstitucionDto>> GetAllAsync(string? q = null);  // Listado de instituciones con filtro por nombre
        Task<InstitucionDto?> GetByIdAsync(int id);  // Obtener institución por ID
        Task<(bool Ok, string Msg)> CreateAsync(InstitucionDto institucion);  // Crear institución
        Task<(bool Ok, string Msg)> UpdateAsync(int id, InstitucionDto institucion);  // Actualizar institución
        Task<(bool Ok, string Msg)> DeleteAsync(int id);  // Eliminar institución
    }
}

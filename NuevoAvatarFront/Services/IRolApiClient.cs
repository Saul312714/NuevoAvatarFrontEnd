using NuevoAvatarFront.Entities;

public interface IRolApiClient
{
    Task<List<RolDto>> GetAllRolesAsync(int idUsuarioAccion);
    Task<RolDto?> GetByIdAsync(int id, int idUsuarioAccion);
    Task CreateAsync(RolDto rol, int idUsuarioAccion);
    Task UpdateAsync(RolDto rol, int idUsuarioAccion);
    Task<bool> DeleteAsync(int id, int idUsuarioAccion);
}

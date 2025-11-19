using NuevoAvatarFront.Entities;

namespace NuevoAvatarFront.Services
{
    public interface IRolModuloApiClient
    {
        Task<List<RolModuloDto>> GetByRolAsync(int idRol);
        Task<bool> UpdatePermisosAsync(int idRol, List<RolModuloDto> permisos);
    }
}

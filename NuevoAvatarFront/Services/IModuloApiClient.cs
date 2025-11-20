using NuevoAvatarFront.Entities;

namespace NuevoAvatarFront.Services
{
    public interface IModuloApiClient
    {
        Task<List<ModuloDto>> GetAllAsync();
    }
}


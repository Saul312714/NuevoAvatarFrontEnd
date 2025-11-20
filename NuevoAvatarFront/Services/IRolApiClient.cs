using NuevoAvatarFront.Entities;
using System.Net;

public interface IRolApiClient
{
    Task<(bool ok, HttpStatusCode status, List<RolDto>? Data, string? Message)>
        GetAllAsync(int idUsuarioAccion);

    Task<(bool ok, HttpStatusCode status, RolDto? Data, string? Message)>
        GetByIdAsync(int id, int idUsuarioAccion);

    Task<(bool ok, HttpStatusCode status, RolDto? Data, string? Message)>
        CreateAsync(RolDto rol, int idUsuarioAccion);

    Task<(bool ok, HttpStatusCode status, RolDto? Data, string? Message)>
        UpdateAsync(RolDto rol, int idUsuarioAccion);

    Task<(bool ok, HttpStatusCode status, string? Message)>
        DeleteAsync(int id, int idUsuarioAccion);
}

using System.Collections.Generic;

namespace NuevoAvatarFront.Services
{
    public interface IMockDirecciones
    {
        List<string> ObtenerProvincias();
        List<string> ObtenerCantones(string provincia);
        List<string> ObtenerDistritos(string provincia, string canton);
    }
}

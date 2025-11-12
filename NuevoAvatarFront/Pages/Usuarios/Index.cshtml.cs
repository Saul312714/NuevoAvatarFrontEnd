using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Usuarios
{
    public class IndexModel : PageModel
    {
        private readonly IUsuarioApiClient _usuarioApi;
        private readonly IBitacoraApiClient _bitacoraApi;

        public IndexModel(IUsuarioApiClient usuarioApi, IBitacoraApiClient bitacoraApi)
        {
            _usuarioApi = usuarioApi;
            _bitacoraApi = bitacoraApi;
        }

        public List<UsuarioDto> Usuarios { get; set; } = new();
        public int Page { get; set; } = 1; // Página actual
        public int TotalPages { get; set; } // Total de páginas
        public int PageSize { get; set; } = 10; // Número de usuarios por página

        [BindProperty(SupportsGet = true)]
        public string? Filtro { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Rol { get; set; }

        public async Task OnGetAsync(int page = 1)
        {
            var idUsuarioAccion = HttpContext.Session.GetInt32("UsuarioID") ?? 0;

            // Paginación: establecer tamaño de página
            var pageSize = PageSize;
            Usuarios = await _usuarioApi.GetAllAsync(Filtro, Rol, page, pageSize);

            // Obtener el total de usuarios para calcular el número de páginas
            var totalUsuarios = await _usuarioApi.GetAllAsync(Filtro, Rol); // Llamada a la API para obtener todos los usuarios
            TotalPages = (int)Math.Ceiling((double)totalUsuarios.Count / pageSize); // Calcular el total de páginas

            // Registrar consulta en bitácora
            await _bitacoraApi.RegistrarAsync(idUsuarioAccion, $"Consulta de usuarios (filtro: {Filtro}, rol: {Rol}, página: {page})");
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var idUsuarioAccion = HttpContext.Session.GetInt32("UsuarioID") ?? 0;
            await _usuarioApi.DeleteAsync(id, idUsuarioAccion);
            await _bitacoraApi.RegistrarAsync(idUsuarioAccion, $"Eliminación de usuario {id}");
            return RedirectToPage();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Instituciones
{
    public class DeleteModel : PageModel
    {
        private readonly IInstitucionApiClient _client;
        private readonly IBitacoraApiClient _bitacora;

        public DeleteModel(IInstitucionApiClient client, IBitacoraApiClient bitacora)
        {
            _client = client;
            _bitacora = bitacora;
        }

        [BindProperty] public string Nombre { get; set; } = "";

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var data = await _client.GetByIdAsync(id);
            if (data == null) return RedirectToPage("Index");

            Nombre = data.Nombre;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var (ok, msg) = await _client.DeleteAsync(id);

            if (ok)
            {
                var idUsuario = HttpContext.Session.GetInt32("UsuarioID") ?? 0;
                await _bitacora.RegistrarAsync(idUsuario, $"Eliminó institución con ID={id}");

                TempData["Success"] = "Institución eliminada correctamente.";
                return RedirectToPage("Index");
            }

            TempData["Error"] = msg;
            return RedirectToPage("Index");
        }
    }
}

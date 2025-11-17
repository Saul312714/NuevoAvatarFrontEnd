using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.DTOs;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Instituciones
{
    public class IndexModel : PageModel
    {
        private readonly IInstitucionApiClient _client;

        public IndexModel(IInstitucionApiClient client)
        {
            _client = client;
        }

        // ? CAMBIO: De 'q' a 'nombre' para que coincida con el query string del API
        [BindProperty(SupportsGet = true)]
        public string? nombre { get; set; }

        public List<InstitucionDto> Instituciones { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        [TempData]
        public string? StatusType { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // ? Pasar 'nombre' en lugar de 'q'
                Instituciones = await _client.GetAllAsync(nombre);
            }
            catch (Exception ex)
            {
                StatusType = "error";
                StatusMessage = "Error al cargar instituciones: " + ex.Message;
            }
            return Page();
        }
    }
}

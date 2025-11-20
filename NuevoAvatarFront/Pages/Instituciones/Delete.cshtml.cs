using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.DTOs;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Instituciones
{
    public class DeleteModel : PageModel
    {
        private readonly IInstitucionApiClient _client;

        public DeleteModel(IInstitucionApiClient client)
        {
            _client = client;
        }

        [BindProperty]
        public InstitucionDto Institucion { get; set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        [TempData]
        public string? StatusType { get; set; }  // success | error

        // GET ? cargar la institución
        public async Task<IActionResult> OnGetAsync(int id)
        {
            var inst = await _client.GetByIdAsync(id);

            if (inst == null)
            {
                StatusType = "error";
                StatusMessage = "La institución no existe.";
                return RedirectToPage("/Instituciones/Index");
            }

            Institucion = inst;
            return Page();
        }

        // POST ? ejecutar eliminación
        public async Task<IActionResult> OnPostAsync(int id)
        {
            var resultado = await _client.DeleteAsync(id);

            if (resultado.Ok)
            {
                StatusType = "success";
                StatusMessage = "Institución eliminada correctamente.";
            }
            else
            {
                StatusType = "error";
                StatusMessage = resultado.Msg;
            }

            return RedirectToPage("/Instituciones/Index");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Models;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.PreMatriculas
{
    public class DeleteModel : PageModel
    {
        private readonly IPreMatriculaService _prematricula;
        private readonly ILogger<DeleteModel> _logger;

        public DeleteModel(
            IPreMatriculaService prematricula,
            ILogger<DeleteModel> logger)
        {
            _prematricula = prematricula;
            _logger = logger;
        }

        [BindProperty]
        public PreMatriculaDTO? PreMatricula { get; set; }

        [TempData]
        public string? MensajeError { get; set; }

        [TempData]
        public string? MensajeExito { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                var existente = await _prematricula.GetByIdAsync(id);
                if (existente is null)
                {
                    MensajeError = "No se encontró la prematrícula solicitada.";
                    return RedirectToPage("./Index");
                }

                PreMatricula = existente;
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar la prematrícula para eliminar");
                MensajeError = "Error al cargar la prematrícula: " + ex.Message;
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            try
            {
                var ok = await _prematricula.DeleteAsync(id);

                if (ok)
                {
                    MensajeExito = "Prematrícula eliminada correctamente.";
                }
                else
                {
                    MensajeError = "No se encontró la prematrícula a eliminar.";
                }

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar prematrícula");
                MensajeError = "Error al eliminar la prematrícula: " + ex.Message;
                return RedirectToPage("./Index");
            }
        }
    }
}


using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Models;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.PreMatriculas
{
    public class DetailsModel : PageModel
    {
        private readonly IPreMatriculaService _prematricula;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(IPreMatriculaService prematricula, ILogger<DetailsModel> logger)
        {
            _prematricula = prematricula;
            _logger = logger;
        }

        public PreMatriculaDTO? PreMatricula { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                PreMatricula = await _prematricula.GetByIdAsync(id);
                if (PreMatricula is null)
                {
                    TempData["MensajeError"] = "No se encontró la prematrícula solicitada.";
                    return RedirectToPage("./Index");
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener detalles de la prematrícula {id}");
                TempData["MensajeError"] = "Error al cargar los detalles de la prematrícula.";
                return RedirectToPage("./Index");
            }
        }
    }
}
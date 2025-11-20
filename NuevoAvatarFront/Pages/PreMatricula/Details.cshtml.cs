using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Infra;
using NuevoAvatarFront.Infra.NuevoAvatarFront.Infra;
using NuevoAvatarFront.Models;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.PreMatriculas
{
    public class DetailsModel : PageModel
    {
        private readonly IPreMatriculaService _prematricula;
        private readonly ILogger<DetailsModel> _logger;
        private readonly TokenHelper _tokenHelper;

        public DetailsModel(IPreMatriculaService prematricula, ILogger<DetailsModel> logger, TokenHelper tokenHelper)
        {
            _prematricula = prematricula;
            _logger = logger;
            _tokenHelper = tokenHelper;
        }

        public PreMatriculaDTO? PreMatricula { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (HttpContext.Session.GetString("AccessToken") == null)
                return RedirectToPage("/HU_MICHEL/Login/Index");

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
            catch (ApiException apiEx)
            {
                _logger.LogWarning(apiEx, "Error de API al obtener detalles de la prematrícula {Id}", id);
                TempData["MensajeError"] = apiEx.Message;
                return RedirectToPage("./Index");
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


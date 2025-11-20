using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Infra;          
using NuevoAvatarFront.Infra.NuevoAvatarFront.Infra;
using NuevoAvatarFront.Models;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Periodos
{
    public class UpsertModel : PageModel
    {
        private readonly PeriodoClient _api;
        private readonly TokenHelper _tokenHelper;
        private readonly ILogger<UpsertModel> _logger;

        public UpsertModel(PeriodoClient api, TokenHelper tokenHelper, ILogger<UpsertModel> logger)
        {
            _api = api;
            _tokenHelper = tokenHelper;
            _logger = logger;                            
        }

        [BindProperty]
        public PeriodoUpsertDto Periodo { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (HttpContext.Session.GetString("AccessToken") == null)
                return RedirectToPage("/HU_MICHEL/Login/Index");

            try
            {
                if (id is null) return Page();

                var data = await _api.GetByIdAsync(id.Value);
                if (data is null)
                {
                    TempData["error"] = "No se encontró el período solicitado.";
                    return RedirectToPage("./Index");
                }

                Periodo = new PeriodoUpsertDto
                {
                    ID_Periodo = data.ID_Periodo,
                    Anno = data.Anno,
                    Numero = data.Numero,
                    Fecha_Inicio = data.Fecha_Inicio.Date,
                    Fecha_Fin = data.Fecha_Fin.Date
                };
                return Page();
            }
            catch (ApiException apiEx)
            {
                _logger.LogWarning(apiEx, "Error de API al cargar el período {Id}", id);
                TempData["error"] = apiEx.Message;
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al cargar el período {Id}", id);
                TempData["error"] = "Error al cargar el período. Intente nuevamente más tarde.";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (HttpContext.Session.GetString("AccessToken") == null)
                return RedirectToPage("/HU_MICHEL/Login/Index");

            if (!ModelState.IsValid) return Page();

            if (Periodo.Fecha_Fin < Periodo.Fecha_Inicio)
            {
                ModelState.AddModelError(string.Empty, "La fecha de fin debe ser mayor o igual a la fecha de inicio.");
                return Page();
            }

            try
            {
                bool ok;
                if (Periodo.ID_Periodo > 0)
                    ok = await _api.UpdateAsync(Periodo);
                else
                    ok = await _api.CreateAsync(Periodo);

                if (!ok)
                {
                    TempData["error"] = "No se pudo guardar el período. Verifica año/número únicos y el token.";
                    return Page();
                }

                TempData["ok"] = "Período guardado correctamente.";
                return RedirectToPage("./Index");
            }
            catch (ApiException apiEx)
            {
                _logger.LogWarning(apiEx, "Error de API al guardar período {Id}", Periodo.ID_Periodo);
                TempData["error"] = apiEx.Message;
                ModelState.AddModelError(string.Empty, apiEx.Message);
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al guardar período {Id}", Periodo.ID_Periodo);
                TempData["error"] = "Error al guardar el período. Intente nuevamente más tarde.";
                ModelState.AddModelError(string.Empty, "Error al guardar el período. Intente nuevamente más tarde.");
                return Page();
            }
        }
    }
}

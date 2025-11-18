using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Models;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Periodos
{
    public class UpsertModel : PageModel
    {
        private readonly PeriodoClient _api;
        private readonly TokenHelper _tokenHelper;

        public UpsertModel(PeriodoClient api, TokenHelper tokenHelper)
        {
            _api = api;
            _tokenHelper = tokenHelper;
        }

        [BindProperty]
        public PeriodoUpsertDto Periodo { get; set; } = new();

        // 🔥 Validación de sesión antes de mostrar la página
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (HttpContext.Session.GetString("AccessToken") == null)
                return RedirectToPage("/HU_MICHEL/Login/Index"); // Redirige si no hay token

            if (id is null) return Page(); // crear

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
            catch (Exception ex)
            {
                TempData["error"] = $"Error al guardar: {ex.Message}";
                return Page();
            }
        }
    }
}

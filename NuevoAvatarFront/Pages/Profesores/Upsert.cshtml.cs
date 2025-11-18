using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Models;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Profesores
{
    public class UpsertModel : PageModel
    {
        private readonly ProfesorClient _api;
        private readonly TokenHelper _tokenHelper;
        private readonly ILogger<UpsertModel> _logger;

        public UpsertModel(ProfesorClient api, TokenHelper tokenHelper, ILogger<UpsertModel> logger)
        {
            _api = api;
            _tokenHelper = tokenHelper;
            _logger = logger;
        }

        [BindProperty]
        public ProfesorDto Profesor { get; set; } = new();

        public async Task OnGet(int? id)
        {
            // Validación de token antes de proceder
            var token = await _tokenHelper.EnsureValidAccessTokenAsync();
            if (token == null)
            {
                // Redirigir al login si el token no es válido
                RedirectToPage("/HU_MICHEL/Login/Index");
            }

            if (id.HasValue && id.Value > 0)
            {
                var data = await _api.GetByIdAsync(id.Value);
                if (data is not null) Profesor = data;
            }
        }

        public async Task<IActionResult> OnPostSave()
        {
            // Validación de token antes de proceder
            var token = await _tokenHelper.EnsureValidAccessTokenAsync();
            if (token == null)
            {
                // Redirigir al login si el token no es válido
                RedirectToPage("/HU_MICHEL/Login/Index");
            }

            if (!ModelState.IsValid) return Page();

            bool ok = Profesor.ID_Profesor == 0
                ? await _api.CreateAsync(Profesor)
                : await _api.UpdateAsync(Profesor);

            if (ok) TempData["ok"] = "Profesor guardado correctamente.";
            else TempData["err"] = "No se pudo guardar el profesor.";

            return RedirectToPage("./Index");
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Infra;
using NuevoAvatarFront.Infra.NuevoAvatarFront.Infra;
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

        public async Task<IActionResult> OnGet(int? id)
        {
            var token = await _tokenHelper.EnsureValidAccessTokenAsync();
            if (token == null)
            {
                return RedirectToPage("/HU_MICHEL/Login/Index");
            }

            try
            {
                if (id.HasValue && id.Value > 0)
                {
                    var data = await _api.GetByIdAsync(id.Value);
                    if (data is not null)
                        Profesor = data;
                    else
                        TempData["error"] = "No se encontró el profesor solicitado.";
                }

                return Page();
            }
            catch (ApiException apiEx)
            {
                _logger.LogWarning(apiEx, "Error de API al cargar profesor para edición");
                TempData["error"] = apiEx.Message;
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al cargar profesor para edición");
                TempData["error"] = "Error al cargar la información del profesor. Intente nuevamente más tarde.";
                return RedirectToPage("./Index");
            }
        }

        public async Task<IActionResult> OnPostSave()
        {
            var token = await _tokenHelper.EnsureValidAccessTokenAsync();
            if (token == null)
            {
                return RedirectToPage("/HU_MICHEL/Login/Index");
            }

            
            var hoy = DateTime.Today;
            var fechaMinima = hoy.AddYears(-18); 

            if (Profesor.Fecha_Nacimiento > fechaMinima)
            {
                ModelState.AddModelError("Profesor.Fecha_Nacimiento",
                    "El profesor debe ser mayor de edad (mínimo 18 años).");
            }

            if (string.IsNullOrWhiteSpace(Profesor.Email))
            {
                ModelState.AddModelError("Profesor.Email", "El correo es obligatorio.");
            }
            else
            {
                Profesor.Email = Profesor.Email.Trim();

                if (!Profesor.Email.EndsWith("@cuc.ac.cr", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("Profesor.Email",
                        "El correo debe pertenecer al dominio @cuc.ac.cr.");
                }
            }

            if (!ModelState.IsValid)
                return Page();

            try
            {
                bool ok = Profesor.ID_Profesor == 0
                    ? await _api.CreateAsync(Profesor)
                    : await _api.UpdateAsync(Profesor);

                if (ok)
                {
                    TempData["ok"] = "Profesor guardado correctamente.";
                }
                else
                {
                    TempData["err"] = "No se pudo guardar el profesor.";
                }

                return RedirectToPage("./Index");
            }
            catch (ApiException apiEx)
            {
                _logger.LogWarning(apiEx, "Error de API al guardar profesor");

                var mensaje = apiEx.Message;

                ModelState.AddModelError(string.Empty, mensaje);
                TempData["error"] = mensaje;

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al guardar profesor");

                const string mensajeUsuario =
                    "Ocurrió un error al guardar el profesor. Intente nuevamente más tarde.";

                ModelState.AddModelError(string.Empty, mensajeUsuario);
                TempData["error"] = mensajeUsuario;

                return Page();
            }
        }
    }
}

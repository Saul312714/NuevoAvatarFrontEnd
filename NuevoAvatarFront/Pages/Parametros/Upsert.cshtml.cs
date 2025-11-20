using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Parametros
{
    public class UpsertModel : PageModel
    {
        private readonly IParametroApiClient _parametroApi;

        public UpsertModel(IParametroApiClient parametroApi)
        {
            _parametroApi = parametroApi;
        }

        [BindProperty]
        public ParametroDto Parametro { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Id { get; set; }

        public bool EsEdicion => !string.IsNullOrWhiteSpace(Id);
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var token = HttpContext.Session.GetString("AccessToken");
                var userId = HttpContext.Session.GetInt32("UsuarioID");

                if (token is null || userId is null)
                    return Redirect("/HU_MICHEL/Login");

                if (EsEdicion)
                {
                    if (string.IsNullOrWhiteSpace(Id) || Id.Length > 20)
                        return RedirectToPage("./Index");

                    var dto = await _parametroApi.GetByIdAsync(Id!, userId.Value, token);

                    if (dto is null)
                    {
                        ErrorMessage = "El parámetro solicitado no existe.";
                        return Page();
                    }

                    Parametro = dto;
                }

                return Page();
            }
            catch
            {
                ErrorMessage = "Ocurrió un error al cargar la información. Intente nuevamente.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var token = HttpContext.Session.GetString("AccessToken");
                var userId = HttpContext.Session.GetInt32("UsuarioID");

                if (token is null || userId is null)
                    return Redirect("/HU_MICHEL/Login");

                // ===== VALIDACIONES MÍNIMAS (solo ID y Valor) =====

                // ID obligatorio
                if (string.IsNullOrWhiteSpace(Parametro.ID_Parametro))
                {
                    ErrorMessage = "El campo ID es obligatorio.";
                    return Page();
                }

                // ID mayúsculas y máximo 10
                if (Parametro.ID_Parametro.Length > 10 ||
                    Parametro.ID_Parametro != Parametro.ID_Parametro.ToUpper())
                {
                    ErrorMessage = "El ID debe estar en MAYÚSCULAS y tener máximo 10 caracteres.";
                    return Page();
                }

                // Valor obligatorio
                if (string.IsNullOrWhiteSpace(Parametro.Valor))
                {
                    ErrorMessage = "El campo Valor es obligatorio.";
                    return Page();
                }

                // Valor máximo 500
                if (Parametro.Valor.Length > 500)
                {
                    ErrorMessage = "El valor no puede superar 500 caracteres.";
                    return Page();
                }

                // ===== EJECUTAR CREATE / UPDATE =====

                var response = EsEdicion
                    ? await _parametroApi.UpdateAsync(Parametro, userId.Value, token)
                    : await _parametroApi.CreateAsync(Parametro, userId.Value, token);

                // ===== MANEJO DE ERRORES =====

                if (!response.ok)
                {
                    if (!string.IsNullOrWhiteSpace(response.error))
                    {
                        var msg = response.error.ToLower();

                        if (msg.Contains("duplicate"))
                            ErrorMessage = "Ya existe un parámetro con este ID.";
                        else if (response.error.Length > 200)
                            ErrorMessage = "El servidor devolvió un error inesperado.";
                        else
                            ErrorMessage = response.error!;
                    }
                    else
                    {
                        ErrorMessage = "No se pudo guardar el parámetro.";
                    }

                    return Page();
                }

                // Redirección segura
                return RedirectToPage("./Index");
            }
            catch
            {
                ErrorMessage = "Ocurrió un error inesperado. Intente nuevamente o contacte soporte.";
                return Page();
            }
        }

    }
}

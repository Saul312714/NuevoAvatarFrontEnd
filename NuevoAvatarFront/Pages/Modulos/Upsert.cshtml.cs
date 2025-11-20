using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;
using System.Text.Json;

namespace NuevoAvatarFront.Pages.Modulos
{
    public class UpsertModel : PageModel
    {
        private readonly IModuloApiClient _api;
        private readonly IBitacoraApiClient _bitacora;

        public UpsertModel(IModuloApiClient api, IBitacoraApiClient bitacora)
        {
            _api = api;
            _bitacora = bitacora;
        }

        [BindProperty]
        public ModuloDto Modulo { get; set; } = new();

        public bool IsEdit => Modulo.ID_Modulo > 0;

        // GET
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (HttpContext.Session.GetString("AccessToken") == null)
                return Redirect("/HU_MICHEL/Login");

            if (id == null)
                return Page(); // Crear

            var data = await _api.GetByIdAsync(id.Value);

            if (data == null)
            {
                TempData["error"] = "El módulo no existe.";
                return RedirectToPage("/Modulos/Index");
            }

            Modulo = data!;
            return Page();
        }

        // POST
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            int idUsuario = HttpContext.Session.GetInt32("UsuarioID") ?? 0;

            try
            {
                if (Modulo.ID_Modulo > 0) 
                {
                    // Antes
                    var antes = await _api.GetByIdAsync(Modulo.ID_Modulo);
                    string jsonAntes = JsonSerializer.Serialize(antes);

                    // Actualizar
                    var actualizado = await _api.UpdateAsync(Modulo);
                    string jsonDespues = JsonSerializer.Serialize(actualizado);

                    // Bitácora
                    await _bitacora.RegistrarAsync(
                        idUsuario,
                        $"Actualización de módulo: Antes={jsonAntes}, Después={jsonDespues}"
                    );
                }
                else
                {
                    // Crear
                    var creado = await _api.CreateAsync(Modulo);
                    string jsonNuevo = JsonSerializer.Serialize(creado);

                    // Bitácora
                    await _bitacora.RegistrarAsync(
                        idUsuario,
                        $"Creación de módulo: {jsonNuevo}"
                    );
                }

                TempData["success"] = "Módulo guardado correctamente.";
            }
            catch
            {
                TempData["error"] = "Error al guardar el módulo.";
                return Page();
            }

            return RedirectToPage("/Modulos/Index");
        }
    }
}

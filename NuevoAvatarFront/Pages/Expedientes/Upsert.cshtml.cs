using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Expedientes
{
    public class UpsertModel : PageModel
    {
        private readonly IExpedienteApiClient _expedienteApi;
        private readonly IMockDirecciones _direcciones;

        public UpsertModel(IExpedienteApiClient expedienteApi, IMockDirecciones direcciones)
        {
            _expedienteApi = expedienteApi;
            _direcciones = direcciones;
        }

        [BindProperty]
        public ExpedienteDto Expediente { get; set; } = new();

        // Selects dinámicos
        [BindProperty] public string ProvinciaSeleccionada { get; set; } = "";
        [BindProperty] public string CantonSeleccionado { get; set; } = "";
        [BindProperty] public string DistritoSeleccionado { get; set; } = "";

        public List<string> Provincias { get; set; } = new();
        public List<string> Cantones { get; set; } = new();
        public List<string> Distritos { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public bool EsEdicion => Expediente.ID_Estudiante > 0;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            CargarCatalogos();

            var token = HttpContext.Session.GetString("AccessToken");
            var userId = HttpContext.Session.GetInt32("UsuarioID");

            if (token is null || userId is null)
                return Redirect("/HU_MICHEL/Login");

            if (id.HasValue)
            {
                var dto = await _expedienteApi.GetByIdAsync(id.Value, userId.Value, token);

                if (dto is null)
                {
                    ErrorMessage = "El expediente solicitado no existe.";
                    return Page();
                }

                Expediente = dto;

                // decodificar dirección
                var partes = dto.Direccion?.Split(" - ");
                if (partes?.Length >= 3)
                {
                    ProvinciaSeleccionada = partes[0];
                    CantonSeleccionado = partes[1];
                    DistritoSeleccionado = partes[2];

                    Cantones = _direcciones.ObtenerCantones(ProvinciaSeleccionada);
                    Distritos = _direcciones.ObtenerDistritos(ProvinciaSeleccionada, CantonSeleccionado);
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            CargarCatalogos();

            var token = HttpContext.Session.GetString("AccessToken");
            var userId = HttpContext.Session.GetInt32("UsuarioID");

            if (token is null || userId is null)
                return Redirect("/HU_MICHEL/Login");

            // VALIDACIONES ============================
            if (string.IsNullOrWhiteSpace(Expediente.Identificacion))
                return Error("La identificación es obligatoria.");

            if (string.IsNullOrWhiteSpace(Expediente.Tipo_Identificacion))
                return Error("Debe seleccionar un tipo de identificación.");

            if (string.IsNullOrWhiteSpace(Expediente.Nombre))
                return Error("El nombre completo es obligatorio.");

            if (string.IsNullOrWhiteSpace(Expediente.Telefono))
                return Error("El teléfono es obligatorio.");

            if (!Expediente.Telefono.All(char.IsDigit) || Expediente.Telefono.Length < 8)
                return Error("El teléfono debe tener al menos 8 dígitos.");

            if (Expediente.Fecha_Nacimiento == default)
                return Error("Debe seleccionar una fecha de nacimiento válida.");

            // Validación edad mínima
            var edad = DateTime.Today.Year - Expediente.Fecha_Nacimiento.Year;
            if (Expediente.Fecha_Nacimiento.Date > DateTime.Today.AddYears(-edad)) edad--;

            if (edad < 15)
                return Error("El estudiante debe tener al menos 15 años.");

            if (!(Expediente.Email.EndsWith("@cuc.cr") || Expediente.Email.EndsWith("@cuc.ac.cr")))
                return Error("El email debe pertenecer al dominio cuc.cr o cuc.ac.cr.");

            if (string.IsNullOrWhiteSpace(ProvinciaSeleccionada) ||
                string.IsNullOrWhiteSpace(CantonSeleccionado) ||
                string.IsNullOrWhiteSpace(DistritoSeleccionado))
            {
                return Error("Debe seleccionar Provincia, Cantón y Distrito.");
            }

            // Construir dirección final
            Expediente.Direccion = $"{ProvinciaSeleccionada} - {CantonSeleccionado} - {DistritoSeleccionado}";

            // GUARDAR ============================
            var response = EsEdicion
                ? await _expedienteApi.UpdateAsync(Expediente, userId.Value, token)
                : await _expedienteApi.CreateAsync(Expediente, userId.Value, token);

            if (!response.ok)
                return Error(response.error ?? "No se pudo guardar el expediente.");

            return RedirectToPage("./Index");
        }

        private IActionResult Error(string mensaje)
        {
            ErrorMessage = mensaje;
            return Page();
        }

        private void CargarCatalogos()
        {
            Provincias = _direcciones.ObtenerProvincias();

            if (!string.IsNullOrWhiteSpace(ProvinciaSeleccionada))
                Cantones = _direcciones.ObtenerCantones(ProvinciaSeleccionada);

            if (!string.IsNullOrWhiteSpace(CantonSeleccionado))
                Distritos = _direcciones.ObtenerDistritos(ProvinciaSeleccionada, CantonSeleccionado);
        }
    }
}

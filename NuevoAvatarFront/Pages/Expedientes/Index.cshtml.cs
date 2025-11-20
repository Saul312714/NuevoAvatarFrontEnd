using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Expedientes
{
    public class IndexModel : PageModel
    {
        private readonly IExpedienteApiClient _expedienteApi;
        private readonly IMockDirecciones _mock;

        public IndexModel(IExpedienteApiClient expedienteApi, IMockDirecciones mock)
        {
            _expedienteApi = expedienteApi;
            _mock = mock;
        }

        public List<ExpedienteViewDto> Lista { get; set; } = new();


        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var token = HttpContext.Session.GetString("AccessToken");
                var userId = HttpContext.Session.GetInt32("UsuarioID");

                if (token is null || userId is null)
                    return Redirect("/HU_MICHEL/Login");

                var expedientes = await _expedienteApi.GetAllAsync(userId.Value, token);

                Lista = expedientes.Select(e => ConvertirAViewDto(e)).ToList();

                return Page();
            }
            catch
            {
                Lista = new();
                return Page();
            }
        }

        public async Task<IActionResult> OnGetEliminarAsync(int id)
        {
            var token = HttpContext.Session.GetString("AccessToken");
            var userId = HttpContext.Session.GetInt32("UsuarioID");

            if (token is null || userId is null)
                return Redirect("/HU_MICHEL/Login");

            await _expedienteApi.DeleteAsync(id, userId.Value, token);

            // Volver al listado
            return RedirectToPage();
        }

        private ExpedienteViewDto ConvertirAViewDto(ExpedienteDto e)
        {
            string provincia = "";
            string canton = "";
            string distrito = "";

            // Dirección viene así: "Provincia - Cantón - Distrito" (más opcionalmente otras señas)
            if (!string.IsNullOrWhiteSpace(e.Direccion))
            {
                var partes = e.Direccion.Split(" - ");

                if (partes.Length >= 3)
                {
                    provincia = partes[0];
                    canton = partes[1];
                    distrito = partes[2];
                }
            }

            return new ExpedienteViewDto
            {
                ID_Estudiante = e.ID_Estudiante,
                Identificacion = e.Identificacion,
                Tipo_Identificacion = e.Tipo_Identificacion,
                Nombre = e.Nombre,
                Email = e.Email,
                Provincia = provincia,
                Canton = canton,
                Distrito = distrito
            };
        }
    }
}

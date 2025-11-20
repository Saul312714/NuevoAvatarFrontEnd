using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.DTOs;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Instituciones
{
    public class UpsertModel : PageModel
    {
        private readonly IInstitucionApiClient _client;

        public UpsertModel(IInstitucionApiClient client)
        {
            _client = client;
        }

        [BindProperty]
        public InstitucionDto Institucion { get; set; } = new();

        //  Guardar si venimos en modo edición desde la URL
        public bool EsEdicion { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        [TempData]
        public string? StatusType { get; set; }


        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }


        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id.HasValue && id.Value > 0)
            {
                // ? Estamos en modo EDICIÓN
                EsEdicion = true;

                var inst = await _client.GetByIdAsync(id.Value);
                if (inst == null)
                {
                    StatusType = "error";
                    StatusMessage = "La institución no existe.";
                    return RedirectToPage("/Instituciones/Index");
                }
                Institucion = inst;
            }
            else
            {
                // ? Estamos en modo CREACIÓN
                EsEdicion = false;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            (bool Ok, string Msg) resultado;

            if (EsEdicion)
            {
                resultado = await _client.UpdateAsync(Institucion.ID_Institucion, Institucion);
            }
            else
            {
                resultado = await _client.CreateAsync(Institucion);
            }

            if (resultado.Ok)
            {
                StatusType = "success";
                StatusMessage = EsEdicion
                    ? "Institución actualizada correctamente."
                    : "Institución creada correctamente.";
                return RedirectToPage("/Instituciones/Index");
            }
            else
            {
                StatusType = "error";
                StatusMessage = resultado.Msg;
                return Page();
            }
        }
    }
}
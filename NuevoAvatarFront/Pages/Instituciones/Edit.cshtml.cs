using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;
using NuevoAvatarFront.DTOs;

namespace NuevoAvatarFront.Pages.Instituciones
{
    public class EditModel : PageModel
    {
        private readonly IInstitucionApiClient _client;
        private readonly IBitacoraApiClient _bitacora;

        public EditModel(IInstitucionApiClient client, IBitacoraApiClient bitacora)
        {
            _client = client;
            _bitacora = bitacora;
        }

        [BindProperty] public FormModel Form { get; set; } = new();
        public int Id { get; set; }

        public class FormModel
        {
            [Required]
            [RegularExpression(@"^[A-Za-z¡…Õ”⁄·ÈÌÛ˙‹¸—Ò ]+$", ErrorMessage = "Solo letras y espacios.")]
            public string Nombre { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var data = await _client.GetByIdAsync(id);
            if (data == null)
                return RedirectToPage("Index");

            Id = id;
            Form.Nombre = data.Nombre;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
                return Page();

            var inst = new InstitucionDto { Id = id, Nombre = Form.Nombre.Trim() };
            var (ok, msg) = await _client.UpdateAsync(id, inst);

            if (ok)
            {
                var idUsuario = HttpContext.Session.GetInt32("UsuarioID") ?? 0;
                await _bitacora.RegistrarAsync(idUsuario, $"EditÛ instituciÛn: {inst.Nombre}");

                TempData["Success"] = "InstituciÛn actualizada correctamente.";
                return RedirectToPage("Index");
            }

            ModelState.AddModelError(string.Empty, msg);
            return Page();
        }
    }
}

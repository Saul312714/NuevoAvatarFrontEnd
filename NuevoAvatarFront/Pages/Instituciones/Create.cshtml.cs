using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;
using NuevoAvatarFront.DTOs;

namespace NuevoAvatarFront.Pages.Instituciones
{
    public class CreateModel : PageModel
    {
        private readonly IInstitucionApiClient _client;
        private readonly IBitacoraApiClient _bitacora;

        public CreateModel(IInstitucionApiClient client, IBitacoraApiClient bitacora)
        {
            _client = client;
            _bitacora = bitacora;
        }

        public class FormModel
        {
            [Required(ErrorMessage = "El nombre es requerido.")]
            [RegularExpression(@"^[A-Za-z¡…Õ”⁄·ÈÌÛ˙‹¸—Ò ]+$", ErrorMessage = "Solo se permiten letras y espacios.")]
            public string Nombre { get; set; } = string.Empty;
        }

        [BindProperty] public FormModel Form { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var institucion = new InstitucionDto { Nombre = Form.Nombre.Trim() };
            var (ok, msg) = await _client.CreateAsync(institucion);

            if (ok)
            {
                var idUsuario = HttpContext.Session.GetInt32("UsuarioID") ?? 0;
                await _bitacora.RegistrarAsync(idUsuario, $"CreÛ instituciÛn: {institucion.Nombre}");

                TempData["Success"] = "InstituciÛn creada correctamente.";
                return RedirectToPage("Index");
            }

            ModelState.AddModelError(string.Empty, msg);
            return Page();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.DTOs;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Carreras
{
    public class EditModel : PageModel
    {
        private readonly ICarreraApiClient _carreraService;
        private readonly IInstitucionApiClient _instService;
        private readonly IProfesorApiClient _profService;
        private readonly IBitacoraApiClient _bitacoraService;

        public EditModel(ICarreraApiClient carreraService, IInstitucionApiClient instService,
                         IProfesorApiClient profService, IBitacoraApiClient bitacoraService)
        {
            _carreraService = carreraService;
            _instService = instService;
            _profService = profService;
            _bitacoraService = bitacoraService;
        }

        [BindProperty]
        public CarreraCreateUpdateDto Form { get; set; } = new();

        public int Id { get; set; }
        public List<InstitucionDto> Instituciones { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var carrera = await _carreraService.GetByIdAsync(id);
            if (carrera == null)
                return RedirectToPage("Index");

            Id = id;
            Form.Nombre = carrera.Nombre;
            Form.InstitucionId = carrera.InstitucionId;
            Form.DirectorId = carrera.DirectorId;

            Instituciones = await _instService.GetAllAsync();
            ViewData["Instituciones"] = Instituciones;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Instituciones"] = await _instService.GetAllAsync();
                return Page();
            }

            var ok = await _carreraService.UpdateAsync(id, Form);
            if (ok)
            {
                await _bitacoraService.RegistrarAsync(GetUsuarioId(), $"Actualizó carrera: {Form.Nombre}");
                return RedirectToPage("Index", new { institucionId = Form.InstitucionId });
            }

            TempData["error"] = "Error al actualizar la carrera";
            return Page();
        }

        public async Task<JsonResult> OnGetSearchProfesorAsync(string q)
        {
            var data = await _profService.SearchAsync(q);
            return new JsonResult(data);
        }

        private int GetUsuarioId() =>
            HttpContext.Session.GetInt32("UsuarioID") ?? 0;
    }
}

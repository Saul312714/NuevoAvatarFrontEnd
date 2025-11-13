using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.DTOs;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Carreras
{
    public class CreateModel : PageModel
    {
        private readonly ICarreraApiClient _carreraService;
        private readonly IInstitucionApiClient _instService;
        private readonly IProfesorApiClient _profService;
        private readonly IBitacoraApiClient _bitacoraService;

        public CreateModel(ICarreraApiClient carreraService, IInstitucionApiClient instService,
                           IProfesorApiClient profService, IBitacoraApiClient bitacoraService)
        {
            _carreraService = carreraService;
            _instService = instService;
            _profService = profService;
            _bitacoraService = bitacoraService;
        }

        [BindProperty]
        public CarreraCreateUpdateDto Form { get; set; } = new();

        public List<InstitucionDto> Instituciones { get; set; } = new();

        public async Task OnGetAsync(int? institucionId)
        {
            Instituciones = await _instService.GetAllAsync();
            ViewData["Instituciones"] = Instituciones;

            if (institucionId.HasValue)
                Form.InstitucionId = institucionId.Value;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ViewData["Instituciones"] = await _instService.GetAllAsync();
                return Page();
            }

            var ok = await _carreraService.CreateAsync(Form);
            if (ok)
            {
                await _bitacoraService.RegistrarAsync(GetUsuarioId(), $"Creó carrera: {Form.Nombre}");
                TempData["ok"] = "Carrera creada correctamente";
                return RedirectToPage("Index", new { institucionId = Form.InstitucionId });
            }

            TempData["error"] = "Error al crear la carrera";
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

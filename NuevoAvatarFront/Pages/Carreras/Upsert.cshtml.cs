using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.DTOs;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Carreras
{
    public class UpsertModel : PageModel
    {
        private readonly ICarreraApiClient _carreraApi;
        private readonly IInstitucionApiClient _institucionApi;
        private readonly IProfesorApiClient _profesorApi;

        public UpsertModel(
            ICarreraApiClient carreraApi,
            IInstitucionApiClient institucionApi,
            IProfesorApiClient profesorApi)
        {
            _carreraApi = carreraApi;
            _institucionApi = institucionApi;
            _profesorApi = profesorApi;
        }

        [BindProperty]
        public CarreraDto Carrera { get; set; } = new();

        public List<InstitucionDto> Instituciones { get; set; } = new();
        public List<ProfesorDto> Profesores { get; set; } = new();

        public bool EsEdicion => Carrera.ID_Carrera > 0;

        public async Task<IActionResult> OnGetAsync(int? id, string searchTerm = "")
        {
            Instituciones = await _institucionApi.GetAllAsync();
            Profesores = await _profesorApi.SearchAsync(searchTerm);

            if (id.HasValue)
            {
                Carrera = await _carreraApi.GetByIdAsync(id.Value) ?? new CarreraDto();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            bool ok;

            if (EsEdicion)
                ok = await _carreraApi.UpdateAsync(Carrera);
            else
                ok = await _carreraApi.CreateAsync(Carrera);

            if (!ok)
                TempData["Error"] = "No se pudo guardar la carrera.";

            return RedirectToPage("/Carreras/Index");
        }
    }
}

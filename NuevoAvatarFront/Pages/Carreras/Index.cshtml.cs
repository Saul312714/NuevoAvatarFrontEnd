using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.DTOs;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Carreras
{
    public class IndexModel : PageModel
    {
        private readonly ICarreraApiClient _carreraApi;
        private readonly IInstitucionApiClient _institucionApi;
        private readonly IProfesorApiClient _profesorApi;

        public IndexModel(
            ICarreraApiClient carreraApi,
            IInstitucionApiClient institucionApi,
            IProfesorApiClient profesorApi)
        {
            _carreraApi = carreraApi;
            _institucionApi = institucionApi;
            _profesorApi = profesorApi;
        }

        public List<CarreraDto> Carreras { get; set; } = new();
        public List<InstitucionDto> Instituciones { get; set; } = new();
        public List<ProfesorDto> Profesores { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int FiltroInstitucion { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Obtener instituciones y profesores para mostrar nombres
            Instituciones = await _institucionApi.GetAllAsync();
            Profesores = await _profesorApi.GetAllAsync();

            // Consultar según filtros
            if (FiltroInstitucion > 0)
            {
                Carreras = await _carreraApi.GetByInstitucionAsync(FiltroInstitucion);
            }
            else
            {
                Carreras = await _carreraApi.GetAllAsync();
            }

            // Enriquecer datos con nombres
            foreach (var c in Carreras)
            {
                c.NombreInstitucion = Instituciones
                    .FirstOrDefault(i => i.ID_Institucion == c.ID_Institucion)?.Nombre ?? "";

                c.NombreDirector = Profesores
                    .FirstOrDefault(p => p.ID_Profesor == c.ID_ProfesorDirector)?.Nombre ?? "";
            }

            return Page();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuevoAvatarFront.DTOs;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Carreras
{
    public class IndexModel : PageModel
    {
        private readonly ICarreraApiClient _carreraService;
        private readonly IInstitucionApiClient _institucionService;
        private readonly IProfesorApiClient _profesorService;

        public IndexModel(
            ICarreraApiClient carreraService,
            IInstitucionApiClient institucionService,
            IProfesorApiClient profesorService)
        {
            _carreraService = carreraService;
            _institucionService = institucionService;
            _profesorService = profesorService;
        }

        public List<CarreraDto> Carreras { get; set; } = new();
        public List<SelectListItem> Instituciones { get; set; } = new();
        public List<SelectListItem> Profesores { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int FiltroInstitucionID { get; set; }

        [BindProperty]
        public CarreraDto Carrera { get; set; } = new();

        public string? ErrorMessage { get; set; }

        // ? Diccionarios para mapear IDs a nombres
        public Dictionary<int, string> InstitucionesDict { get; set; } = new();
        public Dictionary<int, string> ProfesoresDict { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Cargar datos de referencia
                await CargarSelects();

                // Crear diccionarios para búsqueda rápida
                InstitucionesDict = Instituciones.ToDictionary(
                    i => int.Parse(i.Value),
                    i => i.Text
                );

                ProfesoresDict = Profesores.ToDictionary(
                    p => int.Parse(p.Value),
                    p => p.Text
                );

                // Lógica del filtro
                if (FiltroInstitucionID > 0)
                {
                    Carreras = await _carreraService.GetByInstitucionAsync(FiltroInstitucionID);
                }
                else
                {
                    Carreras = await _carreraService.GetAllAsync();
                }

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al obtener las carreras: {ex.Message}";
                return Page();
            }
        }

        private async Task CargarSelects()
        {
            // Instituciones
            var inst = await _institucionService.GetAllAsync();
            Instituciones = inst.Select(i =>
                new SelectListItem(i.Nombre, i.ID_Institucion.ToString())
            ).ToList();

            // Profesores
            var prof = await _profesorService.GetAllAsync();
            Profesores = prof.Select(p =>
                new SelectListItem(p.Nombre, p.ID_Profesor.ToString())
            ).ToList();
        }

        // ? Métodos helper para la vista
        public string ObtenerNombreInstitucion(int id)
        {
            return InstitucionesDict.TryGetValue(id, out var nombre) ? nombre : "Desconocida";
        }

        public string ObtenerNombreDirector(int id)
        {
            return ProfesoresDict.TryGetValue(id, out var nombre) ? nombre : "Desconocido";
        }
    }
}
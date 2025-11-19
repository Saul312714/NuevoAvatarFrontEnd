using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuevoAvatarFront.DTOs;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Cursos
{
    public class IndexModel : PageModel
    {
        private readonly ICursoApiClient _cursoApi;
        private readonly ICarreraApiClient _carreraApi;

        public IndexModel(ICursoApiClient cursoApi, ICarreraApiClient carreraApi)
        {
            _cursoApi = cursoApi;
            _carreraApi = carreraApi;
        }

        // FILTROS
        [BindProperty(SupportsGet = true)]
        public int? FiltroNivel { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? FiltroCarrera { get; set; }

        // LISTADOS
        public List<CursoDto> Cursos { get; set; } = new();
        public List<CarreraDto> CarrerasList { get; set; } = new();
        public List<SelectListItem> Carreras { get; set; } = new();

        // GET
        public async Task OnGetAsync()
        {
            // 1. Traer carreras para el filtro
            CarrerasList = await _carreraApi.GetAllAsync();

            Carreras = CarrerasList
                .Select(c => new SelectListItem(c.Nombre, c.ID_Carrera.ToString()))
                .ToList();

            // 2. Traer cursos con filtros
            Cursos = await _cursoApi.GetAllAsync(FiltroNivel, FiltroCarrera);
        }

        // Para mostrar nombre de carrera en tabla
        public string ObtenerNombreCarrera(int idCarrera)
        {
            return CarrerasList.FirstOrDefault(c => c.ID_Carrera == idCarrera)?.Nombre ?? "";
        }
    }
}

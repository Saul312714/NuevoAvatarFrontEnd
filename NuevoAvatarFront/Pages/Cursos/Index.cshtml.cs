using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.DTOs;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Cursos
{
    public class IndexModel : PageModel
    {
        private readonly ICursoApiClient _cursoService;
        private readonly ICarreraApiClient _carreraService;
        private readonly IBitacoraApiClient _bitacoraService;

        public IndexModel(ICursoApiClient cursoService, ICarreraApiClient carreraService, IBitacoraApiClient bitacoraService)
        {
            _cursoService = cursoService;
            _carreraService = carreraService;
            _bitacoraService = bitacoraService;
        }

        [BindProperty(SupportsGet = true)]
        public int CarreraId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? Nivel { get; set; }

        public List<CarreraDto> Carreras { get; set; } = new();
        public List<CursoDto> Cursos { get; set; } = new();

        public async Task OnGetAsync()
        {
            Carreras = await _carreraService.GetAllAsync();
            Cursos = await _cursoService.GetByCarreraAndNivelAsync(CarreraId, Nivel);

            // Bitácora de consulta
            await _bitacoraService.RegistrarAsync(GetUsuarioId(), $"Consultó listado de cursos (CarreraId={CarreraId}, Nivel={Nivel})");
        }

        private int GetUsuarioId() =>
            HttpContext.Session.GetInt32("UsuarioID") ?? 0;
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.DTOs;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Carreras
{
    public class IndexModel : PageModel
    {
        private readonly ICarreraApiClient _carreraService;
        private readonly IInstitucionApiClient _instService;
        private readonly IBitacoraApiClient _bitacoraService;

        public IndexModel(ICarreraApiClient carreraService, IInstitucionApiClient instService, IBitacoraApiClient bitacoraService)
        {
            _carreraService = carreraService;
            _instService = instService;
            _bitacoraService = bitacoraService;
        }

        [BindProperty(SupportsGet = true)]
        public int InstitucionId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Q { get; set; }

        public List<InstitucionDto> Instituciones { get; set; } = new();
        public List<CarreraDto> Carreras { get; set; } = new();

        public async Task OnGetAsync()
        {
            Instituciones = await _instService.GetAllAsync();
            if (Instituciones.Any() && InstitucionId == 0)
                InstitucionId = Instituciones.First().Id;

            Carreras = await _carreraService.GetByInstitucionAsync(InstitucionId, Q);
            await _bitacoraService.RegistrarAsync(GetUsuarioId(), $"Consultó listado de carreras (InstituciónId={InstitucionId})");
        }

        private int GetUsuarioId() =>
            HttpContext.Session.GetInt32("UsuarioID") ?? 0;
    }
}

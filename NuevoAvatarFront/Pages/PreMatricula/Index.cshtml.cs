using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Infra;  
using NuevoAvatarFront.Infra.NuevoAvatarFront.Infra;
using NuevoAvatarFront.Models;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.PreMatriculas
{
    public class IndexModel : PageModel
    {
        private readonly IPreMatriculaService _prematricula;
        private readonly ILogger<IndexModel> _logger;
        private readonly TokenHelper _tokenHelper;

        public IndexModel(
            IPreMatriculaService prematricula,
            ILogger<IndexModel> logger,
            TokenHelper tokenHelper)
        {
            _prematricula = prematricula;
            _logger = logger;
            _tokenHelper = tokenHelper;
        }

        public List<PreMatriculaDTO> PreMatriculas { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? FiltroPeriodo { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? FiltroCarrera { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? FiltroCurso { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? FiltroEstudiante { get; set; }

        [TempData]
        public string? MensajeExito { get; set; }

        [TempData]
        public string? MensajeError { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var token = await _tokenHelper.EnsureValidAccessTokenAsync();
            if (token == null) return RedirectToPage("/HU_MICHEL/Login/Index");

            try
            {
                var todasPrematriculas = await _prematricula.GetAllAsync();

                PreMatriculas = todasPrematriculas.Where(p =>
                    (!FiltroPeriodo.HasValue || p.ID_Periodo == FiltroPeriodo.Value) &&
                    (!FiltroCarrera.HasValue || p.ID_Carrera == FiltroCarrera.Value) &&
                    (!FiltroCurso.HasValue || p.ID_Curso == FiltroCurso.Value) &&
                    (!FiltroEstudiante.HasValue || p.ID_Estudiante == FiltroEstudiante.Value)
                ).ToList();

                return Page();
            }
            catch (ApiException apiEx)
            {
                _logger.LogWarning(apiEx, "Error de API al cargar las prematrículas");
                MensajeError = apiEx.Message; 
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar las prematrículas");
                MensajeError = "Error al cargar las prematrículas. Intente nuevamente más tarde.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var token = await _tokenHelper.EnsureValidAccessTokenAsync();
            if (token == null) return RedirectToPage("/HU_MICHEL/Login/Index");

            try
            {
                var resultado = await _prematricula.DeleteAsync(id);

                if (resultado)
                {
                    MensajeExito = "Prematrícula eliminada correctamente";
                }
                else
                {
                    MensajeError = "No se encontró la prematrícula a eliminar";
                }
            }
            catch (ApiException apiEx)
            {
                _logger.LogWarning(apiEx, "Error de API al eliminar prematrícula {Id}", id);
                MensajeError = apiEx.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al eliminar prematrícula {id}");
                MensajeError = "Error al eliminar la prematrícula. Intente nuevamente más tarde.";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetLimpiarFiltrosAsync()
        {
            var token = await _tokenHelper.EnsureValidAccessTokenAsync();
            if (token == null) return RedirectToPage("/HU_MICHEL/Login/Index");

            FiltroPeriodo = null;
            FiltroCarrera = null;
            FiltroCurso = null;
            FiltroEstudiante = null;

            return RedirectToPage();
        }
    }
}

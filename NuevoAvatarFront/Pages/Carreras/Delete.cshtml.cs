using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuevoAvatarFront.DTOs;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Carreras
{
    public class DeleteModel : PageModel
    {
        private readonly ICarreraApiClient _carreraService;
        private readonly IInstitucionApiClient _institucionService;
        private readonly IProfesorApiClient _profesorService;

        public DeleteModel(ICarreraApiClient carreraService, IInstitucionApiClient institucionService,
            IProfesorApiClient profesorService)
        {
            _carreraService = carreraService;
            _institucionService = institucionService;
            _profesorService = profesorService;
        }

        [BindProperty]
        public CarreraDto Carrera { get; set; } = new();

        //
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        //
        public List<SelectListItem> Instituciones { get; set; } = new();
        public List<SelectListItem> Profesores { get; set; } = new();

        //
        public string NombreInstitucion { get; set; } = string.Empty;
        public string NombreDirector { get; set; } = string.Empty;

        // 
        // GET
        // 
        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                

                var data = await _carreraService.GetByIdAsync(id);

                if (data == null)
                {
                    ErrorMessage = "La carrera no existe.";
                    return Page();
                }

                Carrera = data;

                await CargarSelects();

                NombreInstitucion = Instituciones
                    .FirstOrDefault(x => x.Value == Carrera.ID_Institucion.ToString())?.Text
                    ?? "Desconocida";

                NombreDirector = Profesores
                    .FirstOrDefault(x => x.Value == Carrera.ID_Profesor_Director.ToString())?.Text
                    ?? "Desconocido";

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar la carrera: {ex.Message}";
                return Page();
            }
        }

       
        // POST
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var ok = await _carreraService.DeleteAsync(Carrera.ID_Carrera);

                if (!ok)
                {
                    ErrorMessage = "No se puede eliminar la carrera. Verifique si tiene cursos asociados.";
                    return Page();
                }

                SuccessMessage = "Carrera eliminada correctamente.";
                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al eliminar la carrera: {ex.Message}";
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
    }
}

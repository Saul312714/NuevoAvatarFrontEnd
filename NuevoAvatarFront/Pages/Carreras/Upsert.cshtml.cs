using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuevoAvatarFront.DTOs;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Carreras
{
    public class UpsertModel : PageModel
    {
        private readonly ICarreraApiClient _carreraService;
        private readonly IInstitucionApiClient _institucionService;
        private readonly IProfesorApiClient _profesorService;

        public UpsertModel(
            ICarreraApiClient carreraService,
            IInstitucionApiClient institucionService,
            IProfesorApiClient profesorService)
        {
            _carreraService = carreraService;
            _institucionService = institucionService;
            _profesorService = profesorService;

            Carrera = new CarreraUpdateDto(); // Se reutiliza para create/update
        }

        // PROPIEDADES PARA LA VISTA
        [BindProperty]
        public CarreraUpdateDto Carrera { get; set; }

        public List<SelectListItem> Instituciones { get; set; } = new();
        public List<SelectListItem> Profesores { get; set; } = new();

        public bool EsEdicion => Carrera.ID_Carrera > 0;

        public string BotonTexto => EsEdicion ? "Actualizar" : "Crear";

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }


        // 
        // GET
        // 
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            try
            {
                await CargarSelects();

                if (id.HasValue)
                {
                    var data = await _carreraService.GetByIdAsync(id.Value);

                    if (data == null)
                    {
                        ErrorMessage = "La carrera no existe.";
                        return Page();
                    }

                    Carrera = new CarreraUpdateDto
                    {
                        ID_Carrera = data.ID_Carrera,
                        Nombre = data.Nombre,
                        ID_Institucion = data.ID_Institucion,
                        ID_Profesor_Director = data.ID_Profesor_Director
                    };
                }

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error al cargar datos: {ex.Message}";
                return Page();
            }
        }

        // 
        // POST
        // 
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                await CargarSelects();

                if (!ModelState.IsValid)
                {
                    ErrorMessage = "Por favor revise los campos.";
                    return Page();
                }

                bool ok;

                if (Carrera.ID_Carrera == 0)
                {
                    // CREAR
                    var createDto = new CarreraCreateDto
                    {
                        Nombre = Carrera.Nombre,
                        ID_Institucion = Carrera.ID_Institucion,
                        ID_Profesor_Director = Carrera.ID_Profesor_Director
                    };

                    ok = await _carreraService.CreateAsync(createDto);
                }
                else
                {
                    // ACTUALIZAR
                    ok = await _carreraService.UpdateAsync(Carrera.ID_Carrera, Carrera);
                }

                if (!ok)
                {
                    ErrorMessage = "Ocurrió un error al guardar la carrera (validación del backend).";
                    return Page();
                }

                SuccessMessage = EsEdicion
                    ? "Carrera actualizada correctamente."
                    : "Carrera creada correctamente.";

                return Page();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error inesperado: {ex.Message}";
                return Page();
            }
        }

        // 
        // Cargar selects (Institución + Profesores)
        // 
        private async Task CargarSelects()
        {
            // Instituciones
            var inst = await _institucionService.GetAllAsync();
            Instituciones = inst.Select(i =>
                new SelectListItem(i.Nombre, i.ID_Institucion.ToString())
            ).ToList();

            // Profesores
            var prof = await _profesorService.GetAllAsync(); // tu service debe existir
            Profesores = prof.Select(p =>
                new SelectListItem(p.Nombre, p.ID_Profesor.ToString())
            ).ToList();
        }
    }
}

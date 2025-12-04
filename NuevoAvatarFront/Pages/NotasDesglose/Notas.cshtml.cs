using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Models;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.NotasDesglose
{
    public class NotasModel : PageModel
    {
        private readonly INotasApiClient _notasService;
        private readonly IEstudianteService _estudianteService;
        private readonly IGrupoService _grupoService;
        private readonly ILogger<NotasModel> _logger;

        public NotasModel(
            INotasApiClient notasService,
            IEstudianteService estudianteService,
            IGrupoService grupoService,
            ILogger<NotasModel> logger)
        {
            _notasService = notasService;
            _estudianteService = estudianteService;
            _grupoService = grupoService;
            _logger = logger;
        }

        [BindProperty]
        public int IdGrupo { get; set; }

        [BindProperty]
        public int IdEstudiante { get; set; }

        public List<NotaRubro> Rubros { get; set; } = new();
        public List<Nota> Notas { get; set; } = new();
        public List<EstudianteDto> Estudiantes { get; set; } = new();

        // Para saber si hay desglose y bloquear flujo si no existe
        public bool TieneDesglose => Rubros != null && Rubros.Any();

        // GET: carga grupo, rubros y estudiantes
        public async Task<IActionResult> OnGetAsync(int idGrupo)
        {
            IdGrupo = idGrupo;

            var idUsuario = HttpContext.Session.GetInt32("UsuarioID") ?? 0;
            if (idUsuario == 0)
                return RedirectToPage("/HU_MICHEL/Login/Index");

            // 1. Cargar rubros (desglose)
            Rubros = await _notasService.ObtenerDesgloseAsync(IdGrupo, idUsuario);

            if (!TieneDesglose)
            {
                ModelState.AddModelError(string.Empty,
                    "Este grupo no tiene un desglose configurado. Debe crear el desglose antes de asignar notas.");
                return Page();
            }

            // 2. Obtener el grupo para saber el ID_Curso
            var grupos = await _grupoService.GetAllAsync();
            var grupo = grupos.FirstOrDefault(g => g.ID_Grupo == idGrupo);

            if (grupo == null)
            {
                ModelState.AddModelError(string.Empty, "No se encontró información del grupo.");
                return Page();
            }

            int idCurso = grupo.ID_Curso;

            // 3. Obtener solo estudiantes matriculados en ese curso+grupo
            Estudiantes = await _estudianteService.GetByCursoYGrupoAsync(idCurso, IdGrupo);

            return Page();
        }

        // POST: seleccionar estudiante
        public async Task<IActionResult> OnPostSeleccionarAsync()
        {
            var idUsuario = HttpContext.Session.GetInt32("UsuarioID") ?? 0;
            if (idUsuario == 0)
                return RedirectToPage("/HU_MICHEL/Login/Index");

            // 1. Cargar rubros
            Rubros = await _notasService.ObtenerDesgloseAsync(IdGrupo, idUsuario);

            if (!TieneDesglose)
            {
                ModelState.AddModelError(string.Empty,
                    "Este grupo no tiene un desglose configurado. Debe crear el desglose antes de asignar notas.");
                return Page();
            }

            // 2. Obtener curso del grupo
            var grupos = await _grupoService.GetAllAsync();
            var grupo = grupos.FirstOrDefault(g => g.ID_Grupo == IdGrupo);

            if (grupo == null)
            {
                ModelState.AddModelError(string.Empty, "No se encontró información del grupo.");
                return Page();
            }

            int idCurso = grupo.ID_Curso;

            // 3. Estudiantes del grupo
            Estudiantes = await _estudianteService.GetByCursoYGrupoAsync(idCurso, IdGrupo);

            // 4. Cargar notas del estudiante seleccionado (si hay)
            Notas = await _notasService.ObtenerNotasAsync(IdEstudiante, IdGrupo, idUsuario);

            return Page();
        }

        // POST: guardar nota
        public async Task<IActionResult> OnPostGuardarNotaAsync(int idRubro, decimal valor)
        {
            var idUsuario = HttpContext.Session.GetInt32("UsuarioID") ?? 0;
            if (idUsuario == 0)
                return RedirectToPage("/HU_MICHEL/Login/Index");

            // Validar que exista desglose antes de guardar
            Rubros = await _notasService.ObtenerDesgloseAsync(IdGrupo, idUsuario);
            if (!TieneDesglose)
            {
                ModelState.AddModelError(string.Empty,
                    "No se pueden asignar notas porque este grupo no tiene un desglose configurado.");
                return await RecargarContextoYVolver(idUsuario);
            }

            // Validar rango 1–100
            if (valor < 1 || valor > 100)
            {
                ModelState.AddModelError(string.Empty, "La nota debe estar entre 1 y 100.");
                return await RecargarContextoYVolver(idUsuario);
            }

            var nota = new Nota
            {
                ID_Grupo = IdGrupo,
                ID_Estudiante = IdEstudiante,
                ID_Rubro = idRubro,
                Valor = valor
            };

            var result = await _notasService.AsignarNotaAsync(nota, idUsuario);

            if (!result.Ok)
            {
                ModelState.AddModelError(string.Empty, result.Msg ?? "No se pudo asignar la nota.");
                return await RecargarContextoYVolver(idUsuario);
            }

            // Si todo sale bien, recargar contexto y quedarse en la misma vista
            TempData["success"] = "Nota guardada correctamente.";
            return await RecargarContextoYVolver(idUsuario);
        }

        // Método auxiliar para no repetir código
        private async Task<IActionResult> RecargarContextoYVolver(int idUsuario)
        {
            // Recargar rubros
            Rubros = await _notasService.ObtenerDesgloseAsync(IdGrupo, idUsuario);

            // Obtener curso y estudiantes
            var grupos = await _grupoService.GetAllAsync();
            var grupo = grupos.FirstOrDefault(g => g.ID_Grupo == IdGrupo);
            int idCurso = grupo?.ID_Curso ?? 0;

            Estudiantes = await _estudianteService.GetByCursoYGrupoAsync(idCurso, IdGrupo);

            // Recargar notas del estudiante seleccionado
            if (IdEstudiante != 0)
            {
                Notas = await _notasService.ObtenerNotasAsync(IdEstudiante, IdGrupo, idUsuario);
            }
            else
            {
                Notas = new();
            }

            return Page();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;
using NuevoAvatarFront.Models;

namespace NuevoAvatarFront.Pages.NotasDesglose
{
    public class DesgloseModel : PageModel
    {
        private readonly INotasApiClient _notasService;
        private readonly IEstudianteService _estudianteService;
        private readonly IGrupoApiClient _grupoApi;

        public DesgloseModel(
            INotasApiClient notasService,
            IEstudianteService estudianteService,
            IGrupoApiClient grupoApi)
        {
            _notasService = notasService;
            _estudianteService = estudianteService;
            _grupoApi = grupoApi;
        }

        [BindProperty]
        public List<NotaRubro> Rubros { get; set; } = new();

        [BindProperty]
        public int IdGrupo { get; set; }

        [BindProperty]
        public bool DesgloseBloqueado { get; set; }

        // Para mostrar el total de porcentaje en la vista (opcional, pero muy útil)
        public int TotalPorcentaje => Rubros?.Sum(r => r.Porcentaje) ?? 0;

        public async Task<IActionResult> OnGetAsync(int idGrupo)
        {
            IdGrupo = idGrupo;

            var token = HttpContext.Session.GetString("AccessToken");
            var idUsuario = HttpContext.Session.GetInt32("UsuarioID");

            if (token is null || idUsuario is null)
                return RedirectToPage("/HU_MICHEL/Login/Index");

            // 1. Obtener el desglose existente desde el backend
            Rubros = await _notasService.ObtenerDesgloseAsync(idGrupo, idUsuario.Value);

            // 2. Obtener el grupo desde la API de grupos (usando IGrupoApiClient)
            var grupo = await _grupoApi.GetByIdAsync(idGrupo, idUsuario.Value, token);

            if (grupo is null)
            {
                ModelState.AddModelError(string.Empty, "No se encontró la información del grupo.");
                DesgloseBloqueado = false;

                // Si además no hay rubros, podríamos ofrecer una estructura básica
                if (Rubros.Count == 0)
                {
                    Rubros = new()
                    {
                        new NotaRubro { ID_Grupo = idGrupo, Nombre = "Rubro 1", Porcentaje = 0 },
                        new NotaRubro { ID_Grupo = idGrupo, Nombre = "Rubro 2", Porcentaje = 0 },
                        new NotaRubro { ID_Grupo = idGrupo, Nombre = "Rubro 3", Porcentaje = 0 }
                    };
                }

                return Page();
            }

            // 3. Obtener los estudiantes matriculados en ese curso + grupo
            // (asumo que este servicio no requiere token; si sí lo requiere, se le agrega)
            var estudiantesGrupo = await _estudianteService.GetByCursoYGrupoAsync(grupo.ID_Curso, idGrupo);

            // 4. Verificar si algún estudiante tiene notas ? si sí, bloquear el desglose
            foreach (var est in estudiantesGrupo)
            {
                var notasEst = await _notasService.ObtenerNotasAsync(est.ID_Estudiante, idGrupo, idUsuario.Value);

                if (notasEst.Any())
                {
                    DesgloseBloqueado = true;
                    break;
                }
            }

            // 5. Si no hay rubros y NO está bloqueado ? crear estructura inicial
            if (Rubros.Count == 0 && !DesgloseBloqueado)
            {
                Rubros = new()
                {
                    new NotaRubro { ID_Grupo = idGrupo, Nombre = "Rubro 1", Porcentaje = 0 },
                    new NotaRubro { ID_Grupo = idGrupo, Nombre = "Rubro 2", Porcentaje = 0 },
                    new NotaRubro { ID_Grupo = idGrupo, Nombre = "Rubro 3", Porcentaje = 0 }
                };
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var token = HttpContext.Session.GetString("AccessToken");
            var idUsuario = HttpContext.Session.GetInt32("UsuarioID");

            if (token is null || idUsuario is null)
                return RedirectToPage("/HU_MICHEL/Login/Index");

            // 1. Si está bloqueado, no se permite guardar nada
            if (DesgloseBloqueado)
            {
                ModelState.AddModelError(string.Empty,
                    "El desglose no puede modificarse porque ya existen notas registradas en este grupo.");
                return Page();
            }

            // 2. Validar sumatoria = 100
            if (Rubros.Sum(r => r.Porcentaje) != 100)
            {
                ModelState.AddModelError(string.Empty,
                    "La sumatoria de los porcentajes debe ser exactamente 100.");
                return Page();
            }

            // 3. Guardar el desglose en el backend
            var (ok, msg, _) = await _notasService.CargarDesgloseAsync(
                IdGrupo,
                Rubros,
                idUsuario.Value
            );

            if (!ok)
            {
                var mensajeUsuario = MapearMensajeError(msg);
                ModelState.AddModelError(string.Empty, mensajeUsuario);
                return Page();
            }

            // ? Si todo salió bien ? PRG pattern
            TempData["success"] = "Desglose guardado correctamente.";
            return RedirectToPage(new { idGrupo = IdGrupo });
        }

        private string MapearMensajeError(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return "Ocurrió un error al guardar el desglose.";

            const string negocio = "No se pueden modificar los rubros, ya existen notas asignadas.";

            if (raw.Contains(negocio, StringComparison.OrdinalIgnoreCase))
                return negocio;

            return "Ocurrió un error al guardar el desglose.";
        }


    }
}

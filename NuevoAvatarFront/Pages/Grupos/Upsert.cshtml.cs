using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Grupos
{
    public class UpsertModel : PageModel
    {
        private readonly IGrupoApiClient _grupoApi;

        public UpsertModel(IGrupoApiClient grupoApi)
        {
            _grupoApi = grupoApi;
        }

        [BindProperty]
        public GrupoDto Grupo { get; set; } = new();

        public List<CursoDto> Cursos { get; set; } = new();
        public List<ProfesorDto> Profesores { get; set; } = new();
        public List<PeriodoDto> Periodos { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public bool EsEdicion => Grupo.ID_Grupo > 0;


        public async Task<IActionResult> OnGetAsync(int? id)
        {
            try
            {
                var token = HttpContext.Session.GetString("AccessToken");
                var userId = HttpContext.Session.GetInt32("UsuarioID");

                if (token is null || userId is null)
                    return Redirect("/HU_MICHEL/Login");

                await CargarSelects(token, userId.Value);

                if (id.HasValue)
                {
                    var dto = await _grupoApi.GetByIdAsync(id.Value, userId.Value, token);
                    if (dto is null)
                    {
                        ErrorMessage = "El grupo solicitado no existe.";
                        return Page();
                    }

                    Grupo = dto;
                }

                return Page();
            }
            catch
            {
                ErrorMessage = "Ocurrió un error al cargar la información.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                var token = HttpContext.Session.GetString("AccessToken");
                var userId = HttpContext.Session.GetInt32("UsuarioID");

                if (token is null || userId is null)
                    return Redirect("/HU_MICHEL/Login");


                // Número del grupo
                if (Grupo.Numero <= 0)
                {
                    ErrorMessage = "Debe ingresar un número de grupo válido (mayor a 0).";
                    return await DevolverConSelects(token, userId.Value);
                }

                // Curso
                if (Grupo.ID_Curso <= 0)
                {
                    ErrorMessage = "Debe seleccionar un curso.";
                    return await DevolverConSelects(token, userId.Value);
                }

                // Profesor
                if (Grupo.ID_Profesor <= 0)
                {
                    ErrorMessage = "Debe seleccionar un profesor.";
                    return await DevolverConSelects(token, userId.Value);
                }

                // Periodo
                if (Grupo.ID_Periodo <= 0)
                {
                    ErrorMessage = "Debe seleccionar un período.";
                    return await DevolverConSelects(token, userId.Value);
                }

                // Horario
                if (string.IsNullOrWhiteSpace(Grupo.Horario))
                {
                    ErrorMessage = "Debe especificar un horario.";
                    return await DevolverConSelects(token, userId.Value);
                }

                if (Grupo.Horario.Length > 100)
                {
                    ErrorMessage = "El horario no puede exceder los 100 caracteres.";
                    return await DevolverConSelects(token, userId.Value);
                }

                (bool ok, string? error) response;

                if (!EsEdicion)
                    response = await _grupoApi.CreateAsync(Grupo, userId.Value, token);
                else
                    response = await _grupoApi.UpdateAsync(Grupo, userId.Value, token);


                if (!response.ok)
                {
                    ErrorMessage = ProcesarError(response.error);
                    return await DevolverConSelects(token, userId.Value);
                }

                return RedirectToPage("./Index");
            }
            catch
            {
                ErrorMessage = "Ocurrió un error inesperado al guardar el grupo.";
                return Page();
            }
        }



        private async Task<IActionResult> DevolverConSelects(string token, int userId)
        {
            await CargarSelects(token, userId);
            return Page();
        }

        private string ProcesarError(string? error)
        {
            if (string.IsNullOrWhiteSpace(error))
                return "Ocurrió un error al procesar la solicitud.";

            string lower = error.ToLower();

            if (lower.Contains("existe") && lower.Contains("grupo"))
                return "Ya existe un grupo con este número o combinación.";

            if (lower.Contains("profesor") && lower.Contains("horario"))
                return "Este profesor ya tiene un grupo asignado en ese horario.";

            if (lower.Contains("periodo"))
                return "El período seleccionado no es válido.";

            if (error.Length > 200)
                return "El servidor devolvió un error inesperado.";

            return error; // mensaje legible
        }

        private async Task CargarSelects(string token, int userId)
        {
            Cursos = await _grupoApi.GetCursosAsync(token);
            Profesores = await _grupoApi.GetProfesoresAsync(token);
            Periodos = await _grupoApi.GetPeriodosAsync(userId, token);
        }
    }
}

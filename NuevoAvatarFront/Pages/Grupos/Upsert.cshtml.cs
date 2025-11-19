using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using static NuevoAvatarFront.Pages.Grupos.IndexModel;

namespace NuevoAvatarFront.Pages.Grupos
{
    public class UpsertModel : PageModel
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public UpsertModel(IHttpClientFactory factory, IConfiguration config)
        {
            _http = factory.CreateClient();
            _config = config;
        }

        [BindProperty]
        public GrupoDto Grupo { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public bool EsEdicion => Grupo.ID_Grupo > 0;
        public List<CursoDto> Cursos { get; set; } = new();
        public List<ProfesorDto> Profesores { get; set; } = new();
        public List<PeriodoDto> Periodos { get; set; } = new();

        // Método GET
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            var token = HttpContext.Session.GetString("AccessToken");
            var userId = HttpContext.Session.GetInt32("UsuarioID");

            if (string.IsNullOrWhiteSpace(token) || userId == null)
                return Redirect("/HU_MICHEL/Login");

            await CargarSelects(token);

            if (id.HasValue)
            {
                _http.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);

                var url = $"{_config["ApiOfertaAcademica"]}/grupo/{id}?idUsuarioAccion={userId}";
                var json = await _http.GetStringAsync(url);

                Grupo = JsonSerializer.Deserialize<GrupoDto>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new GrupoDto();
            }

            return Page();
        }

        // Método POST
        public async Task<IActionResult> OnPostAsync()
        {
            var token = HttpContext.Session.GetString("AccessToken");
            var userId = HttpContext.Session.GetInt32("UsuarioID");

            if (string.IsNullOrWhiteSpace(token) || userId == null)
                return Redirect("/HU_MICHEL/Login");

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response;

            if (EsEdicion)
            {
                // PUT - Edición (con ID en la URL)
                response = await _http.PutAsJsonAsync(
                    $"{_config["ApiOfertaAcademica"]}/grupo/{Grupo.ID_Grupo}?idUsuarioAccion={userId}",
                    new
                    {
                        Numero_Grupo = Grupo.Numero,
                        ID_Curso = Grupo.ID_Curso,
                        ID_Profesor = Grupo.ID_Profesor,
                        Horario = Grupo.Horario,
                        ID_Periodo = Grupo.ID_Periodo
                    }
                );
            }
            else
            {
                // POST - Creación
                response = await _http.PostAsJsonAsync(
                    $"{_config["ApiOfertaAcademica"]}/grupo",
                    new
                    {
                        Numero_Grupo = Grupo.Numero,
                        ID_Curso = Grupo.ID_Curso,
                        ID_Profesor = Grupo.ID_Profesor,
                        Horario = Grupo.Horario,
                        ID_Periodo = Grupo.ID_Periodo,
                        idUsuarioAccion = userId
                    }
                );
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ErrorMessage = $"No se pudo guardar el grupo. Error: {response.StatusCode} - {errorContent}";
                await CargarSelects(token);
                return Page();
            }

            return RedirectToPage("./Index");
        }

        // Método para cargar los selectores (cursos, profesores, períodos)
        private async Task CargarSelects(string token)
        {
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            Cursos = await _http.GetFromJsonAsync<List<CursoDto>>($"{_config["ApiOfertaAcademica"]}/curso") ?? new();
            Profesores = await _http.GetFromJsonAsync<List<ProfesorDto>>($"{_config["ApiOfertaAcademica"]}/profesor") ?? new();
            Periodos = await _http.GetFromJsonAsync<List<PeriodoDto>>($"{_config["ApiOfertaAcademica"]}/periodo") ?? new();
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace NuevoAvatarFront.Pages.Grupos
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public IndexModel(IHttpClientFactory factory, IConfiguration config)
        {
            _http = factory.CreateClient();
            _config = config;
        }

        public List<GrupoViewDto> Grupos { get; set; } = new();
        public List<CursoDto> Cursos { get; set; } = new();
        public List<PeriodoDto> Periodos { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? CursoFiltro { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? PeriodoFiltro { get; set; }

        public async Task OnGetAsync()
        {
            var token = HttpContext.Session.GetString("AccessToken");
            var userId = HttpContext.Session.GetInt32("UsuarioID");

            if (string.IsNullOrWhiteSpace(token) || userId == null)
            {
                Response.Redirect("/HU_MICHEL/Login");
                return;
            }

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            // Catálogos
            Cursos = await _http.GetFromJsonAsync<List<CursoDto>>(
                $"{_config["ApiOfertaAcademica"]}/curso") ?? new();

            Periodos = await _http.GetFromJsonAsync<List<PeriodoDto>>(
                $"{_config["ApiOfertaAcademica"]}/periodo?idUsuarioAccion={userId}") ?? new();

            var grupos = await _http.GetFromJsonAsync<List<GrupoDto>>(
                $"{_config["ApiOfertaAcademica"]}/grupo") ?? new();

            var profesores = await _http.GetFromJsonAsync<List<ProfesorDto>>(
                $"{_config["ApiOfertaAcademica"]}/profesor") ?? new();

            // JOIN + FILTROS
            foreach (var g in grupos)
            {
                if (CursoFiltro.HasValue && g.ID_Curso != CursoFiltro.Value)
                    continue;

                if (PeriodoFiltro.HasValue && g.ID_Periodo != PeriodoFiltro.Value)
                    continue;

                Grupos.Add(new GrupoViewDto
                {
                    ID_Grupo = g.ID_Grupo,
                    Numero = g.Numero,
                    Curso = Cursos.FirstOrDefault(c => c.ID_Curso == g.ID_Curso)?.Nombre ?? "",
                    Profesor = profesores.FirstOrDefault(p => p.ID_Profesor == g.ID_Profesor)?.Nombre ?? "",
                    Periodo = Periodos.FirstOrDefault(p => p.ID_Periodo == g.ID_Periodo)?.Nombre ?? "",
                    Horario = g.Horario
                });
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var token = HttpContext.Session.GetString("AccessToken");
            var userId = HttpContext.Session.GetInt32("UsuarioID");

            if (string.IsNullOrWhiteSpace(token) || userId == null)
                return Redirect("/HU_MICHEL/Login");

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var url = $"{_config["ApiOfertaAcademica"]}/grupo/{id}?idUsuarioAccion={userId}";
            var response = await _http.DeleteAsync(url);

            return RedirectToPage(new { CursoFiltro, PeriodoFiltro });
        }

        // DTOs ───────────────────────────────────────────────────────────

        public class GrupoDto
        {
            public int ID_Grupo { get; set; }

            [JsonPropertyName("numero_Grupo")]
            public int Numero { get; set; }

            public int ID_Curso { get; set; }
            public int ID_Profesor { get; set; }
            public int ID_Periodo { get; set; }
            public string Horario { get; set; } = "";
        }


        public class CursoDto
        {
            public int ID_Curso { get; set; }
            public string Nombre { get; set; } = "";
        }

        public class ProfesorDto
        {
            public int ID_Profesor { get; set; }
            public string Nombre { get; set; } = "";
        }

        public class PeriodoDto
        {
            public int ID_Periodo { get; set; }
            public int Anno { get; set; }
            public int Numero { get; set; }

            public string Nombre => $"{Anno}-{Numero}";
        }

        public class GrupoViewDto
        {
            public int ID_Grupo { get; set; }
            public int Numero { get; set; }
            public string Curso { get; set; } = "";
            public string Profesor { get; set; } = "";
            public string Periodo { get; set; } = "";
            public string Horario { get; set; } = "";
        }
    }
}

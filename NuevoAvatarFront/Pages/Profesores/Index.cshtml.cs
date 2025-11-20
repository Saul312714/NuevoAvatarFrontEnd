using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Infra;           
using NuevoAvatarFront.Infra.NuevoAvatarFront.Infra;
using NuevoAvatarFront.Models;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.Profesores
{
    public class IndexModel : PageModel
    {
        private readonly ProfesorClient _api;
        private readonly TokenHelper _tokenHelper;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ProfesorClient api, TokenHelper tokenHelper, ILogger<IndexModel> logger)
        {
            _api = api;
            _tokenHelper = tokenHelper;
            _logger = logger;
        }

        public List<ProfesorDto> Profesores { get; set; } = new();
        public int Total { get; set; }

        [BindProperty(SupportsGet = true)] public string? q { get; set; }
        [BindProperty(SupportsGet = true)] public string? sort { get; set; }
        [BindProperty(SupportsGet = true)] public int page { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public int size { get; set; } = 10;

        public List<SelectListItem> SortOptions { get; } = new()
        {
            new("Orden: ID", ""),
            new("Nombre", "nombre"),
            new("Email",  "email")
        };

        public List<SelectListItem> SizeOptions { get; } = new()
        {
            new("10", "10"),
            new("25", "25"),
            new("50", "50")
        };

        public async Task<IActionResult> OnGet()
        {
            var token = await _tokenHelper.EnsureValidAccessTokenAsync();
            if (token == null)
            {
                return RedirectToPage("/HU_MICHEL/Login/Index");
            }

            page = page <= 0 ? 1 : page;
            size = size <= 0 ? 10 : size;

            try
            {
                var (datosApi, totalApi) = await _api.GetAllAsync(q, sort);

                IEnumerable<ProfesorDto> query = datosApi;

                if (!string.IsNullOrWhiteSpace(q))
                {
                    var term = q.Trim();
                    query = query.Where(p =>
                           (p.Nombre?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
                        || (p.Email?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
                        || (p.Identificacion?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false));
                }

                query = sort switch
                {
                    "nombre" => query.OrderBy(p => p.Nombre),
                    "email" => query.OrderBy(p => p.Email),
                    _ => query.OrderBy(p => p.ID_Profesor)
                };

                Total = query.Count();

                Profesores = query
                    .Skip((page - 1) * size)
                    .Take(size)
                    .ToList();

                foreach (var it in SortOptions)
                    it.Selected = string.Equals(it.Value, sort ?? "", StringComparison.OrdinalIgnoreCase);

                foreach (var it in SizeOptions)
                    it.Selected = it.Value == size.ToString();

                return Page();
            }
            catch (ApiException apiEx)
            {
                _logger.LogWarning(apiEx, "Error de API al listar profesores");

                TempData["error"] = apiEx.Message;

                Profesores = new();
                Total = 0;
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al cargar listado de profesores");
                TempData["error"] = "Error al cargar el listado de profesores. Intente nuevamente más tarde.";
                Profesores = new();
                Total = 0;
                return Page();
            }
        }
    }
}


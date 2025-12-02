using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.Entities;
using NuevoAvatarFront.Services;

namespace NuevoAvatarFront.Pages.ADM23_Bitacora
{
    public class IndexModel : PageModel
    {
        private readonly IBitacoraApiClient _bitacoraApi;

        public IndexModel(IBitacoraApiClient bitacoraApi)
        {
            _bitacoraApi = bitacoraApi;
        }

        // ========= Seguridad: solo administradores =========
        private bool EsAdmin()
        {
            var rol = HttpContext.Session.GetString("UserRole");
            return rol != null && rol.Equals("Administrador", StringComparison.OrdinalIgnoreCase);
        }

        // ============== Filtros (GET) ======================
        [BindProperty(SupportsGet = true)]
        public DateTime? FechaDesde { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FechaHasta { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? UsuarioFiltro { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? AccionFiltro { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ModuloFiltro { get; set; }

        [BindProperty(SupportsGet = true)]
        public int Pagina { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public int TamanoPagina { get; set; } = 20;

        [BindProperty(SupportsGet = true)]
        public string? Orden { get; set; } = "fecha_desc"; // más recientes primero

        // Datos para la vista
        public List<BitacoraDto> Registros { get; set; } = new();
        public int TotalRegistros { get; set; }
        public int TotalPaginas => TamanoPagina <= 0
            ? 1
            : (int)Math.Ceiling((double)TotalRegistros / TamanoPagina);

        public string? MensajeError { get; set; }

        // ===========================
        // GET
        // ===========================
        public async Task<IActionResult> OnGetAsync()
        {
            if (!EsAdmin())
            {
                return RedirectToPage("/HU_MICHEL/Login/Index");
            }

            if (Pagina <= 0) Pagina = 1;
            if (TamanoPagina <= 0) TamanoPagina = 20;

            var (ok, _, data, msg) = await _bitacoraApi.ListarAsync();

            if (!ok || data is null)
            {
                MensajeError = msg ?? "No se pudieron cargar los registros de bitácora.";
                return Page();
            }

            // Filtros en memoria
            var query = data.AsQueryable();

            if (FechaDesde.HasValue)
            {
                var f = FechaDesde.Value.Date;
                query = query.Where(b => b.Fecha.Date >= f);
            }

            if (FechaHasta.HasValue)
            {
                var f = FechaHasta.Value.Date;
                query = query.Where(b => b.Fecha.Date <= f);
            }

            if (!string.IsNullOrWhiteSpace(UsuarioFiltro))
            {
                // Si escribe número, filtra por ID_Usuario
                if (int.TryParse(UsuarioFiltro, out var idUser))
                {
                    query = query.Where(b => b.ID_Usuario == idUser);
                }
                else
                {
                    query = query.Where(b =>
                        (b.UsuarioNombre ?? string.Empty)
                        .Contains(UsuarioFiltro, StringComparison.OrdinalIgnoreCase));
                }
            }

            if (!string.IsNullOrWhiteSpace(AccionFiltro))
            {
                query = query.Where(b =>
                    (b.Accion ?? string.Empty)
                    .Contains(AccionFiltro, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(ModuloFiltro))
            {
                query = query.Where(b =>
                    (b.Modulo ?? string.Empty)
                    .Contains(ModuloFiltro, StringComparison.OrdinalIgnoreCase));
            }

            // Orden
            query = Orden switch
            {
                "fecha_asc" => query.OrderBy(b => b.Fecha),
                _ => query.OrderByDescending(b => b.Fecha)
            };

            TotalRegistros = query.Count();

            // Paginación
            Registros = query
                .Skip((Pagina - 1) * TamanoPagina)
                .Take(TamanoPagina)
                .ToList();

            return Page();
        }
    }
}

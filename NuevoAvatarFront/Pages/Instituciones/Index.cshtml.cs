using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NuevoAvatarFront.DTOs;
using NuevoAvatarFront.Services;
namespace NuevoAvatarFront.Pages.Instituciones
{
    public class IndexModel : PageModel
    {
        private readonly IInstitucionApiClient _client;

        public IndexModel(IInstitucionApiClient client)
        {
            _client = client;
        }

        [BindProperty(SupportsGet = true)] public string? q { get; set; }
        public List<InstitucionDto> Instituciones { get; set; } = [];

        /*
        public async Task OnGetAsync()
        {
            // Llamada a la API para obtener las instituciones, asegurándonos de que el ID sea parte de la respuesta
            Instituciones = await _client.GetAllAsync(q);
        }*/
        public async Task OnGetAsync()
        {
            Instituciones = await _client.GetAllAsync(q);

            // Depuración: Verifica que los IDs estén presentes
            Console.WriteLine($"Instituciones: {string.Join(", ", Instituciones.Select(i => i.Id))}");
        }
    }
}
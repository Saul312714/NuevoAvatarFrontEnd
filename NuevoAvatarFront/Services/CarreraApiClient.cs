using NuevoAvatarFront.DTOs;
using NuevoAvatarFront.Services;

public class CarreraApiClient : ICarreraApiClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _context;

    public CarreraApiClient(HttpClient http, IHttpContextAccessor context)
    {
        _http = http;
        _context = context;
    }

    private void AddToken()
    {
        var token = _context.HttpContext?.Session.GetString("AccessToken");
        if (!string.IsNullOrEmpty(token))
            _http.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<List<CarreraDto>> GetByInstitucionAsync(int institucionId, string? search)
    {
        AddToken();
        var url = $"carrera?institucionId={institucionId}";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&q={Uri.EscapeDataString(search)}";

        var resp = await _http.GetAsync(url);
        if (!resp.IsSuccessStatusCode)
            return new();

        return await resp.Content.ReadFromJsonAsync<List<CarreraDto>>() ?? new();
    }

    public async Task<CarreraDto?> GetByIdAsync(int id)
    {
        AddToken();
        return await _http.GetFromJsonAsync<CarreraDto>($"carrera/{id}");
    }

    public async Task<bool> CreateAsync(CarreraCreateUpdateDto dto)
    {
        AddToken();
        var resp = await _http.PostAsJsonAsync("carrera", dto);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAsync(int id, CarreraCreateUpdateDto dto)
    {
        AddToken();
        var resp = await _http.PutAsJsonAsync($"carrera/{id}", dto);
        return resp.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        AddToken();
        var resp = await _http.DeleteAsync($"carrera/{id}");
        // Si hay cursos vinculados, el backend puede devolver 409 (Conflict)
        return resp.IsSuccessStatusCode;
    }

    public async Task<List<CarreraDto>> GetAllAsync()
    {
        AddToken();
        var resp = await _http.GetAsync("carrera");
        if (!resp.IsSuccessStatusCode)
            return new();

        return await resp.Content.ReadFromJsonAsync<List<CarreraDto>>() ?? new();
    }
}
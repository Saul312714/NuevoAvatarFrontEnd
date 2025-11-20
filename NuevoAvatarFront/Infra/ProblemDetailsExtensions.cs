using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace NuevoAvatarFront.Infra
{
    public static class ProblemDetailsExtensions
    {
        public static async Task<string> ReadErrorAsync(this HttpResponseMessage resp)
        {
            try
            {
                var pd = await resp.Content.ReadFromJsonAsync<ProblemDetails>();
                if (pd is not null) return pd.Detail ?? pd.Title ?? resp.ReasonPhrase ?? "Error";
                var raw = await resp.Content.ReadAsStringAsync();
                return string.IsNullOrWhiteSpace(raw) ? $"{(int)resp.StatusCode} {resp.ReasonPhrase}" : raw;
            }
            catch
            {
                return $"{(int)resp.StatusCode} {resp.ReasonPhrase}";
            }
        }
    }
}

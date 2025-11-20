using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NuevoAvatarFront.Helpers;
using NuevoAvatarFront.Infra.NuevoAvatarFront.Infra;

namespace NuevoAvatarFront.Infra
{
    public abstract class ApiClientBase
    {
        protected readonly IHttpContextAccessor _httpContextAccessor;
        protected readonly TokenHelper _tokenHelper;

        protected ApiClientBase(IHttpContextAccessor httpContextAccessor, TokenHelper tokenHelper)
        {
            _httpContextAccessor = httpContextAccessor;
            _tokenHelper = tokenHelper;
        }

        protected async Task AttachBearerAsync(HttpClient client)
        {
            var token = await _tokenHelper.EnsureValidAccessTokenAsync();

            if (string.IsNullOrWhiteSpace(token))
            {
                Console.WriteLine("No se encontró un AccessToken válido en sesión.");
                return;
            }

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private sealed class ProblemDetailsLike
        {
            public string? Title { get; set; }
            public string? Detail { get; set; }
            public Dictionary<string, string[]>? Errors { get; set; }
        }

        protected static async Task EnsureSuccessOrThrowAsync(
            HttpResponseMessage response,
            ILogger logger,
            string fallbackMessage = "Ocurrió un error al comunicarse con el servidor.")
        {
            if (response.IsSuccessStatusCode)
                return;

            string body = await response.Content.ReadAsStringAsync();

            string userMessage = fallbackMessage;
            var errorList = new List<string>();

            try
            {
                var problem = JsonSerializer.Deserialize<ProblemDetailsLike>(body, _jsonOptions);

                if (problem != null)
                {
                    if (!string.IsNullOrWhiteSpace(problem.Detail))
                        userMessage = problem.Detail;
                    else if (!string.IsNullOrWhiteSpace(problem.Title))
                        userMessage = problem.Title;

                    if (problem.Errors != null)
                        errorList.AddRange(problem.Errors.SelectMany(kv => kv.Value));
                }
            }
            catch
            {
                if (!string.IsNullOrWhiteSpace(body))
                    userMessage = body;
            }

            logger.LogWarning(
                "API error {StatusCode}: {Message}. Body: {Body}",
                (int)response.StatusCode,
                userMessage,
                body
            );

            throw new ApiException(userMessage, response.StatusCode, errorList);
        }
    }
}


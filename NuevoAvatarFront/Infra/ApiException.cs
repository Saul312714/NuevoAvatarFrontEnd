namespace NuevoAvatarFront.Infra
{
    using System.Net;

    namespace NuevoAvatarFront.Infra
    {
        public class ApiException : Exception
        {
            public HttpStatusCode StatusCode { get; }
            public List<string> Errors { get; } = new();

            public ApiException(string message,
                                HttpStatusCode statusCode,
                                IEnumerable<string>? errors = null)
                : base(message)
            {
                StatusCode = statusCode;
                if (errors != null)
                    Errors.AddRange(errors);
            }
        }
    }
}

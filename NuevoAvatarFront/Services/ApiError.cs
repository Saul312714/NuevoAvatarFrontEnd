namespace NuevoAvatarFront.Services
{
    public class ApiError
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = "Error desconocido";
        public string? Details { get; set; }
    }
}


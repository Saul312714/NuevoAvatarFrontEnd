namespace NuevoAvatarFront.DTOs
{
    public class ProfesorDto
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Identificacion { get; set; }
        public string? Correo { get; set; }
        public bool Activo { get; set; } = true;
    }
}

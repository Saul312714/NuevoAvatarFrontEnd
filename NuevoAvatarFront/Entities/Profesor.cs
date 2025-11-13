namespace NuevoAvatarFront.Entities
{
    public class Profesor
    {
        public int Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Identificacion { get; set; }
        public string? Correo { get; set; }
        public bool Activo { get; set; } = true;
    }
}

namespace NuevoAvatarFront.DTOs
{
    public class ProfesorDto
    {
        public int ID_Profesor { get; set; }
        public string Identificacion { get; set; } = string.Empty;
        public string Tipo_Identificacion { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public DateTime Fecha_Nacimiento { get; set; }
    }
}

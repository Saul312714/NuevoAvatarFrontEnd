namespace NuevoAvatarFront.Entities
{
    // DTO recibido desde la API
    public class ExpedienteDto
    {
        public int ID_Estudiante { get; set; }
        public string Identificacion { get; set; } = "";
        public string Tipo_Identificacion { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Email { get; set; } = "";
        public DateTime Fecha_Nacimiento { get; set; }
        public string Telefono { get; set; } = "";
        public string Direccion { get; set; } = "";  // "provincia - canton - distrito"
    }
}

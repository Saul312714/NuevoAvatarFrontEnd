namespace NuevoAvatarFront.Entities 
{ 
    public class ExpedienteViewDto 
    { 
        public int ID_Estudiante { get; set; } 
        public string Identificacion { get; set; } = ""; 
        public string Tipo_Identificacion { get; set; } = ""; 
        public string Nombre { get; set; } = ""; 
        public string Email { get; set; } = ""; 
        public DateTime Fecha_Nacimiento { get; set; } 
        public string Telefono { get; set; } = ""; 
        public string Provincia { get; set; } = ""; 
        public string Canton { get; set; } = ""; 
        public string Distrito { get; set; } = ""; } 
}
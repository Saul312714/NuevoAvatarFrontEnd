namespace NuevoAvatarFront.Models
{
    public class MatriculadoDto
    {
        public int ID_Estudiante { get; set; }
        public string Identificacion { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int ID_Curso { get; set; }
        public int ID_Grupo { get; set; }
        public int ID_Periodo { get; set; }
    }
}

namespace NuevoAvatarFront.Models
{
    public class Adm19ListadoRowDto
    {
        public int ID_Matricula { get; set; }

        public int ID_Estudiante { get; set; }
        public string Tipo_Identificacion { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;

        public int ID_Carrera { get; set; }
        public string Carrera { get; set; } = string.Empty;

        public int ID_Curso { get; set; }
        public string Curso { get; set; } = string.Empty;

        public int ID_Grupo { get; set; }
        public int Numero_Grupo { get; set; }

        public int ID_Periodo { get; set; }
    }
}

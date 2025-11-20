namespace NuevoAvatarFront.Entities
{
    public class GrupoViewDto
    {
        public int ID_Grupo { get; set; }
        public int Numero { get; set; }

        public int ID_Curso { get; set; }
        public string NombreCurso { get; set; } = "";

        public int ID_Profesor { get; set; }
        public string NombreProfesor { get; set; } = "";

        public int ID_Periodo { get; set; }
        public string NombrePeriodo { get; set; } = "";

        public string Horario { get; set; } = "";
    }
}

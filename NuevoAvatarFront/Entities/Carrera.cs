namespace NuevoAvatarFront.Entities
{
    
    public class Carrera
    {
        public int ID_Carrera { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int ID_Institucion { get; set; }
        public int ID_ProfesorDirector { get; set; }
        public string NombreInstitucion { get; set; } = string.Empty;
        public string NombreDirector { get; set; } = string.Empty;
    }
}


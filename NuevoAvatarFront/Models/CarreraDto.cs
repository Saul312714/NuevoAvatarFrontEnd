namespace NuevoAvatarFront.Models
{
    public class CarreraDto
    {
        public int ID_Carrera { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int ID_Institucion { get; set; }
        public int ID_Profesor_Director { get; set; }
    }
}

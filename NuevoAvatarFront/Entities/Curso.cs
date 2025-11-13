namespace PV_NA_OfertaAcademica.Entities
{
    public class Curso
    {
        public int ID_Curso { get; set; }

        // FK a Carrera
        public int ID_Carrera { get; set; }

        // Nivel entre 1 y 12 (validado en Service/DB)
        public int Nivel { get; set; }

        // Nombre del curso
        public string Nombre { get; set; } = string.Empty;
    }
}

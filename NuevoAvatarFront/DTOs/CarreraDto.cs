namespace NuevoAvatarFront.DTOs
{
    public class CarreraDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public int InstitucionId { get; set; }
        public string InstitucionNombre { get; set; } = "";
        public int DirectorId { get; set; }              // id del profesor
        public string DirectorNombre { get; set; } = "";
        public int CursosVinculados { get; set; }        // para mostrar si bloquea eliminación
    }
}

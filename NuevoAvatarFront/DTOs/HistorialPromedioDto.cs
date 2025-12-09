namespace NuevoAvatarFront.DTOs
{
    public class HistorialPromedioDto
    {
        public string CodigoCurso { get; set; } = string.Empty;
        public string NombreCurso { get; set; } = string.Empty;
        public decimal Promedio { get; set; }
        public string Periodo { get; set; } = string.Empty;
    }
}

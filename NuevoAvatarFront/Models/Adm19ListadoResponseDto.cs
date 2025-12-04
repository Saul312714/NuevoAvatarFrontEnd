namespace NuevoAvatarFront.Models
{
    public class Adm19ListadoResponseDto
    {
        public int total { get; set; }
        public int page { get; set; }
        public int size { get; set; }
        public List<Adm19ListadoRowDto> datos { get; set; } = new();
    }
}

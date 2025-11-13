namespace NuevoAvatarFront.DTOs
{
    public class CursoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Nivel { get; set; }
        public int CarreraId { get; set; }
        public string CarreraNombre { get; set; } = string.Empty;
        public int GruposVinculados { get; set; } // Para verificar la relación
    }
}

namespace NuevoAvatarFront.DTOs
{
    public class CursoDto
    {
        public int ID_Curso { get; set; }

        public int ID_Carrera { get; set; }

        // Nombre de la carrera para la grilla (lo trae el backend con un JOIN)
        public string NombreCarrera { get; set; } = string.Empty;

        public int Nivel { get; set; }

        public string Nombre { get; set; } = string.Empty;
    }
}

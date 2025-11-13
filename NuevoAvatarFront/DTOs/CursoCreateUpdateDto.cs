using System.ComponentModel.DataAnnotations;

namespace NuevoAvatarFront.DTOs
{
    public class CursoCreateUpdateDto
    {
        public int? ID_Curso { get; set; } // null = crear
        public int ID_Carrera { get; set; }
        public int Nivel { get; set; }
        public string Nombre { get; set; } = string.Empty;
    }
}

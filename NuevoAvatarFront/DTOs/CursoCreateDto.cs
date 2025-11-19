using System.ComponentModel.DataAnnotations;
namespace NuevoAvatarFront.DTOs
{
    public class CursoCreateDto
    {
        public string Nombre { get; set; } = string.Empty;
        public int Nivel { get; set; }
        public int ID_Carrera { get; set; }
    }
}

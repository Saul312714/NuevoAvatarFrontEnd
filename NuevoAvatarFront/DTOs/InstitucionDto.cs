using System.ComponentModel.DataAnnotations;

namespace NuevoAvatarFront.DTOs
{
    //VALIDACIONES LISTAS ADM8
    public class InstitucionDto
    {
        // 
        [Required(ErrorMessage = "El ID es obligatorio.")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID debe ser mayor a 0.")]
        public int ID_Institucion { get; set; }

        // 
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [RegularExpression("^[a-zA-ZÀ-ÿ\\s]+$", ErrorMessage = "Solo se permiten letras y espacios.")]
        public string Nombre { get; set; } = string.Empty;
    }
}

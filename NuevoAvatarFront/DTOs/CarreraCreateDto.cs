using System.ComponentModel.DataAnnotations;

namespace NuevoAvatarFront.DTOs
{
    public class CarreraCreateDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre solo permite letras y espacios.")]
        public string Nombre { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una institución válida.")]
        public int ID_Institucion { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un director válido.")]
        public int ID_Profesor_Director { get; set; }
    }
}
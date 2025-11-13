using System.ComponentModel.DataAnnotations;

namespace NuevoAvatarFront.DTOs
{

    public class CarreraCreateUpdateDto
    {
        [Required, RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ ]+$",
            ErrorMessage = "Solo letras y espacios.")]
        public string Nombre { get; set; } = "";

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione institución")]
        public int InstitucionId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione director")]
        public int DirectorId { get; set; }
    }
}

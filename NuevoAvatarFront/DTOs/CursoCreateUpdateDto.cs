using System.ComponentModel.DataAnnotations;

namespace NuevoAvatarFront.DTOs
{
    public class CursoCreateUpdateDto
    {
        [Required, RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ ]+$", ErrorMessage = "El nombre solo puede contener letras y espacios.")]
        public string Nombre { get; set; } = string.Empty;

        [Range(1, 12, ErrorMessage = "El nivel debe estar entre 1 y 12.")]
        public int Nivel { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Seleccione una carrera.")]
        public int CarreraId { get; set; }
    }
}

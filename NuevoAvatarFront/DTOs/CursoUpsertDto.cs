using System.ComponentModel.DataAnnotations;

namespace NuevoAvatarFront.DTOs
{
    public class CursoUpsertDto
    {
        // 0 cuando es nuevo; >0 cuando se está editando
        public int ID_Curso { get; set; }

        [Required(ErrorMessage = "La carrera es requerida.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una carrera válida.")]
        public int ID_Carrera { get; set; }

        [Required(ErrorMessage = "El nivel es requerido.")]
        [Range(1, 12, ErrorMessage = "El nivel debe estar entre 1 y 12.")]
        public int Nivel { get; set; }

        [Required(ErrorMessage = "El nombre es requerido.")]
        [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ ]+$",
            ErrorMessage = "El nombre solo puede contener letras y espacios.")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
        public string Nombre { get; set; } = string.Empty;
    }
}

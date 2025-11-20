using System.ComponentModel.DataAnnotations;

namespace NuevoAvatarFront.Entities
{
    public class ModuloDto
    {
        public int ID_Modulo { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [RegularExpression("^[a-zA-ZáéíóúÁÉÍÓÚñÑ ]+$", ErrorMessage = "Solo se permiten letras y espacios.")]
        public string Nombre { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;

        [Range(1, int.MaxValue, ErrorMessage = "El orden debe ser 1 o mayor.")]
        public int Orden { get; set; } = 1;

        public string Ruta { get; set; } = string.Empty;
        public string Icono { get; set; } = "fas fa-folder";
    }
}

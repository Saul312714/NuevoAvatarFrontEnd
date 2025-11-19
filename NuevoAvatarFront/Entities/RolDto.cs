using System.ComponentModel.DataAnnotations;

public class RolDto
{
    public int ID_Rol { get; set; }

    [Required(ErrorMessage = "El nombre es requerido.")]
    [RegularExpression(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ ]+$",
        ErrorMessage = "El nombre solo puede contener letras y espacios.")]
    public string Nombre { get; set; } = string.Empty;
}

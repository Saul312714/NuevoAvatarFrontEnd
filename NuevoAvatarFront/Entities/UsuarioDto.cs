namespace NuevoAvatarFront.Entities
{
    public class UsuarioDto
    {
        public int ID_Usuario { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Tipo_Identificacion { get; set; } = string.Empty;
        public string Identificacion { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string RolNombre { get; set; } = string.Empty;
    }
}

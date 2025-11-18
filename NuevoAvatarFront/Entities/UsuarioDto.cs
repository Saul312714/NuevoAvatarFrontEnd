using System.Text.Json.Serialization;

namespace NuevoAvatarFront.Entities
{
    public class UsuarioDto
    {
        [JsonPropertyName("iD_Usuario")]
        public int ID_Usuario { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("tipo_Identificacion")]
        public string? Tipo_Identificacion { get; set; }

        [JsonPropertyName("identificacion")]
        public string? Identificacion { get; set; }

        [JsonPropertyName("nombre")]
        public string? Nombre { get; set; }

        [JsonPropertyName("contrasena")]
        public string? Contrasena { get; set; }

        [JsonPropertyName("iD_Rol")]
        public int ID_Rol { get; set; }

        [JsonPropertyName("rolNombre")]
        public string? RolNombre { get; set; }
    }
}



using NuevoAvatarFront.Entities;

namespace NuevoAvatarFront.DTOs
{
    public class CargarDesglose
    {
        public int IdGrupo { get; set; }
        public List<NotaRubro> Rubros { get; set; } = new();
    }
}

using System;

namespace NuevoAvatarFront.Entities
{
    public class BitacoraDto
    {
        public int ID_Bitacora { get; set; }

        public DateTime Fecha { get; set; }      // ajusta el nombre si en tu API se llama distinto

        public int ID_Usuario { get; set; }

        public string? UsuarioNombre { get; set; } // si tu API no lo trae, esto quedará null

        public string? Modulo { get; set; }

        public string? Accion { get; set; }

        public string? Descripcion { get; set; }

        public string? JsonAntes { get; set; }

        public string? JsonDespues { get; set; }
    }
}

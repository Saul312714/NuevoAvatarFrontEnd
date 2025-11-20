using System;

namespace NuevoAvatarFront.Models
{
    public class PeriodoDto
    {
        public int ID_Periodo { get; set; }
        public int Anno { get; set; }
        public int Numero { get; set; }
        public DateTime Fecha_Inicio { get; set; }
        public DateTime Fecha_Fin { get; set; }
        public string NombreMostrar =>
            $"{Anno} - Período {Numero} (Inicio: {Fecha_Inicio:dd/MM/yyyy})";
    }
}

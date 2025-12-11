namespace NuevoAvatarFront.Entities
{
    public class FacturaDto
    {
        public int ID_Factura { get; set; }
        public int ID_Estudiante { get; set; }
        public decimal Monto { get; set; }
        public decimal Impuesto { get; set; }
        public string Estado { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }

        public decimal Total => Monto + Impuesto;
    }
}

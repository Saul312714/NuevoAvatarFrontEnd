namespace NuevoAvatarFront.Entities
{
    public class PeriodoDto
    {
        public int ID_Periodo { get; set; }
        public int Anno { get; set; }
        public int Numero { get; set; }

        public string Nombre => $"{Anno}-{Numero}";
    }
}

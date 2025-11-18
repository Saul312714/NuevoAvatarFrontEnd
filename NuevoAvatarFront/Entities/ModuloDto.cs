namespace NuevoAvatarFront.Entities
{
    public class ModuloDto
    {
        public int ID_Modulo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;

        public string Ruta { get; set; } = string.Empty;

        public string Icono { get; set; } = "fas fa-folder";
    }
}

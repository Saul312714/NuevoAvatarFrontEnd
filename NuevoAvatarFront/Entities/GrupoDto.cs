using System.Text.Json.Serialization;

namespace NuevoAvatarFront.Entities
{
    public class GrupoDto
    {
        public int ID_Grupo { get; set; }

        [JsonPropertyName("numero_Grupo")]
        public int Numero { get; set; }

        public int ID_Curso { get; set; }
        public int ID_Profesor { get; set; }
        public int ID_Periodo { get; set; }
        public string Horario { get; set; } = "";
        public string NombreCurso { get; set; }
        public string NombreProfesor { get; set; }
        public string NombrePeriodo { get; set; }

    }
}

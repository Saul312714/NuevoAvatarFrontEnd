using System.ComponentModel.DataAnnotations;

namespace NuevoAvatarFront.Models
{
    public class PreMatriculaDTO
    {
        public int ID_Prematricula { get; set; }

        [Required(ErrorMessage = "El estudiante es requerido")]
        [Display(Name = "Estudiante")]
        public int ID_Estudiante { get; set; }

        [Required(ErrorMessage = "La carrera es requerida")]
        [Display(Name = "Carrera")]
        public int ID_Carrera { get; set; }

        [Required(ErrorMessage = "El curso es requerido")]
        [Display(Name = "Curso")]
        public int ID_Curso { get; set; }

        [Display(Name = "Observaciones")]
        [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
        public string? Observaciones { get; set; }

        [Required(ErrorMessage = "El periodo es requerido")]
        [Display(Name = "Periodo")]
        public int ID_Periodo { get; set; }

        // Propiedades adicionales para mostrar información relacionada
        public string? NombreEstudiante { get; set; }
        public string? NombreCarrera { get; set; }
        public string? NombreCurso { get; set; }
        public string? NombrePeriodo { get; set; }
    }

    public class PreMatriculaFiltroDTO
    {
        public int? ID_Periodo { get; set; }
        public int? ID_Carrera { get; set; }
        public int? ID_Curso { get; set; }
        public int? ID_Estudiante { get; set; }
    }
}
namespace NuevoAvatarFront.Entities.Dashboard
{
    public class DashboardData
    {
        // Usuario
        public string NombreUsuario { get; set; } = "";
        public string RolUsuario { get; set; } = "";
        public int IdUsuario { get; set; }

        public List<UsuarioSimpleDto> Usuarios { get; set; } = new();
        public Dictionary<int, string> UsuariosLookup { get; set; } = new();


        // ADMIN
        public int TotalUsuarios { get; set; }
        public int TotalCarreras { get; set; }
        public int TotalCursos { get; set; }
        public int TotalPeriodosActivos { get; set; }
        public int TotalFacturasPendientes { get; set; }

        // ESTUDIANTE
        public int MisMatriculas { get; set; }
        public int MisCursosActuales { get; set; }
        public decimal MiSaldoPendiente { get; set; }
        public string MiCarrera { get; set; } = "";

        // PROFESOR
        public int MisGrupos { get; set; }
        public int CursosQueImparto { get; set; }

        public List<GrupoProfesorDto> MisGruposDetalle { get; set; } = new();
        public List<NotificacionDto> Notificaciones { get; set; } = new();
        public List<BitacoraDto> Bitacoras { get; set; } = new();
    }

    public class GrupoProfesorDto
    {
        public int ID_Grupo { get; set; }
        public string NombreCurso { get; set; } = "";
        public string Horario { get; set; } = "";
    }

    public class NotificacionDto
    {
        public int ID_Notificacion { get; set; }
        public string Email_Destino { get; set; } = "";
        public string Asunto { get; set; } = "";
        public string Cuerpo { get; set; } = "";
        public DateTime FechaEnvio { get; set; }
        public string Estado { get; set; } = "";
    }

    public class BitacoraDto
    {
        public int ID_Bitacora { get; set; }
        public int ID_Usuario { get; set; }
        public string Accion { get; set; } = "";
        public DateTime Fecha { get; set; }
    }


}

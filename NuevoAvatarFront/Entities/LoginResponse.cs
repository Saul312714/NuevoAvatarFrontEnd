namespace NuevoAvatarFront.Entities
{
    public class LoginResponse
    {
        public string Expires_In { get; set; } = string.Empty;
        public string Access_Token { get; set; } = string.Empty;
        public string Refresh_Token { get; set; } = string.Empty;
        public int UsuarioID { get; set; }
    }
}

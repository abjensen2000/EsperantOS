namespace EsperantOS.Models
{
    // ViewModel til login-siden (Account/Login).
    // Indeholder de felter som brugeren udfylder i loginformularen.
    // Formularen binder automatisk til denne klasse når der postes.
    public class LoginViewModel
    {
        // Det brugernavn som er indtastet i loginformularen
        public string Username { get; set; } = string.Empty;

        // Det kodeord som er indtastet i loginformularen
        public string Password { get; set; } = string.Empty;
    }
}

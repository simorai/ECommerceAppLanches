using AppLanches.Pages;
using AppLanches.Services;
using AppLanches.Validations;

namespace AppLanches
{
    public partial class App : Application
    {
        private readonly ApiService _apiService;

        private readonly IValidator _validator;

        public App(ApiService apiService, IValidator validator)
        {
            InitializeComponent();
            _apiService = apiService;
            _validator = validator;

            SetMainPage();
        }

        private void SetMainPage()
        {
            // Lê o token e o ID do usuário das Preferences
            var accessToken = Preferences.Get("accesstoken", string.Empty);
            var userId = Preferences.Get("usuarioid", 0);
            var userName = Preferences.Get("usuarionome", string.Empty);

            Console.WriteLine($"[DEBUG] SetMainPage - Token: {accessToken}");
            Console.WriteLine($"[DEBUG] SetMainPage - UserId: {userId}");
            Console.WriteLine($"[DEBUG] SetMainPage - UserName: {userName}");

            // Se não houver token ou o ID for inválido, vai para o login
            if (string.IsNullOrEmpty(accessToken) || userId <= 0)
            {
                MainPage = new NavigationPage(new LoginPage(_apiService, _validator));
                return;
            }

            // Usuário autenticado, vai para o AppShell
            MainPage = new AppShell(_apiService, _validator);
        }
    }
}

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
            var accessToken = Preferences.Get("accesstoken", string.Empty);
            var userName = Preferences.Get("usuarionome", string.Empty);

            Console.WriteLine($"[DEBUG] SetMainPage - Token: {accessToken}");
            Console.WriteLine($"[DEBUG] SetMainPage - UserName: {userName}");

            if (string.IsNullOrEmpty(accessToken))
            {
                MainPage = new NavigationPage(new LoginPage(_apiService, _validator));
                return;
            }

            MainPage = new AppShell(_apiService, _validator);
        }
    }
}

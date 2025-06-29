using AppLanches.Models;
using AppLanches.Services;
using AppLanches.Validations;

namespace AppLanches.Pages;

public partial class HomePage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly IValidator _validator;
    private bool _loginPageDisplayed = false;
    private bool _isDataLoaded = false;

    public HomePage(ApiService apiService, IValidator validator)
    {
        InitializeComponent();
        _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
        _validator = validator;
        OnAppearing();
        Title = AppConfig.TituloHomePage;
    }



    private async Task<IEnumerable<Category>> GetListaCategorias()
    {
        try
        {
            var (categorias, errorMessage) = await _apiService.GetCategorias();

            if (errorMessage == "Unauthorized" && !_loginPageDisplayed)
            {
                await DisplayLoginPage();
                return Enumerable.Empty<Category>();
            }

            if (categorias == null)
            {
                await DisplayAlert("Erro", errorMessage ?? "N�o foi poss�vel obter as categorias.", "OK");
                return Enumerable.Empty<Category>();
            }

            CvCategorias.ItemsSource = categorias;
            return categorias;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Ocorreu um erro inesperado: {ex.Message}", "OK");
            return Enumerable.Empty<Category>();
        }
    }

    private async Task<IEnumerable<Product>> GetMaisVendidos()
    {
        try
        {
            var (produtos, errorMessage) = await _apiService.GetProdutos("maisvendido", string.Empty);

            if (errorMessage == "Unauthorized" && !_loginPageDisplayed)
            {
                await DisplayLoginPage();
                return Enumerable.Empty<Product>();
            }

            if (produtos == null)
            {
                await DisplayAlert("Erro", errorMessage ?? "N�o foi poss�vel obter as categorias.", "OK");
                return Enumerable.Empty<Product>();
            }

            CvMaisVendidos.ItemsSource = produtos;
            return produtos;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Ocorreu um erro inesperado: {ex.Message}", "OK");
            return Enumerable.Empty<Product>();
        }
    }

    private async Task<IEnumerable<Product>> GetPopulares()
    {
        try
        {
            var (produtos, errorMessage) = await _apiService.GetProdutos("popular", string.Empty);

            if (errorMessage == "Unauthorized" && !_loginPageDisplayed)
            {
                await DisplayLoginPage();
                return Enumerable.Empty<Product>();
            }

            if (produtos == null)
            {
                await DisplayAlert("Erro", errorMessage ?? "N�o foi poss�vel obter as categorias.", "OK");
                return Enumerable.Empty<Product>();
            }
            CvPopulares.ItemsSource = produtos;
            return produtos;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Ocorreu um erro inesperado: {ex.Message}", "OK");
            return Enumerable.Empty<Product>();
        }
    }


    private async Task DisplayLoginPage()
    {
        _loginPageDisplayed = true;
        await Navigation.PushAsync(new LoginPage(_apiService, _validator));
    }

    protected override async void OnAppearing()
    {
        // onappearing is called every time the page appears, so we check if data is already loaded
        base.OnAppearing();
        if (!_isDataLoaded)
        {
            // If data is not loaded, we load it
            await LoadDataAsync();
            _isDataLoaded = true;
        }

        LblNomeUsuario.Text = "Ol�, " + Preferences.Get("usuarionome", string.Empty);
        //await GetListaCategorias();
        //await GetMaisVendidos();
        //await GetPopulares();
    }

    private async Task LoadDataAsync()
    {
        var categoriasTask = GetListaCategorias();
        var maisVendidosTask = GetMaisVendidos();
        var popularesTask = GetPopulares();
        await Task.WhenAll(categoriasTask, maisVendidosTask, popularesTask);
    }

    private void CvCategorias_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var currentSelection = e.CurrentSelection.FirstOrDefault() as Category;

        if (currentSelection == null) return;


        Navigation.PushAsync(new ListaProdutosPage(currentSelection.Id,
                                                     currentSelection.Name!,
                                                     _apiService,
                                                     _validator));

        ((CollectionView)sender).SelectedItem = null;

    }

    private void CvMaisVendidos_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is CollectionView collectionView)
        {
            NavigateToProdutoDetalhesPage(collectionView, e);
        }

    }

    private void CvPopulares_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is CollectionView collectionView)
        {
            NavigateToProdutoDetalhesPage(collectionView, e);
        }

    }

    private void NavigateToProdutoDetalhesPage(CollectionView collectionView, SelectionChangedEventArgs e)
    {
        var currentSelection = e.CurrentSelection.FirstOrDefault() as Product;

        if (currentSelection == null)
            return;

        Navigation.PushAsync(new ProdutoDetalhesPage(
                                 currentSelection.Id,
                                 currentSelection.Name!,
                                 _apiService,
                                 _validator
        ));

        collectionView.SelectedItem = null;

    }
}
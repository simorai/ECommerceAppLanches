using AppLanches.Models;
using AppLanches.Services;
using AppLanches.Validations;
using System.Collections.ObjectModel;

namespace AppLanches.Pages;

public partial class CarrinhoPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly IValidator _validator;
    private bool _loginPageDisplayed = false;
    private bool _isNavigatingToEmptyCartPage = false;

    private ObservableCollection<CarrinhoCompraItem>
        ItensCarrinhoCompra = new ObservableCollection<CarrinhoCompraItem>();

    public CarrinhoPage(ApiService apiService, IValidator validator)
    {
        InitializeComponent();
        _apiService = apiService;
        _validator = validator;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await GetItensCarrinhoCompra();

        bool enderecoSalvo = Preferences.ContainsKey("endereco");
        if (enderecoSalvo)
        {
            string nome = Preferences.Get("nome", string.Empty);
            string endereco = Preferences.Get("endereco", string.Empty);
            string telefone = Preferences.Get("telefone", string.Empty);

            //formatar os dados conforme desejado no layout
            LblEndereco.Text = $"{nome}\n{endereco}\n{telefone}";
        }
        else
        {
            LblEndereco.Text = "Informe o endenreco";
        }
    }

    private async Task<bool> GetItensCarrinhoCompra()
    {
        try
        {
            var usuarioId = Preferences.Get("usuarioid", 0);
            var (itensCarrinhoCompra, errorMessage) = await
                     _apiService.GetItensCarrinhoCompra(usuarioId);

            if (errorMessage == "Unauthorized" && !_loginPageDisplayed)
            {
                // Redirecionar para a p�gina de login
                await DisplayLoginPage();
                return false;
            }

            if (itensCarrinhoCompra == null)
            {
                await DisplayAlert("Erro", errorMessage ?? "N�o foi possivel obter os itens do carrinho de compra.", "OK");
                return false;
            }

            ItensCarrinhoCompra.Clear();
            foreach (var item in itensCarrinhoCompra)
            {
                ItensCarrinhoCompra.Add(item);
            }

            CvCarrinho.ItemsSource = ItensCarrinhoCompra;
            return true; // Correctly return a boolean value
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Ocorreu um erro inesperado: {ex.Message}", "OK");
            return false;
        }
    }


    private void AtualizaPrecoTotal()
    {
        try
        {
            var precoTotal = ItensCarrinhoCompra.Sum(item => item.Price * item.Quantidade);
            LblPrecoTotal.Text = precoTotal.ToString();
        }
        catch (Exception ex)
        {
            DisplayAlert("Erro", $"Ocorreu um erro ao atualizar o pre?o total: {ex.Message}", "OK");
        }
    }
    private async Task DisplayLoginPage()
    {
        _loginPageDisplayed = true;

        await Navigation.PushAsync(new LoginPage(_apiService, _validator));
    }

    private async void BtnDecrementar_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is CarrinhoCompraItem itemCarrinho)
        {
            if (itemCarrinho.Quantidade == 1) return;
            else
            {
                itemCarrinho.Quantidade--;
                AtualizaPrecoTotal();
                await _apiService.AtualizaQuantidadeItemCarrinho(itemCarrinho.ProdutoId, "diminuir");
            }
        }

    }

    private async void BtnIncrementar_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is CarrinhoCompraItem itemCarrinho)
        {
            itemCarrinho.Quantidade++;
            AtualizaPrecoTotal();
            await _apiService.AtualizaQuantidadeItemCarrinho(itemCarrinho.ProdutoId, "aumentar");
        }

    }

    private void BtnEditaEndereco_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(new EnderecoPage());
    }

    private async void BtnDeletar_Clicked(object sender, EventArgs e)
    {
        if (sender is ImageButton button && button.BindingContext is CarrinhoCompraItem itemCarrinho)
        {
            bool resposta = await DisplayAlert("Confirma  o",
                          "Tem certeza que deseja excluir este item do carrinho?", "Sim", "N o");
            if (resposta)
            {
                ItensCarrinhoCompra.Remove(itemCarrinho);
                AtualizaPrecoTotal();
                await _apiService.AtualizaQuantidadeItemCarrinho(itemCarrinho.ProdutoId, "deletar");
            }
        }

    }

    private void TapConfirmarPedido_Tapped(object sender, TappedEventArgs e)
    {

    }

}

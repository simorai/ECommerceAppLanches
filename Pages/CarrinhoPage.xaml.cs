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

        if (IsNavigatingToEmptyCartPage()) return;

        bool hasItems = await GetItensCarrinhoCompra();

        if (hasItems)
        {
            ExibirEndereco();
        }
        else
        {
            await NavegarParaCarrinhoVazio();
        }
    }

    private bool IsNavigatingToEmptyCartPage()
    {
        if (_isNavigatingToEmptyCartPage)
        {
            _isNavigatingToEmptyCartPage = false;
            return true;
        }
        return false;
    }

    private void ExibirEndereco()
    {
        bool enderecoSalvo = Preferences.ContainsKey("endereco");

        if (enderecoSalvo)
        {
            string nome = Preferences.Get("nome", string.Empty);
            string endereco = Preferences.Get("endereco", string.Empty);
            string telefone = Preferences.Get("telefone", string.Empty);

            // Formatar os dados conforme desejado na label
            LblEndereco.Text = $"{nome}\n{endereco} \n{telefone}";
        }
        else
        {
            LblEndereco.Text = "Informe o seu endere�o";
        }
    }

    private async Task NavegarParaCarrinhoVazio()
    {
        LblEndereco.Text = string.Empty;
        _isNavigatingToEmptyCartPage = true;
        await Navigation.PushAsync(new CarrinhoVazioPage());
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
                // Redirecionar para a p?gina de login
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
            AtualizaPrecoTotal(); // Atualizar o preco total ap?s atualizar os itens do carrinho

            if (!ItensCarrinhoCompra.Any())
            {
                return false;
            }
            return true;
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
            var precoTotal = ItensCarrinhoCompra.Sum(item => item.Price * item.Quantity);
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
            if (itemCarrinho.Quantity == 1) return;
            else
            {
                itemCarrinho.Quantity--;
                AtualizaPrecoTotal();
                await _apiService.AtualizaQuantidadeItemCarrinho(itemCarrinho.ProdutoId, "diminuir");
            }
        }
    }

    private async void BtnIncrementar_Clicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is CarrinhoCompraItem itemCarrinho)
        {
            itemCarrinho.Quantity++;
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
            bool resposta = await DisplayAlert("Confirma o",
                          "Tem certeza que deseja excluir este item do carrinho?", "Sim", "Nao");
            if (resposta)
            {
                ItensCarrinhoCompra.Remove(itemCarrinho);
                AtualizaPrecoTotal();
                await _apiService.AtualizaQuantidadeItemCarrinho(itemCarrinho.ProdutoId, "deletar");
            }
        }
    }

    private async void TapConfirmarPedido_Tapped(object sender, TappedEventArgs e)
    {
        if (ItensCarrinhoCompra == null || !ItensCarrinhoCompra.Any())
        {
            await DisplayAlert("Informa��o", "Seu carrinho est� vazio ou o pedido j� foi confirmado.", "OK");
            return;
        }

        // Valida��o do valor do total antes de converter
        decimal total = 0;
        if (string.IsNullOrWhiteSpace(LblPrecoTotal.Text) || !decimal.TryParse(LblPrecoTotal.Text, out total))
        {
            await DisplayAlert("Erro", "O valor total do pedido � inv�lido.", "OK");
            return;
        }

        var pedido = new Pedido()
        {
            Address = LblEndereco.Text,
            UserId = Preferences.Get("usuarioid", 0),
            Total = total // Usa o valor validado
        };

        var response = await _apiService.ConfirmarPedido(pedido);

        if (response.HasError)
        {
            if (response.ErrorMessage == "Unauthorized")
            {
                // Redirecionar para a p�gina de login
                await DisplayLoginPage();
                return;
            }
            await DisplayAlert("Opa !!!", $"Algo deu errado: {response.ErrorMessage}", "Cancelar");
            return;
        }

        ItensCarrinhoCompra.Clear();
        LblEndereco.Text = "Informe o seu endere�o";
        LblPrecoTotal.Text = "0.00";

        await Navigation.PushAsync(new PedidoConfirmadoPage());
    }
}

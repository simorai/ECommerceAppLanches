using AppLanches.Models;
using AppLanches.Services;
using AppLanches.Validations;

namespace AppLanches.Pages;

public partial class ProdutoDetalhesPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly IValidator _validator;
    private int _produtoId;
    private bool _loginPageDisplayed = false;
    private FavoritosService _favoritosService = new FavoritosService();
    //private FavoritosService _favoritosService;
    private string? _imagemUrl;

    public ProdutoDetalhesPage(int produtoId, string produtoNome, ApiService apiService, IValidator validator)
    {
        InitializeComponent();
        _apiService = apiService;
        _validator = validator;
        _produtoId = produtoId;
        Title = produtoNome ?? "Detalhe do Produto";
    }

    // M�todo chamado quando a p�gina aparece
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await GetProdutoDetalhes(_produtoId);
        AtualizaFavoritoButton();
    }

    private async Task<Product?> GetProdutoDetalhes(int produtoId)
    {
        var (produtoDetalhe, errorMessage) = await _apiService.GetProdutoDetalhe(produtoId);

        if (errorMessage == "Unauthorized" && !_loginPageDisplayed)
        {
            await DisplayLoginPage();
            return null;
        }

        // Verificar se houve algum erro na obten��o das produtos
        if (produtoDetalhe == null)
        {
            // Lidar com o erro, exibir mensagem ou logar
            await DisplayAlert("Erro", errorMessage ?? "N�o foi poss�vel obter o produto.", "OK");
            return null;
        }

        if (produtoDetalhe != null)
        {
            // Atualizar as propriedades dos controles com os dados do produto
            ImagemProduto.Source = produtoDetalhe.CaminhoImagem;
            LblProdutoNome.Text = produtoDetalhe.Name;
            LblProdutoPreco.Text = produtoDetalhe.Price.ToString();
            LblProdutoDescricao.Text = produtoDetalhe.Details;
            LblPrecoTotal.Text = produtoDetalhe.Price.ToString();
            _imagemUrl = produtoDetalhe.CaminhoImagem;
        }
        else
        {
            await DisplayAlert("Erro", errorMessage ?? "N�o foi poss�vel obter os detalhes do produto.", "OK");
            return null;
        }
        return produtoDetalhe;
    }

    private async Task DisplayLoginPage()
    {
        _loginPageDisplayed = true;

        await Navigation.PushAsync(new LoginPage(_apiService, _validator));
    }

    private async void ImagemBtnFavorito_Clicked(object sender, EventArgs e)
    {
        try
        {
            var existeFavorito = await _favoritosService.ReadAsync(_produtoId);
            if (existeFavorito is not null)
            {
                await _favoritosService.DeleteAsync(existeFavorito);
            }
            else
            {
                var produtoFavorito = new ProdutoFavorito()
                {
                    ProductId = _produtoId,
                    IsFavorite = true,
                    Details = LblProdutoDescricao.Text,
                    Name = LblProdutoNome.Text,
                    Price = Convert.ToDecimal(LblProdutoPreco.Text),
                    ImageUrl = _imagemUrl
                };

                await _favoritosService.CreateAsync(produtoFavorito);
            }
            AtualizaFavoritoButton();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Ocorreu um erro: {ex.Message}", "OK");
        }
    }

    private async void AtualizaFavoritoButton()
    {
        var existeFavorito = await
               _favoritosService.ReadAsync(_produtoId);

        if (existeFavorito is not null)
            ImagemBtnFavorito.Source = "heartfill";
        else
            ImagemBtnFavorito.Source = "heart";
    }

    private void BtnRemove_Clicked(object sender, EventArgs e)
    {
        if (int.TryParse(LblQuantidade.Text, out int quantidade) &&
            decimal.TryParse(LblProdutoPreco.Text, out decimal precoUnitario))
        {
            // Decrementa a quantidade, e n o permite que seja menor que 1
            quantidade = Math.Max(1, quantidade - 1);
            LblQuantidade.Text = quantidade.ToString();

            // Calcula o pre o total
            var precoTotal = quantidade * precoUnitario;
            LblPrecoTotal.Text = precoTotal.ToString();
        }
        else
        {
            // Tratar caso as convers es falhem
            DisplayAlert("Erro", "Valores inv lidos", "OK");
        }
    }

    private void BtnAdiciona_Clicked(object sender, EventArgs e)
    {
        if (int.TryParse(LblQuantidade.Text, out int quantidade) &&
      decimal.TryParse(LblProdutoPreco.Text, out decimal precoUnitario))
        {
            // Incrementa a quantidade
            quantidade++;
            LblQuantidade.Text = quantidade.ToString();

            // Calcula o preco total
            var precoTotal = quantidade * precoUnitario;
            LblPrecoTotal.Text = precoTotal.ToString(); // Formata como moeda
        }
        else
        {
            // Tratar caso as conversoes falhem
            DisplayAlert("Erro", "Valores inv lidos", "OK");
        }
    }

    private async void BtnIncluirNoCarrinho_Clicked(object sender, EventArgs e)
    {
        try
        {
            var carrinhoCompra = new CarrinhoCompra()
            {
                quantity = Convert.ToInt32(LblQuantidade.Text),
                unitPrice = Convert.ToDecimal(LblProdutoPreco.Text),
                total = Convert.ToDecimal(LblPrecoTotal.Text),
                productId = _produtoId,
                clientId = Preferences.Get("usuarioid", 0)
            };
            var response = await _apiService.AdicionaItemNoCarrinho(carrinhoCompra);
            if (response.Data)
            {
                await DisplayAlert("Sucesso", "Item adicionado ao carrinho !", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Erro", $"Falha ao adicionar item: {response.ErrorMessage}", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Ocorreu um erro: {ex.Message}", "OK");
        }
    }
}
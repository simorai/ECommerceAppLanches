namespace AppLanches.Pages;

public partial class CarrinhoVazioPage : ContentPage
{
    public CarrinhoVazioPage()
    {
        InitializeComponent();
    }

    private async void BtnRetornar_Clicked(object sender, EventArgs e)
    {
        // Navega de volta para a página anterior
        await Navigation.PopAsync();
    }
}
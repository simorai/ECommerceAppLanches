namespace AppLanches.Pages;

public partial class CarrinhoVazioPage : ContentPage
{
    public CarrinhoVazioPage()
    {
        InitializeComponent();
    }

    private async void BtnRetornar_Clicked(object sender, EventArgs e)
    {
        // Navega de volta para a p�gina anterior
        await Navigation.PopAsync();
    }
}
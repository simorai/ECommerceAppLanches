namespace AppLanches.Models
{
    public class PedidoDetalhe
    {
        public int Id { get; set; }

        public int Quantity { get; set; }

        public decimal SubTotal { get; set; }

        public string? ProductName { get; set; }

        public string? ProductImage { get; set; }

        public string ImagePath => AppConfig.BaseUrl + ProductImage;

        public decimal ProductPrice { get; set; }
    }
}

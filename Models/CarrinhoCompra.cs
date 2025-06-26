namespace AppLanches.Models
{
    public class CarrinhoCompra
    {
        public decimal unitPrice { get; set; }
        public int quantity { get; set; }
        public decimal total { get; set; }
        public int productId { get; set; }
        public int clientId { get; set; }
    }
}

namespace AppLanches.Models
{
    public class Pedido
    {
        public string? Address { get; set; }
        public decimal Total { get; set; }
        public int UserId { get; set; }
    }
}

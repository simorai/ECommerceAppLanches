using System.Text.Json.Serialization;

namespace AppLanches.Models
{
    public class PedidoPorUsuario
    {
        public int Id { get; set; }

        [JsonPropertyName("total")]
        public decimal TotalOrder { get; set; }

        public DateTime OrderDate { get; set; }
    }
}

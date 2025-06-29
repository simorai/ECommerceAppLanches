using SQLite;

namespace AppLanches.Models
{
    public class ProdutoFavorito
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int ProductId { get; set; }
        public string? Name { get; set; }
        public string? Details { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsFavorite { get; set; }

    }
}

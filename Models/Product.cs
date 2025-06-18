namespace AppLanches.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public decimal Price { get; set; }
        public string? Details { get; set; }
        public string? UrlImage { get; set; }
        public string? CaminhoImagem => AppConfig.BaseUrl + UrlImage;

    }
}

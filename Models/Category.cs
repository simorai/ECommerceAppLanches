namespace AppLanches.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? UrlImage { get; set; }
        public string? CaminhoImagem => AppConfig.BaseUrl + UrlImage;

    }
}

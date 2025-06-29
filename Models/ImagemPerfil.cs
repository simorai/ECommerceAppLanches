namespace AppLanches.Models
{
    public class ImagemPerfil
    {
        public string? UrlImage { get; set; }
        public string? ImagePath => AppConfig.BaseUrl + UrlImage;

    }
}

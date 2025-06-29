using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AppLanches.Models
{
    public class CarrinhoCompraItem : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
        private int _quantity;
        public int Quantity
        {
            get { return _quantity; }
            set
            {
                if (_quantity != value)
                {
                    _quantity = value;
                    OnPropertyChanged();
                }
            }
        }

        public int ProdutoId { get; set; }
        public string? ProductName { get; set; }
        public string? UrlImage { get; set; }
        public string? ImagePath => AppConfig.BaseUrl + UrlImage;
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}

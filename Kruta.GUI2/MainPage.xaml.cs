using Kruta.GUI2.ViewModels;

namespace Kruta.GUI2
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            BindingContext = new MainViewModel();
        }        
    }
}

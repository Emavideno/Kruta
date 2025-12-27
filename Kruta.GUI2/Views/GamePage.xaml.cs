namespace Kruta.GUI2.Views;

public partial class GamePage : ContentPage
{
    public GamePage(ViewModels.GameViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel; // Привязываем ViewModel к странице
    }
}
using Kruta.GUI2.ViewModels;

namespace Kruta.GUI2.Views;

public partial class PlayPage : ContentPage
{
    public PlayPage(PlayViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnExitClicked(object sender, EventArgs e)
    {
        // Пример возврата назад
        await Shell.Current.GoToAsync("//RegistrationPage");
    }
}
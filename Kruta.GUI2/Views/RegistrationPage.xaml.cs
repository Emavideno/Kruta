using Kruta.GUI2.ViewModels;

namespace Kruta.GUI2.Views;

public partial class RegistrationPage : ContentPage
{
    public RegistrationPage(ViewModels.RegistrationViewModel viewModel)
    {
        InitializeComponent();
        // Вот здесь происходит магия DI
        BindingContext = viewModel;
    }
}
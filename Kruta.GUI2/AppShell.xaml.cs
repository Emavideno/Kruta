using Kruta.GUI2.ViewModels;

namespace Kruta.GUI2
{
    public partial class AppShell : Shell
    {
        // Внедряем PlayViewModel. MAUI создаст её автоматически при запуске Shell.
        public AppShell(PlayViewModel forceInit)
        {
            InitializeComponent();

            Routing.RegisterRoute("GamePage", typeof(Views.GamePage));
            Routing.RegisterRoute("PlayPage", typeof(Views.PlayPage));
        }
    }
}
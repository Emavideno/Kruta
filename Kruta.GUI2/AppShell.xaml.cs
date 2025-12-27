namespace Kruta.GUI2
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Регистрируем маршрут для страницы игры
            Routing.RegisterRoute("GamePage", typeof(Views.GamePage));

            // Регистрируем маршрут для самой игры
            Routing.RegisterRoute("PlayPage", typeof(Views.PlayPage));
        }
    }
}

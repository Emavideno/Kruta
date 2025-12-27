using Microsoft.Extensions.DependencyInjection;

namespace Kruta.GUI2
{
    public partial class App : Application
    {
        private readonly AppShell _shell;

        // Получаем Shell из контейнера зависимостей
        public App(AppShell shell)
        {
            InitializeComponent();
            _shell = shell;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Используем shell, который нам передал DI-контейнер
            return new Window(_shell);
        }
    }
}
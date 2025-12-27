using Microsoft.Extensions.DependencyInjection;

namespace Kruta.GUI2
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            // Оставляем только это. 
            // Это создаст окно и установит AppShell как корневой элемент.
            return new Window(new AppShell());
        }
    }
}
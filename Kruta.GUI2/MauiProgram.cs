using Kruta.GUI2.Services;
using Kruta.GUI2.ViewModels;
using Kruta.GUI2.Views;
using Microsoft.Extensions.Logging;


namespace Kruta.GUI2
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            // Сервис — один на всё приложение (обязательно!)
            builder.Services.AddSingleton<NetworkService>();

            // ViewModel для регистрации — Transient (чистая при каждом заходе)
            builder.Services.AddTransient<RegistrationViewModel>();

            // GameViewModel — тут зависит от логики. 
            // Если хочешь, чтобы данные игры не сбрасывались при сворачивании — Singleton.
            // Если это просто экран, который должен обновляться — Transient.
            builder.Services.AddSingleton<GameViewModel>();

            // Страницы всегда Transient
            builder.Services.AddTransient<RegistrationPage>();
            builder.Services.AddTransient<GamePage>();

            // ViewModel для игры
            builder.Services.AddTransient<PlayViewModel>();

            // Страница игры
            builder.Services.AddTransient<PlayPage>();

            return builder.Build();
        }
    }
}

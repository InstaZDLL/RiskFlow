using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using RiskFlow.Services;
using RiskFlow.ViewModels;
using RiskFlow.Data;

namespace RiskFlow
{
    /// <summary>
    /// Application RiskFlow. Configure l'injection de dépendances, initialise la base
    /// SQLite locale puis affiche la fenêtre principale.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;

        /// <summary>Conteneur de services partagé de l'application.</summary>
        public static IServiceProvider Services { get; private set; } = null!;

        public App()
        {
            InitializeComponent();
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
            Services = ConfigureServices();
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddDbContextFactory<RiskFlowDbContext>(options =>
                options.UseSqlite(AppPaths.ConnectionString));

            services.AddSingleton<SettingsService>();
            services.AddSingleton<RisksViewModel>();
            services.AddSingleton<ShellViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddTransient<MainPage>();
            services.AddTransient<MainWindow>();

            return services.BuildServiceProvider();
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            await using (var db = await Services
                .GetRequiredService<IDbContextFactory<RiskFlowDbContext>>()
                .CreateDbContextAsync())
            {
                await DbInitializer.InitializeAsync(db);
            }

            // Charge les analyses avant d'afficher la fenêtre (le shell sélectionne la 1re).
            await Services.GetRequiredService<ShellViewModel>().LoadAsync();

            _window = Services.GetRequiredService<MainWindow>();
            _window.Activate();
        }
    }
}

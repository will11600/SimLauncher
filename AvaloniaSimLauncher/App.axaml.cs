using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaSimLauncher.ViewModels;
using AvaloniaSimLauncher.Views;
using SimLauncher;
using System;
using System.Collections.Generic;

namespace AvaloniaSimLauncher
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };

                desktop.MainWindow.ShowDialog(new ProgressBarDialogue
                {
                    DataContext = new ProgressBarDialogueViewModel("Creating mod database...", ContentManager.GetModLoader())
                });
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}

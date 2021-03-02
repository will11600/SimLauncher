using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using AvaloniaSimLauncher.Views;
using MessageBox.Avalonia.Enums;
using SimLauncher;
using SimLauncher.DataTemplates;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaSimLauncher.ViewModels
{
    class ProgressBarDialogueViewModel : ViewModelBase
    {
        private IProgressBarOperation Operation;

        public string Title { get; }

        public int Max => Operation.Max;
        public int Min => Operation.Min;
        public string Status => Operation.Status;
        public float Progress => Operation.Current;

        public ProgressBarDialogueViewModel(string title, IProgressBarOperation operation)
        {
            Title = title;
            Operation = operation;

            operation.Main().ContinueWith(OnCompleted);
        }

        private void OnCompleted(Task task)
        {
            Dispatcher.UIThread.Post(() =>
            {
                var main = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
                main.Show();
                App.Desktop!.MainWindow.Close();
                App.Desktop!.MainWindow = main;
            });
        }
    }
}

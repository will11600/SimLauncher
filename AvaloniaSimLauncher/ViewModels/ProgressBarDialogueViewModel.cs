using Avalonia;
using Avalonia.Threading;
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
        public string Title { get; }

        public int Max => Operations.Count();
        public int Min => 0;
        public string Status => current?.status ?? "Initializing...";
        public float Progress => currentIndex;

        public IEnumerable<AsyncOperation> Operations { get; }
        private int currentIndex = 0;
        private AsyncOperation? current;

        private CancellationTokenSource source;

        public ProgressBarDialogueViewModel(string title, IEnumerable<AsyncOperation> operations)
        {
            Title = title;
            Operations = operations;
            source = new CancellationTokenSource();

            InvokeOperations(source.Token);
        }

        private void InvokeOperations(CancellationToken token)
        {
            foreach (var op in Operations)
            {
                if (token.IsCancellationRequested) { break; }

                currentIndex++;
                current = op;

                Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    try { op.Main?.Invoke(); }
                    catch (Exception e)
                    {
                        if (await DialogueBox.SendError(e, ButtonEnum.OkAbort) == ButtonResult.Abort) { source.Cancel(); }
                    }
                }, DispatcherPriority.Normal);
            }
        }
    }
}

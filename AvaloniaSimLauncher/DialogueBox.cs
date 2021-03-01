using Avalonia.Threading;
using MessageBox.Avalonia;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using System;
using System.Threading.Tasks;

namespace SimLauncher
{
    public static class DialogueBox
    {
        private static string defaultTitle = "SimLauncher";

        public static Task<ButtonResult> SendError(string message, ButtonEnum button = ButtonEnum.Ok)
        {
            var msgBox = MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
            {
                ButtonDefinitions = button,
                ContentTitle = defaultTitle,
                ContentMessage = message,
                Icon = Icon.Error,
                Style = Style.Windows
            });
            return msgBox.Show();
        }

        public static Task<ButtonResult> SendError(Exception e, ButtonEnum button = ButtonEnum.Ok)
        {
            var msgBox = MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
            {
                ButtonDefinitions = button,
                ContentTitle = defaultTitle,
                ContentMessage = $"{e.GetType().Name}: {e.Message}\n\n{e.StackTrace}",
                Icon = Icon.Error,
                Style = Style.Windows
            });
            return msgBox.Show();
        }

        public static Task<ButtonResult> Send(string message)
        {
            var msgBox = MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.Ok,
                ContentTitle = defaultTitle,
                ContentMessage = message,
                Icon = Icon.Info,
                Style = Style.Windows
            });
            return msgBox.Show();
        }
        
        public static Task<ButtonResult> SendWarning(string message, ButtonEnum button = ButtonEnum.Ok)
        {
            var msgBox = MessageBoxManager.GetMessageBoxStandardWindow(new MessageBoxStandardParams
            {
                ButtonDefinitions = button,
                ContentTitle = defaultTitle,
                ContentMessage = message,
                Icon = Icon.Warning,
                Style = Style.Windows
            });
            return msgBox.Show();
        }
    }
}

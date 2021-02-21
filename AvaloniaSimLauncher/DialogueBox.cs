using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SimLauncher
{
    public static class DialogueBox
    {
        public static MessageBoxResult SendError(string message/*, MessageBoxButton button = MessageBoxButton.OK*/)
        {
            return /*MessageBox.Show(message, "Error", button, MessageBoxImage.Error);*/null;
        }

        public static MessageBoxResult Send(string message)
        {
            return /*MessageBox.Show(message, "Information");*/null;
        }
        
        public static MessageBoxResult SendWarning(string message/*, MessageBoxButton button = MessageBoxButton.OK*/)
        {
            return /*MessageBox.Show(message, "Warning", button, MessageBoxImage.Warning);*/null;
        }
    }

    public class MessageBoxResult
    {
    }

    public class MessageBoxButton
    {
    }
}

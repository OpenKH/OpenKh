using OpenKh.Tools.ModsManager.Services;
using System;
using System.Windows;

namespace OpenKh.Tools.ModsManager
{
    public static class Helpers
    {
        public static bool Question(string message, string title = null) =>
            MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

        public static void Handle(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Handle(ex);
            }
        }

        public static T Handle<T>(Func<T> action)
        {
            try
            {
                return action();
            }
            catch (Exception ex)
            {
                Handle(ex);
                return default;
            }
        }

        public static void Handle(Exception ex)
        {
            switch (ex)
            {
                case RepositoryNotFoundException _:
                    MessageBox.Show(ex.Message, "Repository not found", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                default:
                    MessageBox.Show(ex.Message, "Generic error", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
            }
        }
    }
}

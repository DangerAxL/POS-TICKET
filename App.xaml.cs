using System.Windows;
using SimplePOS.Models;

namespace SimplePOS
{
    public partial class App : Application
    {
        public static User? CurrentUser { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var loginWindow = new Views.LoginWindow();
            if (loginWindow.ShowDialog() == true && loginWindow.LoggedInUser != null)
            {
                CurrentUser = loginWindow.LoggedInUser;
                var mainWindow = new Views.MainWindow();
                mainWindow.Show();
            }
            else
            {
                // User cancelled or failed login, close the app
                Shutdown();
            }
        }
    }
}

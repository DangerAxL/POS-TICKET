using System;
using System.Linq;
using System.Windows;
using SimplePOS.Models;

namespace SimplePOS.Views
{
    public partial class LoginWindow : Window
    {
        public User? LoggedInUser { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
            CheckAndCreateDefaultAdmin();
            UsernameBox.Focus();
        }

        private void CheckAndCreateDefaultAdmin()
        {
            try
            {
                using var db = new PosDbContext();
                db.Database.EnsureCreated(); // Ensure DB is there

                if (!db.Users.Any())
                {
                    // Create default Admin if no users exist
                    db.Users.Add(new User
                    {
                        Username = "admin",
                        PasswordHash = "admin", // Simple for now
                        Role = "Admin"
                    });
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error conectando a la base de datos de usuarios.", "Error DB", MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine(ex.Message);
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ErrorText.Text = "Ingrese usuario y contraseña";
                ErrorText.Visibility = Visibility.Visible;
                return;
            }

            try
            {
                using var db = new PosDbContext();
                var user = db.Users.FirstOrDefault(u => u.Username == username && u.PasswordHash == password);

                if (user != null)
                {
                    LoggedInUser = user;
                    DialogResult = true;
                    Close();
                }
                else
                {
                    ErrorText.Text = "Credenciales incorrectas";
                    ErrorText.Visibility = Visibility.Visible;
                    PasswordBox.Clear();
                    PasswordBox.Focus();
                }
            }
            catch (Exception ex)
            {
                ErrorText.Text = "Error de conexión";
                ErrorText.Visibility = Visibility.Visible;
                Console.WriteLine(ex.Message);
            }
        }
    }
}

using System;
using System.IO;
using System.Linq;
using System.Windows;
using SimplePOS.Models;

namespace SimplePOS.Views
{
    public partial class SettingsWindow : Window
    {
        public string NewCajaNumber { get; private set; }

        public SettingsWindow()
        {
            InitializeComponent();
            LoadCurrentSettings();
        }

        private void LoadCurrentSettings()
        {
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");
                if (File.Exists(configPath))
                {
                    var lines = File.ReadAllLines(configPath);
                    
                    var cajaLine = lines.FirstOrDefault(l => l.StartsWith("CajaNumber="));
                    if (cajaLine != null)
                        CajaNumberTextBox.Text = cajaLine.Split('=')[1].Trim();
                    else
                        CajaNumberTextBox.Text = "1";

                    var businessLine = lines.FirstOrDefault(l => l.StartsWith("BusinessName="));
                    if (businessLine != null)
                        BusinessNameTextBox.Text = businessLine.Split('=')[1].Trim();
                    else
                        BusinessNameTextBox.Text = "CARNAVALES 2026";
                }
                else
                {
                    CajaNumberTextBox.Text = "1";
                    BusinessNameTextBox.Text = "CARNAVALES 2026";
                }
            }
            catch
            {
                CajaNumberTextBox.Text = "1";
                BusinessNameTextBox.Text = "CARNAVALES 2026";
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CajaNumberTextBox.Text))
            {
                MessageBox.Show("Por favor ingrese un número de caja válido.");
                return;
            }

            if (string.IsNullOrWhiteSpace(BusinessNameTextBox.Text))
            {
                MessageBox.Show("Por favor ingrese el nombre de la empresa.");
                return;
            }

            NewCajaNumber = CajaNumberTextBox.Text;
            
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");
                string content = $"[General]{Environment.NewLine}CajaNumber={NewCajaNumber}{Environment.NewLine}BusinessName={BusinessNameTextBox.Text}";
                File.WriteAllText(configPath, content);
                
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la configuración: {ex.Message}");
            }
        }

        private void CreateCashier_Click(object sender, RoutedEventArgs e)
        {
            string username = NewCashierUsername.Text.Trim();
            string password = NewCashierPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("El usuario y la contraseña no pueden estar vacíos.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using var db = new PosDbContext();
                
                // Check if user already exists
                if (db.Users.Any(u => u.Username == username))
                {
                    MessageBox.Show("Ya existe una caja con ese nombre de usuario.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                db.Users.Add(new User
                {
                    Username = username,
                    PasswordHash = password, // Simple plain text for now, as implemented in Login
                    Role = "Cajero"
                });

                db.SaveChanges();

                MessageBox.Show($"Caja '{username}' agregada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Clear inputs
                NewCashierUsername.Clear();
                NewCashierPassword.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al crear la caja: {ex.Message}", "Error de Base de Datos", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

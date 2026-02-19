using SimplePOS.Models;
using System.Windows;
using System.Windows.Controls;

namespace SimplePOS.Views
{
    public partial class AddProductWindow : Window
    {
        public Product? NewProduct { get; private set; }

        public AddProductWindow()
        {
            InitializeComponent();
            NameInput.Focus();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameInput.Text))
            {
                MessageBox.Show("El nombre es obligatorio.");
                return;
            }

            if (!decimal.TryParse(PriceInput.Text, out decimal price))
            {
                MessageBox.Show("Ingrese un precio válido.");
                return;
            }

            var selectedItem = (ComboBoxItem)ColorPicker.SelectedItem;
            string color = selectedItem.Tag.ToString() ?? "#93c5fd";

            NewProduct = new Product
            {
                Name = NameInput.Text.ToUpper(),
                Price = price,
                Color = color
            };

            DialogResult = true;
            Close();
        }
    }
}

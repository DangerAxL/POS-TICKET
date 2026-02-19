using System.Windows;

namespace SimplePOS.Views
{
    public partial class QuantityInputWindow : Window
    {
        public int Quantity { get; private set; }

        public QuantityInputWindow(string productName)
        {
            InitializeComponent();
            ProductLabel.Text = productName;
            QuantityInput.Focus();
            QuantityInput.SelectAll();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(QuantityInput.Text, out int result) && result > 0)
            {
                Quantity = result;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Por favor ingrese una cantidad válida (mayor a 0).");
            }
        }
    }
}

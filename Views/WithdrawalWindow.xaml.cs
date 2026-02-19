using System.Windows;

namespace SimplePOS.Views
{
    public partial class WithdrawalWindow : Window
    {
        public decimal Amount { get; private set; }

        public WithdrawalWindow()
        {
            InitializeComponent();
            AmountInput.Focus();
            AmountInput.SelectAll();
        }

        private void Withdraw_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(AmountInput.Text, out decimal result))
            {
                Amount = result;
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Por favor ingrese un monto válido.");
            }
        }
    }
}

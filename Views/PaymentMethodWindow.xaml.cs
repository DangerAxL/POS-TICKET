using System.Windows;

namespace SimplePOS.Views
{
    public partial class PaymentMethodWindow : Window
    {
        public string? SelectedMethod { get; set; } // Legacy or general indicator if needed
        public decimal CashAmount { get; private set; } = 0;
        public decimal DigitalAmount { get; private set; } = 0;
        
        public decimal TotalToPay { get; }

        public PaymentMethodWindow(decimal totalToPay)
        {
            InitializeComponent();
            TotalToPay = totalToPay;
            TotalText.Text = TotalToPay.ToString("C0");
            
            // Set focus to cash input initially
            CashInput.Focus();
            CashInput.SelectAll();
        }

        private void Input_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (ConfirmBtn == null) return;

            decimal cash = 0;
            decimal mp = 0;

            bool cashValid = decimal.TryParse(CashInput.Text, out cash);
            bool mpValid = decimal.TryParse(MpInput.Text, out mp);

            if (!cashValid && !string.IsNullOrWhiteSpace(CashInput.Text))
            {
                WarningText.Text = "Monto de efectivo inválido.";
                WarningText.Visibility = Visibility.Visible;
                ConfirmBtn.IsEnabled = false;
                return;
            }

            if (!mpValid && !string.IsNullOrWhiteSpace(MpInput.Text))
            {
                WarningText.Text = "Monto de Mercado Pago inválido.";
                WarningText.Visibility = Visibility.Visible;
                ConfirmBtn.IsEnabled = false;
                return;
            }

            decimal sum = cash + mp;

            if (sum == TotalToPay)
            {
                WarningText.Visibility = Visibility.Collapsed;
                ConfirmBtn.IsEnabled = true;
            }
            else
            {
                WarningText.Text = $"Falta cobrar: {(TotalToPay - sum).ToString("C0")}";
                // If overpaid, optionally show change due or block it (we will block for exact change simplicity in hybrid).
                if (sum > TotalToPay)
                {
                    WarningText.Text = $"Se supera el total por: {(sum - TotalToPay).ToString("C0")}";
                }
                WarningText.Visibility = Visibility.Visible;
                ConfirmBtn.IsEnabled = false;
            }
        }

        private void AllCash_Click(object sender, RoutedEventArgs e)
        {
            CashInput.Text = TotalToPay.ToString("F0");
            MpInput.Text = "0";
        }

        private void AllMp_Click(object sender, RoutedEventArgs e)
        {
            MpInput.Text = TotalToPay.ToString("F0");
            CashInput.Text = "0";
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (decimal.TryParse(CashInput.Text, out decimal cash)) CashAmount = cash;
            if (decimal.TryParse(MpInput.Text, out decimal mp)) DigitalAmount = mp;

            if (CashAmount > 0 && DigitalAmount > 0)
                SelectedMethod = "Híbrido";
            else if (CashAmount > 0)
                SelectedMethod = "Efectivo";
            else
                SelectedMethod = "Mercado Pago/Transferencia";

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

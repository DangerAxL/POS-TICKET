using System.Windows;

namespace SimplePOS.Views
{
    public partial class PaymentMethodWindow : Window
    {
        public string? SelectedMethod { get; private set; }

        public PaymentMethodWindow()
        {
            InitializeComponent();
        }

        private void Method_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn)
            {
                SelectedMethod = btn.Tag.ToString();
                DialogResult = true;
                Close();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}

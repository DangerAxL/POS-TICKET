using System.Windows;

namespace SimplePOS.Views
{
    public partial class UpdatePriceWindow : Window
    {
        public UpdatePriceWindow(object dataContext)
        {
            InitializeComponent();
            DataContext = dataContext;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}

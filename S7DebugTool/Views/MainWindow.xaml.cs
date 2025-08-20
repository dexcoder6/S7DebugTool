using System.Windows;
using System.Windows.Controls;
using S7DebugTool.ViewModels;

namespace S7DebugTool.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel? viewModel;

        public MainWindow()
        {
            InitializeComponent();
            viewModel = new MainViewModel();
            DataContext = viewModel;
        }

        private void TxtLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (viewModel?.AutoScroll == true && sender is TextBox textBox)
            {
                textBox.ScrollToEnd();
            }
        }
    }
}
using Avalonia.Controls;
using Avalonia.Interactivity;
using shoppro.ViewModels;
using shoppro.Views;

namespace shoppro.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel(); // or use DI if you have it
        }

        private void OnGoToProducts(object? sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.NavigateToProducts();
            }
        }
    }
}

using Avalonia.Controls;

namespace shoppro.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new shoppro.ViewModels.MainWindowViewModel();
        DataContext = new shoppro.ViewModels.InventoryViewModel();
    }
}
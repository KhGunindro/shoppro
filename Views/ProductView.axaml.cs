using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using shoppro.ViewModels;

namespace shoppro.Views
{
    public partial class ProductView : UserControl
    {
        public ProductView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnRefreshClicked(object sender, RoutedEventArgs e)
        {
            if (DataContext is ProductViewModel vm)
                vm.LoadProducts();
        }
    }
}

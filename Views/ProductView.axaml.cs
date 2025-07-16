using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using shoppro.ViewModels;
using System.Threading.Tasks; // Required for Task

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

        // The initial data load is handled by the ViewModel's constructor (synchronously on UI thread).
        // So, OnAttachedToVisualTree no longer needs to trigger an initial load.
        // protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
        // {
        //     base.OnAttachedToVisualTree(e);
        // }

        // This event handler is for explicit user actions, like a "Refresh" button.
        private async void OnRefreshClicked(object sender, RoutedEventArgs e)
        {
            if (DataContext is ProductViewModel vm)
            {
                // This uses the async version of LoadProducts, which fetches data
                // on a background thread and marshals collection updates to the UI thread.
                await vm.LoadProductsAsync();
            }
        }
    }
}
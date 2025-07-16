using ReactiveUI;
using shoppro.Models;
using shoppro.Services;
using System.Collections.ObjectModel;
using System.Reactive;
using Avalonia.Threading; // For Dispatcher.UIThread
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using System.Reactive.Concurrency; // For IScheduler (RxApp.MainThreadScheduler)

namespace shoppro.ViewModels;

public class ProductViewModel : ViewModelBase
{
    private readonly ProductService _productService = new();
    private string _searchTerm = string.Empty;
    private string _notificationMessage = string.Empty;
    private DateTime _lastUpdate = DateTime.Now;
    private bool _isNotificationVisible;
    private string _lastUpdateText = "🔄 Last Updated: Now";

    // SourceList to hold all products, managed by DynamicData
    private readonly SourceList<Product> _allProducts = new();

    // ReadOnlyObservableCollection for UI binding, filtered by SearchTerm
    public ReadOnlyObservableCollection<Product> FilteredProducts { get; }

    private int _totalProductCount;
    public int TotalProductCount
    {
        get => _totalProductCount;
        set => this.RaiseAndSetIfChanged(ref _totalProductCount, value);
    }

    // SelectedProduct, ensures it's always a ReactiveObject
    private Product _selectedProduct;
    public Product SelectedProduct
    {
        get => _selectedProduct;
        // Assign a new Product (which is ReactiveObject) if null is attempted.
        set => this.RaiseAndSetIfChanged(ref _selectedProduct, value ?? new Product());
    }

    public string SearchTerm
    {
        get => _searchTerm;
        set => this.RaiseAndSetIfChanged(ref _searchTerm, value);
    }

    public string NotificationMessage
    {
        get => _notificationMessage;
        set => this.RaiseAndSetIfChanged(ref _notificationMessage, value);
    }

    public bool IsNotificationVisible
    {
        get => _isNotificationVisible;
        set => this.RaiseAndSetIfChanged(ref _isNotificationVisible, value);
    }

    public DateTime LastUpdate
    {
        get => _lastUpdate;
        set => this.RaiseAndSetIfChanged(ref _lastUpdate, value);
    }

    public string LastUpdateText
    {
        get => _lastUpdateText;
        set => this.RaiseAndSetIfChanged(ref _lastUpdateText, value);
    }

    // ReactiveCommands for UI interactions
    public ReactiveCommand<Unit, Unit> AddCommand { get; }
    public ReactiveCommand<Unit, Unit> UpdateCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
    public ReactiveCommand<Unit, Unit> NewCommand { get; }

    public ProductViewModel()
    {
        // Initialize _selectedProduct FIRST, ensuring it's a valid ReactiveObject instance from the start.
        // This prevents potential null reference issues for bindings or canExecute observables at startup.
        _selectedProduct = new Product();

        // --- EXPLICIT CANEXECUTE OBSERVABLES WITH OBSERVEON ---
        // Observable for Update/Delete commands: true if a product is selected (Id != 0)
        var canExecuteIfSelected = this.WhenAnyValue(x => x.SelectedProduct)
                                       .Select(p => p != null && p.Id != 0)
                                       .ObserveOn(RxApp.MainThreadScheduler); // Crucial: ensure evaluation on UI thread

        // Observable for Add command: true if Product Name is not empty/whitespace
        var canAddProduct = this.WhenAnyValue(x => x.SelectedProduct.Name)
                                .Select(name => !string.IsNullOrWhiteSpace(name))
                                .ObserveOn(RxApp.MainThreadScheduler); // Crucial: ensure evaluation on UI thread

        // Initialize ReactiveCommands with their respective canExecute observables.
        // ReactiveCommand internally handles combining these with its IsExecuting state,
        // and because our canExecute observables are already on the MainThreadScheduler,
        // the `CanExecuteChanged` notifications sent to the Button should also be on the main thread.
        AddCommand = ReactiveCommand.CreateFromTask(AddProductAsync, canAddProduct);
        UpdateCommand = ReactiveCommand.CreateFromTask(UpdateProductAsync, canExecuteIfSelected);
        DeleteCommand = ReactiveCommand.CreateFromTask(DeleteProductAsync, canExecuteIfSelected);
        NewCommand = ReactiveCommand.Create(NewProduct); // NewCommand is always enabled

        // Setup filtered products using DynamicData
        var productsFilter = this.WhenValueChanged(x => x.SearchTerm) // React to SearchTerm changes
            .Select(searchTerm => new Func<Product, bool>(product => // Create a filter predicate
                string.IsNullOrWhiteSpace(searchTerm) ||
                (product.Name?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true) ||
                (product.Sku?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true) ||
                (product.Barcode?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true)));

        // Connect SourceList to FilteredProducts for UI binding
        _allProducts.Connect()
            .Filter(productsFilter) // Apply the search filter
            .ObserveOn(RxApp.MainThreadScheduler) // Crucial: All collection changes are marshalled to the UI thread
            .Bind(out var filteredProducts) // Bind to the ReadOnlyObservableCollection
            .DisposeMany() // Proper disposal of items when they leave the collection
            .Subscribe(); // Start the observable chain

        FilteredProducts = filteredProducts;

        // Subscribe to _allProducts changes to update TotalProductCount
        // .Count() provides the current count of items in the SourceList
        _allProducts.Connect()
            .Count()
            .ObserveOn(RxApp.MainThreadScheduler) // Ensure the count update happens on the UI thread
            .Subscribe(count => TotalProductCount = count); // Update the bound property

        // Throttled subscription for search term changes to update timestamp
        this.WhenAnyValue(x => x.SearchTerm)
            .Throttle(TimeSpan.FromMilliseconds(200)) // Wait for 200ms of no changes before processing
            .ObserveOn(RxApp.MainThreadScheduler) // Ensure timestamp update is on UI thread
            .Subscribe(_ => UpdateTimestamp()); // Update the last update time text

        // --- INITIAL DATA LOAD (Synchronous on UI thread at startup) ---
        // This is a "last ditch" effort to ensure initial data is loaded into the UI
        // without threading issues during application startup.
        // WARNING: If _productService.GetAll() is slow, this will block the UI thread.
        Dispatcher.UIThread.Invoke(() =>
        {
            var products = _productService.GetAll(); // Synchronous call to get initial data
            _allProducts.Clear(); // Clear existing (empty) list
            _allProducts.AddRange(products); // Populate the SourceList
            UpdateTimestamp(); // Update UI with initial load time
        });
    }

    // Public method to load products, useful for a "Refresh" button
    public async Task LoadProductsAsync()
    {
        // Fetch data on a background thread to keep UI responsive
        var products = await Task.Run(() => _productService.GetAll());

        // Ensure SourceList modification (Clear/AddRange) is always on the UI thread
        RxApp.MainThreadScheduler.Schedule(() =>
        {
            _allProducts.Clear();
            _allProducts.AddRange(products);
            UpdateTimestamp();
        });
    }

    public async Task AddProductAsync()
    {
        // Client-side validation (redundant with canAddProduct observable, but good for UX)
        if (string.IsNullOrWhiteSpace(SelectedProduct.Name))
        {
            ShowNotification("Product name is required!");
            return;
        }

        // Create a new Product instance with current UI values
        var productToAdd = new Product
        {
            Name = SelectedProduct.Name,
            Sku = SelectedProduct.Sku,
            Barcode = SelectedProduct.Barcode,
            Quantity = SelectedProduct.Quantity,
            Price = SelectedProduct.Price
        };

        // Perform database add operation on a background thread
        await Task.Run(() => _productService.Add(productToAdd));

        // Update the SourceList (and thus FilteredProducts) on the UI thread
        RxApp.MainThreadScheduler.Schedule(() =>
        {
            _allProducts.Add(productToAdd);
            ShowNotification("Product added successfully!");
            NewProduct(); // Reset SelectedProduct, which updates command canExecute states
        });
    }

    public async Task UpdateProductAsync()
    {
        // Client-side validation (redundant with canExecuteIfSelected observable, but good for UX)
        if (SelectedProduct == null || SelectedProduct.Id == 0)
        {
            ShowNotification("Please select a product to update");
            return;
        }

        // Create a new Product instance with updated UI values
        var productToUpdate = new Product
        {
            Id = SelectedProduct.Id, // Ensure ID is preserved for update
            Name = SelectedProduct.Name,
            Sku = SelectedProduct.Sku,
            Barcode = SelectedProduct.Barcode,
            Quantity = SelectedProduct.Quantity,
            Price = SelectedProduct.Price
        };

        // Perform database update operation on a background thread
        await Task.Run(() => _productService.Update(productToUpdate));

        // Update the SourceList on the UI thread
        RxApp.MainThreadScheduler.Schedule(() =>
        {
            var existingProduct = _allProducts.Items.FirstOrDefault(p => p.Id == productToUpdate.Id);
            if (existingProduct != null)
            {
                // DynamicData's Replace method updates the item in the collection
                _allProducts.Replace(existingProduct, productToUpdate);
            }
            ShowNotification("Product updated successfully!");
        });
    }

    public async Task DeleteProductAsync()
    {
        // Client-side validation (redundant with canExecuteIfSelected observable, but good for UX)
        if (SelectedProduct == null || SelectedProduct.Id == 0)
        {
            ShowNotification("Please select a product to delete");
            return;
        }

        var productIdToDelete = SelectedProduct.Id;

        // Perform database delete operation on a background thread
        await Task.Run(() => _productService.Delete(productIdToDelete));

        // Update the SourceList on the UI thread
        RxApp.MainThreadScheduler.Schedule(() =>
        {
            var productToRemove = _allProducts.Items.FirstOrDefault(p => p.Id == productIdToDelete);
            if (productToRemove != null)
            {
                _allProducts.Remove(productToRemove);
                ShowNotification("Product deleted successfully!");
                NewProduct(); // Reset SelectedProduct, which updates command canExecute states
            }
            else
            {
                ShowNotification("Product not found in list after deletion attempt.");
            }
        });
    }

    public void NewProduct()
    {
        SelectedProduct = new Product(); // Assign a new, empty ReactiveObject instance
    }

    private void UpdateTimestamp()
    {
        // This method is always called on the MainThreadScheduler due to its subscriptions' ObserveOn
        LastUpdate = DateTime.Now;
        LastUpdateText = $"🔄 Last Updated: {LastUpdate:g}";
    }

    private async void ShowNotification(string message)
    {
        // Ensure all notification UI updates are explicitly on the UI thread
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            NotificationMessage = message;
            IsNotificationVisible = true;
            UpdateTimestamp();
        });

        // Delay the hiding of the notification off the UI thread
        await Task.Delay(3000);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            IsNotificationVisible = false;
        });
    }
}
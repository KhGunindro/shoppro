using shoppro.Models;
using shoppro.Services;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;
using Microsoft.EntityFrameworkCore.Update;

namespace shoppro.ViewModels;

public partial class ProductViewModel : ViewModelBase
{
    private readonly ProductService _productService = new();

    public ObservableCollection<Product> Products { get; } = new();

    private Product _selectedProduct = new();

    public Product SelectedProduct
    {
        get => _selectedProduct;
        set => this.RaiseAndSetIfChanged(ref _selectedProduct, value);
    }

    public ProductViewModel()
    {
        LoadProducts();
        AddCommand = ReactiveCommand.Create(AddProduct);
        UpdateCommand = ReactiveCommand.Create(UpdateProduct);
        DeleteCommand = ReactiveCommand.Create(DeleteProduct);
        NewCommand = ReactiveCommand.Create(NewProduct);
    }

    public void LoadProducts()
    {
        var products = _productService.GetAll();
        Products.Clear();
        foreach (var product in products)
        {
            Products.Add(product);
        }
    }

    public void AddProduct()
    {
        try
        {
            _productService.Add(SelectedProduct);
            LoadProducts();
            NewProduct();
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., show a message to the user)
            Console.WriteLine($"Error adding product: {ex.Message}");
        }
    }

    public void UpdateProduct()
    {
        try
        {
            _productService.Update(SelectedProduct);
            LoadProducts();
            NewProduct();
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., show a message to the user)
            Console.WriteLine($"Error updating product: {ex.Message}");
        }
    }

    public void DeleteProduct()
    {
        try
        {
            if (SelectedProduct?.Id > 0)
            {
                _productService.Delete(SelectedProduct.Id);
                LoadProducts();
                NewProduct();
            }
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., show a message to the user)
            Console.WriteLine($"Error deleting product: {ex.Message}");
        }
    }
    public void NewProduct()
    {
        SelectedProduct = new Product();
    }
    public ReactiveCommand<Unit, Unit> AddCommand { get; }
    public ReactiveCommand<Unit, Unit> UpdateCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteCommand { get; }
    public ReactiveCommand<Unit, Unit> NewCommand { get; }
}
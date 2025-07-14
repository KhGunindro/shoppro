using ReactiveUI;
using shoppro.Models;
using shoppro.Services;
using System.Collections.ObjectModel;

namespace shoppro.ViewModels;

public partial class ProductViewModel : ReactiveObject
{
    private readonly ProductService _productService = new();

    public ObservableCollection<Product> Products { get; } = new();

    public ProductViewModel()
    {
        LoadProducts();
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

}
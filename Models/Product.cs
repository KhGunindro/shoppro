using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ReactiveUI; // IMPORTANT: This line must be present for ReactiveObject

namespace shoppro.Models;

[Table("Products")]
public class Product : ReactiveObject // Inherit from ReactiveObject for INotifyPropertyChanged
{
    private int _id;
    [Key]
    [Column("id")]
    public int Id
    {
        get => _id;
        set => this.RaiseAndSetIfChanged(ref _id, value);
    }

    private string? _name;
    [Column("name")]
    public string? Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    private string? _sku;
    [Column("sku")]
    public string? Sku
    {
        get => _sku;
        set => this.RaiseAndSetIfChanged(ref _sku, value);
    }

    private int _quantity;
    [Column("quantity")]
    public int Quantity
    {
        get => _quantity;
        set => this.RaiseAndSetIfChanged(ref _quantity, value); // Corrected: Was 'ref _id'
    }

    private decimal _price;
    [Column("price")]
    public decimal Price
    {
        get => _price;
        set => this.RaiseAndSetIfChanged(ref _price, value);
    }

    private string? _barcode;
    [Column("barcode")]
    public string? Barcode
    {
        get => _barcode;
        set => this.RaiseAndSetIfChanged(ref _barcode, value);
    }

    public Product()
    {
        // Parameterless constructor for default initialization, if needed.
        // E.g., when you call 'new Product()' in the ViewModel.
    }
}
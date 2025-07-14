using System;
using ReactiveUI;
using System.Reactive;
using Avalonia.Controls;
using shoppro.Views;

namespace shoppro.ViewModels;

public partial class MainWindowViewModel : ReactiveObject
{
    public string Greeting { get; } = "Welcome to shoppro!";

    public string DBStatus { get; set; }
    public string Description { get; } = "Your one-stop solution for managing your shop efficiently.";

    public MainWindowViewModel()
    {
        try
        {
            var db = new Services.DatabaseService();
            using var connection = db.GetConnection();
            DBStatus = "Database connection successful!";
            connection.Close();
        }
        catch (Exception ex)
        {
            DBStatus = $"Database connection failed: {ex.Message}";
        }
    }
    private Control? _currentView;
    public Control? CurrentView
    {
        get => _currentView;
        set => this.RaiseAndSetIfChanged(ref _currentView, value);
    }

    public void NavigateToProducts()
    {
        CurrentView = new ProductView(); // You must have ProductView defined
    }
}
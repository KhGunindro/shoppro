using System;

namespace shoppro.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
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
}
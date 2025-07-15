using System;
using Avalonia.Controls;
using shoppro.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections.ObjectModel;

namespace shoppro.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isPaneOpen = true;

    [RelayCommand]
    private void TriggerPane()
    {
        IsPaneOpen = !IsPaneOpen;
    }

    [ObservableProperty]
    private ListItemTemplate? _selectedListItem;

    partial void OnSelectedListItemChanged(ListItemTemplate? value)
    {
        if (value?.ModelType != null &&
            Activator.CreateInstance(value.ModelType) is ViewModelBase vm)
        {
            CurrentPage = vm;
        }
    }
    public ObservableCollection<ListItemTemplate> Items { get; } = new()
    {
        new ListItemTemplate(typeof(DashboardViewModel)),
        new ListItemTemplate(typeof(ProductViewModel)),
    };

    [ObservableProperty]
    private ViewModelBase? _currentPage = new DashboardViewModel();

}

public class ListItemTemplate
{
    public ListItemTemplate(Type type)
    {
        ModelType = type;
        Label = type.Name.Replace("ViewModel", string.Empty);
    }
    public string? Label { get; }
    public Type? ModelType { get; }
}
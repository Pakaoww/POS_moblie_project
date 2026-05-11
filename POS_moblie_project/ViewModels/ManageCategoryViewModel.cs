using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS_moblie_project.Models;
using POS_moblie_project.Services;
using System.Collections.ObjectModel;

namespace POS_moblie_project.ViewModels;  // ← file-scoped namespace, no braces

public partial class ManageCategoryViewModel : ObservableObject  // ← public partial
{
    private readonly DatabaseService _databaseService;

    [ObservableProperty]
    private ObservableCollection<Category> categories = new();

    [ObservableProperty]
    private bool isLoading;

    public ManageCategoryViewModel()
    {
        _databaseService = ServiceHelper.GetService<DatabaseService>();
    }

    [RelayCommand]
    public async Task LoadCategoriesAsync()
    {
        IsLoading = true;
        try
        {
            var list = await _databaseService.GetAllCategoriesAsync();
            Categories.Clear();
            foreach (var c in list)
                Categories.Add(c);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error", $"Failed to load categories: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    public async Task AddCategoryAsync()
    {
        var name = await Application.Current.MainPage.DisplayPromptAsync(
            "New Category", "Enter category name:", "Add", "Cancel");

        if (string.IsNullOrWhiteSpace(name)) return;

        try
        {
            var nextSort = Categories.Count > 0
                ? Categories.Max(c => c.SortOrder) + 1
                : 0;
            var category = new Category(name.Trim(), nextSort);
            await _databaseService.CreateCategoryAsync(category);
            Categories.Add(category);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error", $"Failed to add category: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task EditCategoryAsync(Category category)
    {
        if (category == null) return;

        var newName = await Application.Current.MainPage.DisplayPromptAsync(
            "Edit Category", "Category name:", "Save", "Cancel",
            initialValue: category.Name);

        if (string.IsNullOrWhiteSpace(newName) || newName == category.Name) return;

        try
        {
            category.Name = newName.Trim();
            await _databaseService.UpdateCategoryAsync(category);

            var index = Categories.IndexOf(category);
            if (index >= 0)
            {
                Categories.RemoveAt(index);
                Categories.Insert(index, category);
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error", $"Failed to update: {ex.Message}", "OK");
        }
    }

    [RelayCommand]
    public async Task DeleteCategoryAsync(Category category)
    {
        if (category == null) return;

        var confirm = await Application.Current.MainPage.DisplayAlert(
            "Delete Category",
            $"Delete \"{category.Name}\"?",
            "Delete", "Cancel");

        if (!confirm) return;

        try
        {
            await _databaseService.DeleteCategoryAsync(category.Id);
            Categories.Remove(category);
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert(
                "Error", $"Failed to delete: {ex.Message}", "OK");
        }
    }
}
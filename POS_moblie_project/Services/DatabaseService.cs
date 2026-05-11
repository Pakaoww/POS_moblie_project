using POS_moblie_project.Models;
using SQLite;

namespace POS_moblie_project.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _database;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _isInitialized = false;
    private const string DatabaseFilename = "mobilepos.db3";

    private static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

    public DatabaseService()
    {
    }

    public async Task InitializeAsync()
    {
        if (_isInitialized)
            return;

        await _initLock.WaitAsync();
        try
        {
            // Double-check inside lock
            if (_isInitialized)
                return;

            var connection = new SQLiteAsyncConnection(
                DatabasePath,
                SQLiteOpenFlags.ReadWrite |
                SQLiteOpenFlags.Create |
                SQLiteOpenFlags.SharedCache);

            // Create all tables before assigning to _database
            await connection.CreateTableAsync<Category>();
            await connection.CreateTableAsync<Product>();
            await connection.CreateTableAsync<Transaction>();
            await connection.CreateTableAsync<TransactionItem>();
            await connection.CreateTableAsync<AppSetting>();

            // Only assign after everything is ready
            _database = connection;
            _isInitialized = true;

            await InitializeDefaultCategoriesAsync();
            await InitializeDefaultSettingsAsync();
        }
        finally
        {
            _initLock.Release();
        }
    }

    private async Task EnsureInitializedAsync()
    {
        if (_isInitialized)
            return;

        await InitializeAsync();
    }

    private async Task InitializeDefaultCategoriesAsync()
    {
        var count = await _database!.Table<Category>().CountAsync();
        if (count == 0)
        {
            var defaults = new[]
            {
                new Category("General", 0),
                new Category("Food & Drinks", 1),
                new Category("Snacks", 2),
                new Category("Other", 99),
            };
            foreach (var c in defaults)
                await _database.InsertAsync(c);
        }
    }

    private async Task InitializeDefaultSettingsAsync()
    {
        var count = await _database!.Table<AppSetting>().CountAsync();
        if (count == 0)
        {
            var defaults = new[]
            {
                new AppSetting("password_hash", string.Empty),
                new AppSetting("vat_enabled", "false"),
                new AppSetting("vat_rate", "7"),
            };
            foreach (var s in defaults)
                await _database.InsertAsync(s);
        }
    }

    // ============================================
    // AppSettings methods
    // ============================================
    public async Task<string> GetSettingAsync(string key)
    {
        await EnsureInitializedAsync();
        var setting = await _database!.Table<AppSetting>()
                                      .Where(s => s.Key == key)
                                      .FirstOrDefaultAsync();
        return setting?.Value ?? string.Empty;
    }

    public async Task SetSettingAsync(string key, string value)
    {
        await EnsureInitializedAsync();
        var existing = await _database!.Table<AppSetting>()
                                       .Where(s => s.Key == key)
                                       .FirstOrDefaultAsync();
        if (existing == null)
            await _database.InsertAsync(new AppSetting(key, value));
        else
        {
            existing.Value = value;
            await _database.UpdateAsync(existing);
        }
    }

    // ============================================
    // Category methods
    // ============================================
    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        await EnsureInitializedAsync();
        return await _database!.Table<Category>()
                               .OrderBy(c => c.SortOrder)
                               .ToListAsync();
    }

    public async Task<Category> GetCategoryAsync(int id)
    {
        await EnsureInitializedAsync();
        return await _database!.GetAsync<Category>(id);
    }

    public async Task<int> CreateCategoryAsync(Category category)
    {
        await EnsureInitializedAsync();
        return await _database!.InsertAsync(category);
    }

    public async Task<int> UpdateCategoryAsync(Category category)
    {
        await EnsureInitializedAsync();
        return await _database!.UpdateAsync(category);
    }

    public async Task<int> DeleteCategoryAsync(int id)
    {
        await EnsureInitializedAsync();
        return await _database!.DeleteAsync<Category>(id);
    }

    // ============================================
    // Product methods
    // ============================================
    public async Task<List<Product>> GetAllProductsAsync()
    {
        await EnsureInitializedAsync();
        return await _database!.Table<Product>()
                               .OrderBy(p => p.Name)
                               .ToListAsync();
    }

    public async Task<List<Product>> GetVisibleProductsAsync()
    {
        await EnsureInitializedAsync();
        return await _database!.Table<Product>()
                               .Where(p => p.IsVisible)
                               .OrderBy(p => p.Name)
                               .ToListAsync();
    }

    public async Task<Product> GetProductAsync(int id)
    {
        await EnsureInitializedAsync();
        return await _database!.GetAsync<Product>(id);
    }

    public async Task<int> CreateProductAsync(Product product)
    {
        await EnsureInitializedAsync();
        product.CreatedAt = DateTime.Now;
        product.UpdatedAt = DateTime.Now;
        return await _database!.InsertAsync(product);
    }

    public async Task<int> UpdateProductAsync(Product product)
    {
        await EnsureInitializedAsync();
        product.UpdatedAt = DateTime.Now;
        return await _database!.UpdateAsync(product);
    }

    public async Task<int> DeleteProductAsync(int id)
    {
        await EnsureInitializedAsync();
        return await _database!.DeleteAsync<Product>(id);
    }

    public async Task ToggleProductVisibilityAsync(int id)
    {
        await EnsureInitializedAsync();
        var product = await _database!.GetAsync<Product>(id);
        if (product == null) return;

        product.IsVisible = !product.IsVisible;
        product.UpdatedAt = DateTime.Now;
        await _database.UpdateAsync(product);
    }

    public async Task<bool> IsProductCodeExistsAsync(string code, int excludeId = 0)
    {
        await EnsureInitializedAsync();
        var existing = await _database!.Table<Product>()
                                       .Where(p => p.ProductCode == code
                                                && p.Id != excludeId)
                                       .FirstOrDefaultAsync();
        return existing != null;
    }
}
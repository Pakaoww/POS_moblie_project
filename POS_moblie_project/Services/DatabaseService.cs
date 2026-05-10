using POS_moblie_project.Models;
using SQLite;

namespace POS_moblie_project.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection _database;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private const string DatabaseFilename = "mobilepos.db3";

    private static string DatabasePath =>
        Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename);

    public DatabaseService()
    {
    }

    public async Task InitializeAsync()
    {
        if (_database != null)
            return;

        _database = new SQLiteAsyncConnection(
            DatabasePath,
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

        // Create tables
        await _database.CreateTableAsync<Category>();
        await _database.CreateTableAsync<Product>();
        await _database.CreateTableAsync<Transaction>();
        await _database.CreateTableAsync<TransactionItem>();
        await _database.CreateTableAsync<AppSetting>();

        // Seed defaults if empty
        await InitializeDefaultCategoriesAsync();
        await InitializeDefaultSettingsAsync();
    }

    private async Task EnsureInitializedAsync()
    {
        if (_database != null)
            return;

        await _initLock.WaitAsync();
        try
        {
            if (_database == null)
                await InitializeAsync();
        }
        finally
        {
            _initLock.Release();
        }
    }

    private async Task InitializeDefaultCategoriesAsync()
    {
        var count = await _database.Table<Category>().CountAsync();
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
        var count = await _database.Table<AppSetting>().CountAsync();
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

        var setting = await _database.Table<AppSetting>()
                                      .Where(s => s.Key == key)
                                      .FirstOrDefaultAsync();
        return setting?.Value ?? string.Empty;
    }

    public async Task SetSettingAsync(string key, string value)
    {
        await EnsureInitializedAsync();

        var existing = await _database.Table<AppSetting>()
                                       .Where(s => s.Key == key)
                                       .FirstOrDefaultAsync();
        if (existing == null)
        {
            await _database.InsertAsync(new AppSetting(key, value));
        }
        else
        {
            existing.Value = value;
            await _database.UpdateAsync(existing);
        }
    }
}
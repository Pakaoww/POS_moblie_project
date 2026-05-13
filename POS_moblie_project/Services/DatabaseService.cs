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
                new AppSetting("show_currency_symbol", "true"),   // เพิ่ม
                new AppSetting("currency_symbol", "฿"),           // เพิ่ม
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

    // ============================================
    // Transaction methods
    // ============================================
    public async Task<int> CreateTransactionAsync(
        Transaction transaction, List<TransactionItem> items)
    {
        await EnsureInitializedAsync();

        // Atomic: insert transaction + all items + decrement stock
        await _database!.RunInTransactionAsync(db =>
        {
            db.Insert(transaction);

            foreach (var item in items)
            {
                item.TransactionId = transaction.Id;
                db.Insert(item);

                // Decrement stock
                var product = db.Get<Product>(item.ProductId);
                if (product != null)
                {
                    product.Stock -= item.Quantity;
                    product.UpdatedAt = DateTime.Now;
                    db.Update(product);
                }
            }
        });

        return transaction.Id;
    }

    public async Task<string> GenerateTransactionIdAsync()
    {
        await EnsureInitializedAsync();

        var today = DateTime.Now;
        var datePrefix = today.ToString("yyyyMMdd");

        // นับจำนวน transaction ของวันนี้
        var startOfDay = today.Date;
        var endOfDay = today.Date.AddDays(1);

        var countToday = await _database!.Table<Transaction>()
                                          .Where(t => t.Timestamp >= startOfDay
                                                   && t.Timestamp < endOfDay)
                                          .CountAsync();

        // เลขที่คำสั่งซื้อเริ่มที่ 0001
        var orderNumber = (countToday + 1).ToString("D4");

        return $"{datePrefix}{orderNumber}";
    }

    // ============================================
    // Report methods
    // ============================================
    public async Task<List<Transaction>> GetTransactionsAsync(
        DateTime from, DateTime to)
    {
        await EnsureInitializedAsync();
        return await _database!.Table<Transaction>()
                               .Where(t => t.Timestamp >= from
                                        && t.Timestamp <= to)
                               .OrderByDescending(t => t.Timestamp)
                               .ToListAsync();
    }

    public async Task<(Transaction transaction, List<TransactionItem> items)>
        GetTransactionWithItemsAsync(string transactionId)
    {
        await EnsureInitializedAsync();
        var transaction = await _database!.Table<Transaction>()
                                          .Where(t => t.TransactionId == transactionId)
                                          .FirstOrDefaultAsync();
        var items = await _database!.Table<TransactionItem>()
                                    .Where(i => i.TransactionId == transaction.Id)
                                    .ToListAsync();
        return (transaction, items);
    }

    public async Task<List<TransactionItem>> GetTransactionItemsByProductAsync(
        int productId, DateTime from, DateTime to)
    {
        await EnsureInitializedAsync();

        // Get transaction IDs within date range first
        var transactions = await _database!.Table<Transaction>()
                                           .Where(t => t.Timestamp >= from
                                                    && t.Timestamp <= to)
                                           .ToListAsync();
        var transactionIds = transactions.Select(t => t.Id).ToList();

        // Get items for this product within those transactions
        var allItems = await _database!.Table<TransactionItem>()
                                       .Where(i => i.ProductId == productId)
                                       .ToListAsync();

        return allItems.Where(i => transactionIds.Contains(i.TransactionId)).ToList();
    }

    public async Task<List<SalesReportItem>> GetSalesReportAsync(
        DateTime from, DateTime to)
    {
        await EnsureInitializedAsync();

        var transactions = await _database!.Table<Transaction>()
                                           .Where(t => t.Timestamp >= from
                                                    && t.Timestamp <= to)
                                           .ToListAsync();
        var transactionIds = transactions.Select(t => t.Id).ToList();

        var allItems = await _database!.Table<TransactionItem>().ToListAsync();
        var filteredItems = allItems
            .Where(i => transactionIds.Contains(i.TransactionId))
            .ToList();

        var products = await _database!.Table<Product>().ToListAsync();

        var report = filteredItems
            .GroupBy(i => i.ProductId)
            .Select(g =>
            {
                var product = products.FirstOrDefault(p => p.Id == g.Key);
                return new SalesReportItem
                {
                    ProductId = g.Key,
                    ProductCode = product?.ProductCode ?? string.Empty,
                    ProductName = g.First().ProductName,
                    ImagePath = product?.ImagePath ?? string.Empty,
                    TotalQuantity = g.Sum(i => i.Quantity),
                    TotalRevenue = g.Sum(i => i.Subtotal)
                };
            })
            .OrderByDescending(r => r.TotalQuantity)
            .ToList();

        return report;
    }

    public async Task<List<TransactionItem>> GetAllTransactionItemsAsync()
    {
        await EnsureInitializedAsync();
        return await _database!.Table<TransactionItem>().ToListAsync();
    }
}
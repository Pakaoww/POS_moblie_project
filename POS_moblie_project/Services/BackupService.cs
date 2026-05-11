using ClosedXML.Excel;
using POS_moblie_project.Models;
using POS_moblie_project.ViewModels;
using SQLite;

namespace POS_moblie_project.Services;

public class BackupService
{
    private readonly DatabaseService _databaseService;

    public BackupService()
    {
        _databaseService = ServiceHelper.GetService<DatabaseService>();
    }

    // ============================================
    // EXPORT — สร้างไฟล์ Excel จากข้อมูลทั้งหมด
    // ============================================
    public async Task<string> ExportToExcelAsync()
    {
        var products = await _databaseService.GetAllProductsAsync();
        var categories = await _databaseService.GetAllCategoriesAsync();
        var transactions = await _databaseService.GetTransactionsAsync(
            DateTime.MinValue, DateTime.MaxValue);
        var transactionItems = await GetAllTransactionItemsAsync();

        using var workbook = new XLWorkbook();

        // ── Sheet 1: Categories ──
        var catSheet = workbook.Worksheets.Add("Categories");
        catSheet.Cell(1, 1).Value = "Id";
        catSheet.Cell(1, 2).Value = "Name";
        catSheet.Cell(1, 3).Value = "SortOrder";
        catSheet.Cell(1, 4).Value = "CreatedAt";

        for (int i = 0; i < categories.Count; i++)
        {
            var c = categories[i];
            catSheet.Cell(i + 2, 1).Value = c.Id;
            catSheet.Cell(i + 2, 2).Value = c.Name;
            catSheet.Cell(i + 2, 3).Value = c.SortOrder;
            catSheet.Cell(i + 2, 4).Value = c.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
        }
        catSheet.Row(1).Style.Font.Bold = true;
        catSheet.Columns().AdjustToContents();

        // ── Sheet 2: Products ──
        var prodSheet = workbook.Worksheets.Add("Products");
        prodSheet.Cell(1, 1).Value = "Id";
        prodSheet.Cell(1, 2).Value = "ProductCode";
        prodSheet.Cell(1, 3).Value = "Name";
        prodSheet.Cell(1, 4).Value = "CategoryId";
        prodSheet.Cell(1, 5).Value = "Price";
        prodSheet.Cell(1, 6).Value = "Stock";
        prodSheet.Cell(1, 7).Value = "IsVisible";
        prodSheet.Cell(1, 8).Value = "ImagePath";
        prodSheet.Cell(1, 9).Value = "CreatedAt";
        prodSheet.Cell(1, 10).Value = "UpdatedAt";

        for (int i = 0; i < products.Count; i++)
        {
            var p = products[i];
            prodSheet.Cell(i + 2, 1).Value = p.Id;
            prodSheet.Cell(i + 2, 2).Value = p.ProductCode;
            prodSheet.Cell(i + 2, 3).Value = p.Name;
            prodSheet.Cell(i + 2, 4).Value = p.CategoryId;
            prodSheet.Cell(i + 2, 5).Value = (double)p.Price;
            prodSheet.Cell(i + 2, 6).Value = p.Stock;
            prodSheet.Cell(i + 2, 7).Value = p.IsVisible;
            prodSheet.Cell(i + 2, 8).Value = p.ImagePath;
            prodSheet.Cell(i + 2, 9).Value = p.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
            prodSheet.Cell(i + 2, 10).Value = p.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss");
        }
        prodSheet.Row(1).Style.Font.Bold = true;
        prodSheet.Columns().AdjustToContents();

        // ── Sheet 3: Transactions ──
        var txSheet = workbook.Worksheets.Add("Transactions");
        txSheet.Cell(1, 1).Value = "Id";
        txSheet.Cell(1, 2).Value = "TransactionId";
        txSheet.Cell(1, 3).Value = "Timestamp";
        txSheet.Cell(1, 4).Value = "TotalAmount";
        txSheet.Cell(1, 5).Value = "VatRate";
        txSheet.Cell(1, 6).Value = "VatAmount";
        txSheet.Cell(1, 7).Value = "GrandTotal";
        txSheet.Cell(1, 8).Value = "MoneyReceived";
        txSheet.Cell(1, 9).Value = "Change";

        for (int i = 0; i < transactions.Count; i++)
        {
            var t = transactions[i];
            txSheet.Cell(i + 2, 1).Value = t.Id;
            txSheet.Cell(i + 2, 2).Value = t.TransactionId;
            txSheet.Cell(i + 2, 3).Value = t.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
            txSheet.Cell(i + 2, 4).Value = (double)t.TotalAmount;
            txSheet.Cell(i + 2, 5).Value = (double)t.VatRate;
            txSheet.Cell(i + 2, 6).Value = (double)t.VatAmount;
            txSheet.Cell(i + 2, 7).Value = (double)t.GrandTotal;
            txSheet.Cell(i + 2, 8).Value = (double)t.MoneyReceived;
            txSheet.Cell(i + 2, 9).Value = (double)t.Change;
        }
        txSheet.Row(1).Style.Font.Bold = true;
        txSheet.Columns().AdjustToContents();

        // ── Sheet 4: TransactionItems ──
        var itemSheet = workbook.Worksheets.Add("TransactionItems");
        itemSheet.Cell(1, 1).Value = "Id";
        itemSheet.Cell(1, 2).Value = "TransactionId";
        itemSheet.Cell(1, 3).Value = "ProductId";
        itemSheet.Cell(1, 4).Value = "ProductName";
        itemSheet.Cell(1, 5).Value = "UnitPrice";
        itemSheet.Cell(1, 6).Value = "Quantity";
        itemSheet.Cell(1, 7).Value = "Subtotal";

        for (int i = 0; i < transactionItems.Count; i++)
        {
            var item = transactionItems[i];
            itemSheet.Cell(i + 2, 1).Value = item.Id;
            itemSheet.Cell(i + 2, 2).Value = item.TransactionId;
            itemSheet.Cell(i + 2, 3).Value = item.ProductId;
            itemSheet.Cell(i + 2, 4).Value = item.ProductName;
            itemSheet.Cell(i + 2, 5).Value = (double)item.UnitPrice;
            itemSheet.Cell(i + 2, 6).Value = item.Quantity;
            itemSheet.Cell(i + 2, 7).Value = (double)item.Subtotal;
        }
        itemSheet.Row(1).Style.Font.Bold = true;
        itemSheet.Columns().AdjustToContents();

        // ── บันทึกไฟล์ ──
        var fileName = $"POS_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        var filePath = Path.Combine(
            FileSystem.Current.CacheDirectory, fileName);

        using var stream = File.Create(filePath);
        workbook.SaveAs(stream);

        return filePath;
    }

    // ============================================
    // BACKUP — คัดลอกไฟล์ .db3 ทั้งหมด
    // ============================================
    public async Task<string> BackupDatabaseAsync()
    {
        var dbPath = Path.Combine(
            FileSystem.AppDataDirectory, "mobilepos.db3");

        var backupName = $"POS_DB_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.db3";
        var backupPath = Path.Combine(
            FileSystem.Current.CacheDirectory, backupName);

        await Task.Run(() => File.Copy(dbPath, backupPath, overwrite: true));

        return backupPath;
    }

    // ============================================
    // IMPORT — restore จากไฟล์ .db3
    // ============================================
    public async Task<bool> ImportDatabaseAsync(string sourceFilePath)
    {
        try
        {
            var dbPath = Path.Combine(
                FileSystem.AppDataDirectory, "mobilepos.db3");

            await Task.Run(() => File.Copy(sourceFilePath, dbPath, overwrite: true));

            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<List<TransactionItem>> GetAllTransactionItemsAsync()
    => await _databaseService.GetAllTransactionItemsAsync();

    

}
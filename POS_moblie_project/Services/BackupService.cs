using ClosedXML.Excel;
using POS_moblie_project.Models;
using POS_moblie_project.ViewModels;

namespace POS_moblie_project.Services;

public class BackupService
{
    private readonly DatabaseService _databaseService;

    public BackupService()
    {
        _databaseService = ServiceHelper.GetService<DatabaseService>();
    }

    public async Task<string> ExportToExcelAsync()
    {
        var products = await _databaseService.GetAllProductsAsync();
        var categories = await _databaseService.GetAllCategoriesAsync();
        var lots = await _databaseService.GetAllLotsAsync();
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
        prodSheet.Cell(1, 5).Value = "SalePrice";     // ← เปลี่ยนจาก Price
        prodSheet.Cell(1, 6).Value = "IsVisible";     // ← ลบ Stock ออก
        prodSheet.Cell(1, 7).Value = "ImagePath";
        prodSheet.Cell(1, 8).Value = "CreatedAt";
        prodSheet.Cell(1, 9).Value = "UpdatedAt";
        for (int i = 0; i < products.Count; i++)
        {
            var p = products[i];
            prodSheet.Cell(i + 2, 1).Value = p.Id;
            prodSheet.Cell(i + 2, 2).Value = p.ProductCode;
            prodSheet.Cell(i + 2, 3).Value = p.Name;
            prodSheet.Cell(i + 2, 4).Value = p.CategoryId;
            prodSheet.Cell(i + 2, 5).Value = (double)p.SalePrice; // ← เปลี่ยนจาก p.Price
            prodSheet.Cell(i + 2, 6).Value = p.IsVisible;         // ← ลบ p.Stock
            prodSheet.Cell(i + 2, 7).Value = p.ImagePath;
            prodSheet.Cell(i + 2, 8).Value = p.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss");
            prodSheet.Cell(i + 2, 9).Value = p.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss");
        }
        prodSheet.Row(1).Style.Font.Bold = true;
        prodSheet.Columns().AdjustToContents();

        // ── Sheet 3: ProductLots ── (ใหม่)
        var lotSheet = workbook.Worksheets.Add("ProductLots");
        lotSheet.Cell(1, 1).Value = "Id";
        lotSheet.Cell(1, 2).Value = "LotId";
        lotSheet.Cell(1, 3).Value = "ProductId";
        lotSheet.Cell(1, 4).Value = "CostPrice";
        lotSheet.Cell(1, 5).Value = "Quantity";
        lotSheet.Cell(1, 6).Value = "Remaining";
        lotSheet.Cell(1, 7).Value = "ReceivedAt";
        lotSheet.Cell(1, 8).Value = "IsActive";
        for (int i = 0; i < lots.Count; i++)
        {
            var l = lots[i];
            lotSheet.Cell(i + 2, 1).Value = l.Id;
            lotSheet.Cell(i + 2, 2).Value = l.LotId;
            lotSheet.Cell(i + 2, 3).Value = l.ProductId;
            lotSheet.Cell(i + 2, 4).Value = (double)l.CostPrice;
            lotSheet.Cell(i + 2, 5).Value = l.Quantity;
            lotSheet.Cell(i + 2, 6).Value = l.Remaining;
            lotSheet.Cell(i + 2, 7).Value = l.ReceivedAt.ToString("yyyy-MM-dd HH:mm:ss");
            lotSheet.Cell(i + 2, 8).Value = l.IsActive;
        }
        lotSheet.Row(1).Style.Font.Bold = true;
        lotSheet.Columns().AdjustToContents();

        // ── Sheet 4: Transactions ──
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

        // ── Sheet 5: TransactionItems ──
        var itemSheet = workbook.Worksheets.Add("TransactionItems");
        itemSheet.Cell(1, 1).Value = "Id";
        itemSheet.Cell(1, 2).Value = "TransactionId";
        itemSheet.Cell(1, 3).Value = "ProductId";
        itemSheet.Cell(1, 4).Value = "LotId";
        itemSheet.Cell(1, 5).Value = "ProductName";
        itemSheet.Cell(1, 6).Value = "UnitCost";
        itemSheet.Cell(1, 7).Value = "UnitPrice";
        itemSheet.Cell(1, 8).Value = "Quantity";
        itemSheet.Cell(1, 9).Value = "Subtotal";
        for (int i = 0; i < transactionItems.Count; i++)
        {
            var item = transactionItems[i];
            itemSheet.Cell(i + 2, 1).Value = item.Id;
            itemSheet.Cell(i + 2, 2).Value = item.TransactionId;
            itemSheet.Cell(i + 2, 3).Value = item.ProductId;
            itemSheet.Cell(i + 2, 4).Value = item.LotId;
            itemSheet.Cell(i + 2, 5).Value = item.ProductName;
            itemSheet.Cell(i + 2, 6).Value = (double)item.UnitCost;
            itemSheet.Cell(i + 2, 7).Value = (double)item.UnitPrice;
            itemSheet.Cell(i + 2, 8).Value = item.Quantity;
            itemSheet.Cell(i + 2, 9).Value = (double)item.Subtotal;
        }
        itemSheet.Row(1).Style.Font.Bold = true;
        itemSheet.Columns().AdjustToContents();

        var fileName = $"POS_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        var filePath = Path.Combine(FileSystem.Current.CacheDirectory, fileName);
        using var stream = File.Create(filePath);
        workbook.SaveAs(stream);

        return filePath;
    }

    public async Task<string> ExportTransactionHistoryAsync(
        List<Transaction> transactions)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Transaction History");

        sheet.Cell(1, 1).Value = "Transaction ID";
        sheet.Cell(1, 2).Value = "Date";
        sheet.Cell(1, 3).Value = "Time";
        sheet.Cell(1, 4).Value = "Subtotal";
        sheet.Cell(1, 5).Value = "VAT";
        sheet.Cell(1, 6).Value = "Total";
        sheet.Cell(1, 7).Value = "Received";
        sheet.Cell(1, 8).Value = "Change";
        sheet.Row(1).Style.Font.Bold = true;

        for (int i = 0; i < transactions.Count; i++)
        {
            var t = transactions[i];
            sheet.Cell(i + 2, 1).Value = t.TransactionId;
            sheet.Cell(i + 2, 2).Value = t.Timestamp.ToString("dd/MM/yyyy");
            sheet.Cell(i + 2, 3).Value = t.Timestamp.ToString("HH:mm");
            sheet.Cell(i + 2, 4).Value = (double)t.TotalAmount;
            sheet.Cell(i + 2, 5).Value = (double)t.VatAmount;
            sheet.Cell(i + 2, 6).Value = (double)t.GrandTotal;
            sheet.Cell(i + 2, 7).Value = (double)t.MoneyReceived;
            sheet.Cell(i + 2, 8).Value = (double)t.Change;
        }

        sheet.Columns().AdjustToContents();

        var fileName = $"Transactions_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        var filePath = Path.Combine(FileSystem.Current.CacheDirectory, fileName);
        using var stream = File.Create(filePath);
        workbook.SaveAs(stream);

        return filePath;
    }

    public async Task<string> ExportSalesReportAsync(
        List<SalesReportItem> reportItems, DateTime from, DateTime to)
    {
        using var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add("Sales Report");

        sheet.Cell(1, 1).Value = $"Sales Report: {from:dd/MM/yyyy} - {to:dd/MM/yyyy}";
        sheet.Cell(1, 1).Style.Font.Bold = true;

        sheet.Cell(3, 1).Value = "Product Code";
        sheet.Cell(3, 2).Value = "Product Name";
        sheet.Cell(3, 3).Value = "Total Qty Sold";
        sheet.Cell(3, 4).Value = "Total Revenue";
        sheet.Row(3).Style.Font.Bold = true;

        for (int i = 0; i < reportItems.Count; i++)
        {
            var item = reportItems[i];
            sheet.Cell(i + 4, 1).Value = item.ProductCode;
            sheet.Cell(i + 4, 2).Value = item.ProductName;
            sheet.Cell(i + 4, 3).Value = item.TotalQuantity;
            sheet.Cell(i + 4, 4).Value = (double)item.TotalRevenue;
        }

        var lastRow = reportItems.Count + 5;
        sheet.Cell(lastRow, 2).Value = "TOTAL";
        sheet.Cell(lastRow, 3).Value = reportItems.Sum(i => i.TotalQuantity);
        sheet.Cell(lastRow, 4).Value = (double)reportItems.Sum(i => i.TotalRevenue);
        sheet.Row(lastRow).Style.Font.Bold = true;

        sheet.Columns().AdjustToContents();

        var fileName = $"SalesReport_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        var filePath = Path.Combine(FileSystem.Current.CacheDirectory, fileName);
        using var stream = File.Create(filePath);
        workbook.SaveAs(stream);

        return filePath;
    }

    public async Task<string> BackupDatabaseAsync()
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "mobilepos.db3");
        var backupName = $"POS_DB_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.db3";
        var backupPath = Path.Combine(FileSystem.Current.CacheDirectory, backupName);
        await Task.Run(() => File.Copy(dbPath, backupPath, overwrite: true));
        return backupPath;
    }

    public async Task<bool> ImportDatabaseAsync(string sourceFilePath)
    {
        try
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "mobilepos.db3");
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
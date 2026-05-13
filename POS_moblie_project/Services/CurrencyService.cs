using POS_moblie_project.ViewModels;

namespace POS_moblie_project.Services;

public class CurrencyService
{
    // ไม่ inject ผ่าน constructor — ดึงเองตอนใช้งาน
    private DatabaseService Db => ServiceHelper.GetService<DatabaseService>();

    public bool ShowSymbol { get; private set; } = true;
    public string Symbol { get; private set; } = "฿";

    public event Action? SettingChanged;

    public async Task LoadAsync()
    {
        try
        {
            var show = await Db.GetSettingAsync("show_currency_symbol");
            var symbol = await Db.GetSettingAsync("currency_symbol");

            ShowSymbol = show != "false";
            Symbol = string.IsNullOrWhiteSpace(symbol) ? "฿" : symbol;
        }
        catch
        {
            // ถ้า DB ยังไม่พร้อม ใช้ค่า default ไปก่อน
            ShowSymbol = true;
            Symbol = "฿";
        }
    }

    public async Task SetShowSymbolAsync(bool value)
    {
        ShowSymbol = value;
        await Db.SetSettingAsync("show_currency_symbol", value ? "true" : "false");
        SettingChanged?.Invoke();
    }

    public async Task SetSymbolAsync(string value)
    {
        Symbol = string.IsNullOrWhiteSpace(value) ? "฿" : value.Trim();
        await Db.SetSettingAsync("currency_symbol", Symbol);
        SettingChanged?.Invoke();
    }

    public string Format(decimal amount, string numberFormat = "N2")
    {
        var num = amount.ToString(numberFormat, System.Globalization.CultureInfo.InvariantCulture);
        return ShowSymbol ? $"{Symbol}{num}" : num;
    }
}
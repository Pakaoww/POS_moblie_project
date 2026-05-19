namespace POS_moblie_project.Services;

public static class PageLockService
{
    private const string AdminPinKey = "admin_pin";
    private static readonly HashSet<string> _authorized = new();

    // ── Route constants for locked pages ─────────────────────
    public const string RouteTransactions  = "///transactions";
    public const string RouteReports       = "///reports";
    public const string RouteProfitReport  = "///profitreport";

    // ── Authorization (one-shot) ─────────────────────────────

    public static bool IsAuthorized(string route)
    {
        return _authorized.Contains(route);
    }

    public static void Authorize(string route)
    {
        _authorized.Add(route);
    }

    public static bool ConsumeAuthorization(string route)
    {
        return _authorized.Remove(route);
    }

    // ── Admin PIN helpers ────────────────────────────────────

    public static async Task<bool> VerifyAdminPinAsync(string pin)
    {
        var saved = await SecureStorage.GetAsync(AdminPinKey);
        return saved is null || saved == pin;
    }

    public static async Task<bool> HasAdminPinAsync()
    {
        var saved = await SecureStorage.GetAsync(AdminPinKey);
        return !string.IsNullOrWhiteSpace(saved);
    }
}

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace POS_moblie_project.ViewModels.Settings;

public partial class AdminManagePasswordViewModel : ObservableObject
{
    private const string AdminPinKey = "admin_pin";
    private const int PinLength = 6;

    // ════════════════════════════════════════════════════════
    //  OBSERVABLE PROPERTIES  (เหมือน ManagePasswordViewModel)
    // ════════════════════════════════════════════════════════

    [ObservableProperty]
    private PinStep _currentStep = PinStep.VerifyCurrent;

    [ObservableProperty]
    private string _stepTitle = "Enter current admin PIN";

    [ObservableProperty]
    private string _stepDescription = "Step 1 of 3";

    [ObservableProperty]
    private bool _isError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _enteredPin = string.Empty;

    private string _newPinTemp = string.Empty;

    // ════════════════════════════════════════════════════════
    //  EVENTS
    // ════════════════════════════════════════════════════════

    public event Action<string>? OnPinError;
    public event Action<string>? OnStepChanged;
    public event Action? OnPinSuccess;

    // ════════════════════════════════════════════════════════
    //  COMMANDS
    // ════════════════════════════════════════════════════════

    [RelayCommand]
    private void DigitPressed(string digit)
    {
        if (EnteredPin.Length >= PinLength) return;
        IsError = false;
        ErrorMessage = string.Empty;
        EnteredPin += digit;
    }

    [RelayCommand]
    private void Delete()
    {
        if (EnteredPin.Length == 0) return;
        IsError = false;
        ErrorMessage = string.Empty;
        EnteredPin = EnteredPin[..^1];
    }

    [RelayCommand]
    private async Task ConfirmAsync()
    {
        if (EnteredPin.Length < PinLength)
        {
            IsError = true;
            ErrorMessage = "Please enter all 6 digits";
            OnPinError?.Invoke(ErrorMessage);
            return;
        }

        var pin = EnteredPin;
        EnteredPin = string.Empty;

        switch (CurrentStep)
        {
            case PinStep.VerifyCurrent: await HandleVerifyCurrentAsync(pin); break;
            case PinStep.EnterNew: HandleEnterNew(pin); break;
            case PinStep.ConfirmNew: await HandleConfirmNewAsync(pin); break;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        _newPinTemp = string.Empty;
        await Shell.Current.GoToAsync("..");
    }

    // ════════════════════════════════════════════════════════
    //  STEP HANDLERS
    // ════════════════════════════════════════════════════════

    private async Task HandleVerifyCurrentAsync(string pin)
    {
        var saved = await SecureStorage.GetAsync(AdminPinKey);

        // ถ้ายังไม่มี admin PIN (first-time setup) ให้ข้ามขั้นตอนนี้ได้เลย
        if (saved is not null && saved != pin)
        {
            IsError = true;
            ErrorMessage = "Incorrect admin PIN. Please try again.";
            OnPinError?.Invoke(ErrorMessage);
            return;
        }

        CurrentStep = PinStep.EnterNew;
        StepDescription = "Step 2 of 3";
        StepTitle = "Enter new admin PIN";
        OnStepChanged?.Invoke(StepTitle);
    }

    private void HandleEnterNew(string pin)
    {
        _newPinTemp = pin;
        CurrentStep = PinStep.ConfirmNew;
        StepDescription = "Step 3 of 3";
        StepTitle = "Confirm new admin PIN";
        OnStepChanged?.Invoke(StepTitle);
    }

    private async Task HandleConfirmNewAsync(string pin)
    {
        if (pin != _newPinTemp)
        {
            IsError = true;
            ErrorMessage = "PINs do not match. Try again.";
            _newPinTemp = string.Empty;
            CurrentStep = PinStep.EnterNew;
            StepDescription = "Step 2 of 3";
            StepTitle = "Enter new admin PIN";
            OnPinError?.Invoke(ErrorMessage);
            return;
        }

        await SecureStorage.SetAsync(AdminPinKey, pin);
        _newPinTemp = string.Empty;
        OnPinSuccess?.Invoke();
    }

    // ════════════════════════════════════════════════════════
    //  STATIC HELPER — ใช้จาก SettingsViewModel เหมือนเดิม
    // ════════════════════════════════════════════════════════

    public static async Task<bool> VerifyAdminAsync(string inputPin)
    {
        var saved = await SecureStorage.GetAsync(AdminPinKey);
        return saved is null || saved == inputPin;
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace POS_moblie_project.ViewModels;

public enum PinStep
{
    VerifyCurrent,  // รอบ 1 — ยืนยัน PIN ปัจจุบัน
    EnterNew,       // รอบ 2 — ใส่ PIN ใหม่
    ConfirmNew      // รอบ 3 — ยืนยัน PIN ใหม่
}

public enum ManagePasswordMode
{
    ChangePassword, // มาจากปุ่ม Change Password (3 รอบ)
    ChangePin       // มาจากปุ่ม PIN Code (3 รอบเหมือนกัน)
}

public partial class ManagePasswordViewModel : ObservableObject
{
    // ════════════════════════════════════════════════════════
    //  STEP STATE
    // ════════════════════════════════════════════════════════

    [ObservableProperty]
    private PinStep _currentStep = PinStep.VerifyCurrent;

    [ObservableProperty]
    private string _stepTitle = "Enter current PIN";

    [ObservableProperty]
    private string _stepDescription = "Step 1 of 3";

    [ObservableProperty]
    private bool _isError;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    // Mode ที่รับมาจาก Settings
    private ManagePasswordMode _mode = ManagePasswordMode.ChangePassword;

    // เก็บ PIN ใหม่ชั่วคราว
    private string _newPinTemp = string.Empty;

    // ════════════════════════════════════════════════════════
    //  PIN INPUT
    // ════════════════════════════════════════════════════════

    [ObservableProperty]
    private string _enteredPin = string.Empty;

    // ════════════════════════════════════════════════════════
    //  EVENTS → ManagePasswordPage subscribe
    // ════════════════════════════════════════════════════════

    public event Action<string>? OnPinError;
    public event Action<string>? OnStepChanged;
    public event Action? OnPinSuccess;

    // ════════════════════════════════════════════════════════
    //  INIT — เรียกจาก Page พร้อมบอก mode
    // ════════════════════════════════════════════════════════

    [RelayCommand]
    private void Initialize(ManagePasswordMode mode)
    {
        _mode = mode;

        CurrentStep = PinStep.VerifyCurrent;
        StepDescription = "Step 1 of 3";
        IsError = false;
        ErrorMessage = string.Empty;
        EnteredPin = string.Empty;
        _newPinTemp = string.Empty;

        // Title ต่างกันตาม mode
        StepTitle = _mode == ManagePasswordMode.ChangePin
            ? "Enter current PIN"
            : "Enter current PIN";
    }

    // ════════════════════════════════════════════════════════
    //  KEYPAD COMMANDS
    // ════════════════════════════════════════════════════════

    [RelayCommand]
    private void DigitPressed(string digit)
    {
        if (EnteredPin.Length >= 6) return;
        IsError = false;
        ErrorMessage = string.Empty;
        EnteredPin += digit;

        if (EnteredPin.Length == 6)
            ConfirmCommand.Execute(null);
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
        if (EnteredPin.Length < 6)
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
        var saved = await SecureStorage.GetAsync("user_pin");
        bool isValid = saved is null || saved == pin;

        if (!isValid)
        {
            IsError = true;
            ErrorMessage = "Incorrect PIN. Please try again.";
            OnPinError?.Invoke(ErrorMessage);
            return;
        }

        // ผ่าน → ไปรอบ 2
        CurrentStep = PinStep.EnterNew;
        StepDescription = "Step 2 of 3";
        StepTitle = _mode == ManagePasswordMode.ChangePin
            ? "Enter new PIN"
            : "Enter new PIN";
        OnStepChanged?.Invoke(StepTitle);
    }

    private void HandleEnterNew(string pin)
    {
        _newPinTemp = pin;
        CurrentStep = PinStep.ConfirmNew;
        StepDescription = "Step 3 of 3";
        StepTitle = _mode == ManagePasswordMode.ChangePin
            ? "Confirm new PIN"
            : "Confirm new PIN";
        OnStepChanged?.Invoke(StepTitle);
    }

    private async Task HandleConfirmNewAsync(string pin)
    {
        if (pin != _newPinTemp)
        {
            IsError = true;
            ErrorMessage = "PINs do not match. Try again.";

            // กลับรอบ 2
            _newPinTemp = string.Empty;
            CurrentStep = PinStep.EnterNew;
            StepDescription = "Step 2 of 3";
            StepTitle = "Enter new PIN";
            OnPinError?.Invoke(ErrorMessage);
            return;
        }

        // บันทึก
        var storageKey = _mode == ManagePasswordMode.ChangePin
            ? "user_pin"
            : "user_pin";

        await SecureStorage.SetAsync(storageKey, pin);
        _newPinTemp = string.Empty;
        OnPinSuccess?.Invoke();
    }
}
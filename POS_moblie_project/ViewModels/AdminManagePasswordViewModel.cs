using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS_moblie_project.Services;

namespace POS_moblie_project.ViewModels.Settings;

public partial class AdminManagePasswordViewModel : ObservableObject
{
    private const string AdminPinKey = "admin_pin";
    private const int PinLength = 6;

    // ════════════════════════════════════════════════════════
    //  MODE — รับจาก AdminPanelViewModel
    // ════════════════════════════════════════════════════════

    /// <summary>
    /// SetNew  = สร้าง PIN ครั้งแรก (2 รอบ: EnterNew + ConfirmNew)
    /// Change  = เปลี่ยน PIN (3 รอบ: VerifyCurrent + EnterNew + ConfirmNew)
    /// </summary>
    public enum AdminPinMode { SetNew, Change }

    private AdminPinMode _mode = AdminPinMode.Change;

    // ════════════════════════════════════════════════════════
    //  STATE
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
    //  INIT — เรียกจาก Page พร้อมบอก mode
    // ════════════════════════════════════════════════════════

    public void Initialize(AdminPinMode mode)
    {
        _mode = mode;
        _newPinTemp = string.Empty;
        EnteredPin = string.Empty;
        IsError = false;
        ErrorMessage = string.Empty;

        if (_mode == AdminPinMode.SetNew)
        {
            // ข้ามรอบ VerifyCurrent ไปเลย
            CurrentStep = PinStep.EnterNew;
            StepTitle = "Set new admin PIN";
            StepDescription = "Step 1 of 2";
        }
        else
        {
            CurrentStep = PinStep.VerifyCurrent;
            StepTitle = "Enter current admin PIN";
            StepDescription = "Step 1 of 3";
        }
    }


    /// <summary>Reset เฉพาะ input ไม่ reset mode และ step</summary>
    public void ResetInput()
    {
        EnteredPin = string.Empty;
        IsError = false;
        ErrorMessage = string.Empty;
    }

    // ════════════════════════════════════════════════════════
    //  KEYPAD COMMANDS
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

        // step description ต่างกันตาม mode
        StepDescription = _mode == AdminPinMode.SetNew ? "Step 2 of 2" : "Step 3 of 3";
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

            StepDescription = _mode == AdminPinMode.SetNew ? "Step 1 of 2" : "Step 2 of 3";
            StepTitle = _mode == AdminPinMode.SetNew ? "Set new admin PIN" : "Enter new admin PIN";

            OnPinError?.Invoke(ErrorMessage);
            return;
        }

        await SecureStorage.SetAsync(AdminPinKey, pin);
        _newPinTemp = string.Empty;

        var successMsg = _mode == AdminPinMode.SetNew
            ? "Admin Panel is now locked with your PIN."
            : "Your admin PIN has been changed successfully.";

        await AppAlert.ShowSuccessAsync("PIN Updated", successMsg);

        OnPinSuccess?.Invoke();
        await Shell.Current.GoToAsync("..");
    }

}
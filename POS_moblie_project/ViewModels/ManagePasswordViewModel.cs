using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace POS_moblie_project.ViewModels;

public enum PinStep
{
    VerifyCurrent,  // รอบ 1 — ยืนยัน PIN ปัจจุบัน
    EnterNew,       // รอบ 2 — ใส่ PIN ใหม่
    ConfirmNew      // รอบ 3 — ยืนยัน PIN ใหม่
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

    // เก็บ PIN ใหม่ชั่วคราวระหว่างรอบ 2 → 3
    private string _newPinTemp = string.Empty;

    // ════════════════════════════════════════════════════════
    //  PIN INPUT STATE
    // ════════════════════════════════════════════════════════

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EnteredPin))]
    private string _enteredPin = string.Empty;

    // ════════════════════════════════════════════════════════
    //  EVENTS → ManagePasswordPage subscribe
    // ════════════════════════════════════════════════════════

    /// <summary>PIN ผิด — ส่ง error message ไปให้ Page ทำ shake animation</summary>
    public event Action<string>? OnPinError;

    /// <summary>เปลี่ยน step สำเร็จ — Page ทำ fade animation</summary>
    public event Action<string>? OnStepChanged;

    /// <summary>เปลี่ยน PIN สำเร็จทั้ง 3 รอบ</summary>
    public event Action? OnPinSuccess;

    // ════════════════════════════════════════════════════════
    //  INIT
    // ════════════════════════════════════════════════════════

    [RelayCommand]
    private void Initialize()
    {
        CurrentStep = PinStep.VerifyCurrent;
        StepTitle = "Enter current PIN";
        StepDescription = "Step 1 of 3";
        IsError = false;
        ErrorMessage = string.Empty;
        EnteredPin = string.Empty;
        _newPinTemp = string.Empty;
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

        // Auto-confirm เมื่อครบ 6 หลัก
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
        EnteredPin = string.Empty; // clear ก่อน process

        switch (CurrentStep)
        {
            case PinStep.VerifyCurrent:
                await HandleVerifyCurrentAsync(pin);
                break;

            case PinStep.EnterNew:
                HandleEnterNew(pin);
                break;

            case PinStep.ConfirmNew:
                await HandleConfirmNewAsync(pin);
                break;
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

    // รอบ 1 — ตรวจสอบ PIN ปัจจุบัน
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
        StepTitle = "Enter new PIN";
        StepDescription = "Step 2 of 3";
        OnStepChanged?.Invoke(StepTitle);
    }

    // รอบ 2 — รับ PIN ใหม่
    private void HandleEnterNew(string pin)
    {
        _newPinTemp = pin;

        CurrentStep = PinStep.ConfirmNew;
        StepTitle = "Confirm new PIN";
        StepDescription = "Step 3 of 3";
        OnStepChanged?.Invoke(StepTitle);
    }

    // รอบ 3 — ยืนยัน PIN ใหม่
    private async Task HandleConfirmNewAsync(string pin)
    {
        if (pin != _newPinTemp)
        {
            IsError = true;
            ErrorMessage = "PINs do not match. Try again.";

            // กลับไปรอบ 2 ใหม่
            _newPinTemp = string.Empty;
            CurrentStep = PinStep.EnterNew;
            StepTitle = "Enter new PIN";
            StepDescription = "Step 2 of 3";
            OnPinError?.Invoke(ErrorMessage);
            return;
        }

        // บันทึก PIN ใหม่
        await SecureStorage.SetAsync("user_pin", pin);
        _newPinTemp = string.Empty;
        OnPinSuccess?.Invoke();
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS_moblie_project.Services;

namespace POS_moblie_project.ViewModels;

public enum PinStep
{
    VerifyCurrent,
    EnterNew,
    ConfirmNew
}

public enum ManagePasswordMode
{
    ChangePassword,
    ChangePin
}

public partial class ManagePasswordViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;

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

    [ObservableProperty]
    private string _enteredPin = string.Empty;

    private ManagePasswordMode _mode = ManagePasswordMode.ChangePassword;
    private string _newPinTemp = string.Empty;

    public event Action<string>? OnPinError;
    public event Action<string>? OnStepChanged;
    public event Action? OnPinSuccess;

    public ManagePasswordViewModel()
    {
        _databaseService = ServiceHelper.GetService<DatabaseService>();
    }

    [RelayCommand]
    private void Initialize(ManagePasswordMode mode)
    {
        _mode = mode;
        CurrentStep = PinStep.VerifyCurrent;
        StepDescription = "Step 1 of 3";
        StepTitle = "Enter current PIN";
        IsError = false;
        ErrorMessage = string.Empty;
        EnteredPin = string.Empty;
        _newPinTemp = string.Empty;
    }

    [RelayCommand]
    private void DigitPressed(string digit)
    {
        if (EnteredPin.Length >= 6) return;
        IsError = false;
        ErrorMessage = string.Empty;
        EnteredPin += digit;

        // No auto-submit — user must press OK explicitly
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

    private async Task HandleVerifyCurrentAsync(string pin)
    {
        var storedHash = await _databaseService.GetSettingAsync("password_hash");
        var enteredHash = PasswordViewModel.HashPin(pin);

        if (storedHash != enteredHash)
        {
            IsError = true;
            ErrorMessage = "Incorrect PIN. Please try again.";
            OnPinError?.Invoke(ErrorMessage);
            return;
        }

        CurrentStep = PinStep.EnterNew;
        StepDescription = "Step 2 of 3";
        StepTitle = "Enter new PIN";
        OnStepChanged?.Invoke(StepTitle);
    }

    private void HandleEnterNew(string pin)
    {
        _newPinTemp = pin;
        CurrentStep = PinStep.ConfirmNew;
        StepDescription = "Step 3 of 3";
        StepTitle = "Confirm new PIN";
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
            StepTitle = "Enter new PIN";
            OnPinError?.Invoke(ErrorMessage);
            return;
        }

        var newHash = PasswordViewModel.HashPin(pin);
        await _databaseService.SetSettingAsync("password_hash", newHash);

        _newPinTemp = string.Empty;
        OnPinSuccess?.Invoke();
    }
}
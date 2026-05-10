using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using POS_moblie_project.Services;
using System.Security.Cryptography;
using System.Text;

namespace POS_moblie_project.ViewModels;

public partial class PasswordViewModel : ObservableObject
{
    private readonly DatabaseService _databaseService;
    private const int PinLength = 6;

    [ObservableProperty]
    private string enteredPin = string.Empty;

    [ObservableProperty]
    private string statusMessage = "Enter password";

    [ObservableProperty]
    private bool isError;

    [ObservableProperty]
    private bool isFirstTimeSetup;

    private string _firstPinAttempt = string.Empty;
    private bool _isConfirmingNewPin;

    public PasswordViewModel()
    {
        _databaseService = ServiceHelper.GetService<DatabaseService>();
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        var storedHash = await _databaseService.GetSettingAsync("password_hash");
        IsFirstTimeSetup = string.IsNullOrEmpty(storedHash);

        StatusMessage = IsFirstTimeSetup
            ? "Create a 6-digit PIN"
            : "Enter password";
    }

    [RelayCommand]
    private void DigitPressed(string digit)
    {
        if (EnteredPin.Length >= PinLength)
            return;

        // Clear error on new input
        if (IsError)
        {
            IsError = false;
            EnteredPin = string.Empty;
        }

        EnteredPin += digit;
    }

    [RelayCommand]
    private void Delete()
    {
        if (IsError)
        {
            IsError = false;
            EnteredPin = string.Empty;
            return;
        }

        if (EnteredPin.Length > 0)
        {
            EnteredPin = EnteredPin.Substring(0, EnteredPin.Length - 1);
        }
    }

    [RelayCommand]
    private async Task ConfirmAsync()
    {
        // Require full 6-digit PIN before submitting
        if (EnteredPin.Length != PinLength)
        {
            ShowError($"PIN must be {PinLength} digits");
            return;
        }

        if (IsFirstTimeSetup)
        {
            await HandleFirstTimeSetupAsync();
        }
        else
        {
            await HandleLoginAsync();
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        EnteredPin = string.Empty;
        IsError = false;

        if (_isConfirmingNewPin)
        {
            _firstPinAttempt = string.Empty;
            _isConfirmingNewPin = false;
            StatusMessage = "Create a 6-digit PIN";
        }
    }

    private async Task HandleFirstTimeSetupAsync()
    {
        if (!_isConfirmingNewPin)
        {
            _firstPinAttempt = EnteredPin;
            _isConfirmingNewPin = true;
            EnteredPin = string.Empty;
            StatusMessage = "Confirm your PIN";
        }
        else
        {
            if (EnteredPin == _firstPinAttempt)
            {
                var hash = HashPin(EnteredPin);
                await _databaseService.SetSettingAsync("password_hash", hash);
                NavigateToShell();
            }
            else
            {
                ShowError("PINs don't match. Try again.");
                _firstPinAttempt = string.Empty;
                _isConfirmingNewPin = false;
                await Task.Delay(1200);
                StatusMessage = "Create a 6-digit PIN";
            }
        }
    }

    private async Task HandleLoginAsync()
    {
        var storedHash = await _databaseService.GetSettingAsync("password_hash");
        var enteredHash = HashPin(EnteredPin);

        if (storedHash == enteredHash)
        {
            NavigateToShell();
        }
        else
        {
            ShowError("Incorrect password");
        }
    }

    private void ShowError(string message)
    {
        IsError = true;
        StatusMessage = message;
    }

    private void NavigateToShell()
    {
        Application.Current.MainPage = new AppShell();
    }

    public static string HashPin(string pin)
    {
        var bytes = Encoding.UTF8.GetBytes(pin);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureDailyJournal.Services;
using System.Windows.Input;

namespace SecureDailyJournal.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly SecurityService _securityService;
        private readonly IServiceProvider _serviceProvider;

        [ObservableProperty]
        private string _pin = "";

        [ObservableProperty]
        private string _statusMessage = "Enter PIN";

        [ObservableProperty]
        private bool _isSetupMode;

        [ObservableProperty]
        private string _titleText = "Welcome";

        public LoginViewModel(SecurityService securityService, IServiceProvider serviceProvider)
        {
            _securityService = securityService;
            _serviceProvider = serviceProvider;
            CheckStatus();
        }

        private async void CheckStatus()
        {
            IsSetupMode = !await _securityService.IsPinSetAsync();
            if (IsSetupMode)
            {
                TitleText = "Setup New PIN";
                StatusMessage = "Create a 4-digit PIN";
            }
            else
            {
                TitleText = "Secure Journal";
                StatusMessage = "Enter Your PIN";
            }
        }

        [RelayCommand]
        public void AddDigit(string digit)
        {
            if (Pin.Length < 4)
            {
                Pin += digit;
                if (Pin.Length == 4)
                {
                    SubmitPin();
                }
            }
        }

        [RelayCommand]
        public void Clear()
        {
            Pin = "";
            StatusMessage = IsSetupMode ? "Create a 4-digit PIN" : "Enter Your PIN";
        }

        private async void SubmitPin()
        {
            if (IsSetupMode)
            {
                await _securityService.SetPinAsync(Pin);
                IsSetupMode = false;
                Pin = "";
                TitleText = "Secure Journal";
                StatusMessage = "PIN Set! Enter to Login";
                // Optionally auto-login or ask to verify. 
                // For simplicity, let them re-enter to confirm logic is simpler.
            }
            else
            {
                bool isValid = await _securityService.VerifyPinAsync(Pin);
                if (isValid)
                {
                    StatusMessage = "Success!";
                    // Navigate to Main App
                    if (Application.Current != null)
                    {
                        Application.Current.MainPage = _serviceProvider.GetRequiredService<AppShell>();
                    }
                }
                else
                {
                    StatusMessage = "Incorrect PIN";
                    Pin = "";
                }
            }
        }
    }
}

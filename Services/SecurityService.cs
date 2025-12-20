using System.Security.Cryptography;
using System.Text;
using SecureDailyJournal.Models;

namespace SecureDailyJournal.Services
{
    public class SecurityService
    {
        private readonly DatabaseService _databaseService;

        public SecurityService(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public async Task<bool> IsPinSetAsync()
        {
            var user = await _databaseService.GetUserAsync();
            return user != null && !string.IsNullOrEmpty(user.PinHash);
        }

        public async Task SetPinAsync(string pin)
        {
            var hash = HashPin(pin);
            var user = await _databaseService.GetUserAsync();
            if (user == null)
            {
                user = new User { PinHash = hash };
            }
            else
            {
                user.PinHash = hash;
            }
            await _databaseService.SaveUserAsync(user);
        }

        public async Task<bool> VerifyPinAsync(string pin)
        {
            var user = await _databaseService.GetUserAsync();
            if (user == null) return false;

            var hash = HashPin(pin);
            return user.PinHash == hash;
        }

        private string HashPin(string pin)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(pin);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}

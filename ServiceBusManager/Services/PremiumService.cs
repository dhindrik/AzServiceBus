using System;
namespace ServiceBusManager.Services
{
    public sealed class PremiumService : IPremiumService
    {
        public PremiumService()
        {
        }

        public event EventHandler? PremiumChanged;

        public void AddPremium()
        {
            Preferences.Default.Set(Constants.Premium, DateTime.MaxValue);

            PremiumChanged?.Invoke(this, EventArgs.Empty);
        }

        public void AddPremium(DateTime validTo)
        {
            Preferences.Default.Set(Constants.Premium, validTo);

            PremiumChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool HasPremium()
        {
            if (Preferences.ContainsKey(Constants.Premium))
            {
                var validTo = Preferences.Default.Get<DateTime>(Constants.Premium, DateTime.MinValue);
 
                if (validTo >= DateTime.Now)
                {
                    return true;
                }
            }

            return false;
        }
    }
}


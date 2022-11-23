
namespace ServiceBusManager.Services;

public interface IPremiumService
{
    bool HasPremium();
    void AddPremium();
    void AddPremium(DateTime validTo);

    event EventHandler? PremiumChanged;
}


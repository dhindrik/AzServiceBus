
namespace ServiceBusManager.Services;

public interface IFeatureService
{
    bool HasFeature(string featureName);
    void AddFeature(string featureName);
    void AddFeature(string featureName, DateTime validTo);
}


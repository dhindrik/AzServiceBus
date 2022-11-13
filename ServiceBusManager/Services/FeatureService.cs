using System;
namespace ServiceBusManager.Services
{
    public sealed class FeatureService : IFeatureService
    {
        private Dictionary<string, DateTime>? features;

        public FeatureService()
        {
        }

        public void AddFeature(string featureName)
        {
            if(features == null)
            {
                LoadFeatures();
            }

            if(features!.ContainsKey(featureName))
            {
                features[featureName] = DateTime.MaxValue;
                return;
            }

            features.Add(featureName, DateTime.MaxValue);
        }

        public void AddFeature(string featureName, DateTime validTo)
        {
            if (features == null)
            {
                LoadFeatures();
            }

            if (features!.ContainsKey(featureName))
            {
                features[featureName] = validTo;
                return;
            }

            features.Add(featureName, validTo);
        }

        public bool HasFeature(string featureName)
        {
            if (features == null)
            {
                LoadFeatures();
            }

            if(features!.ContainsKey(featureName))
            {
                var validTo = features[featureName];

                if(validTo >= DateTime.Now)
                {
                    return true;
                }
            }

            return false;
        }

        private void LoadFeatures()
        {
            var path = Path.Combine(FileSystem.Current.AppDataDirectory, "appfeatures.json");

            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);

                features = JsonSerializer.Deserialize<Dictionary<string, DateTime>>(json);

                return;
            }

            features = new();
        }

        private void SaveFeatures()
        {
            var path = Path.Combine(FileSystem.Current.AppDataDirectory, "appfeatures.json");

            var json = JsonSerializer.Serialize(features);

            File.WriteAllText(path, json);
        }
    }
}


using LaunchDarkly.Client;
using Microsoft.Extensions.Options;
using ServiceToggle.Demo.Configuration;

namespace ServiceToggle.Demo.Toggle
{
    public class LaunchDarklyClient
    {
        private readonly LdClient _client;

        public LaunchDarklyClient(LaunchDarklyOptions launchDarklyOptions)
        {
            _client = new LdClient(launchDarklyOptions.SdkKey);
        }

        public virtual bool IsFeatureEnabled(string featureName)
        {
            var user = new User("test");

            return _client.BoolVariation(featureName, user);
        }
    }
}
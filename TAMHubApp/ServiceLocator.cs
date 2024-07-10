using Microsoft.Extensions.DependencyInjection;

namespace TAMHubApp
{
    public static class ServiceLocator
    {
        public static ServiceProvider Instance { get; private set; }

        public static void Init(ServiceProvider serviceProvider)
        {
            Instance = serviceProvider;
        }
    }
}

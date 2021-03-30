using System;

namespace DD_WRT_API.Services
{
    public static class ServiceFactory
    {
        public static DdWrtServices Create(string username, string password, string deviceUrl)
        {
            if (string.IsNullOrWhiteSpace(username)) throw new ArgumentNullException(nameof(username));
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentNullException(nameof(password));
            if (string.IsNullOrWhiteSpace(deviceUrl)) throw new ArgumentNullException(nameof(deviceUrl));

            IRouterConnectionService pageContentReaderService = 
                new RouterConnectionService(username: username, password:password, deviceUrl);

            return new DdWrtServices(pageContentReaderService);
        }
    }
}

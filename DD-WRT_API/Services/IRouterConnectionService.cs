namespace DD_WRT_API.Services
{
    public interface IRouterConnectionService
    {
        string GetPageContent(string webPage);
        bool Reboot();
    }
}
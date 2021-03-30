using System;
using System.IO;
using System.Net;

namespace DD_WRT_API.Services
{
    public class RouterConnectionService : IRouterConnectionService
    {
        private readonly string _username;
        private readonly string _password;
        private readonly string _deviceAddress;

        public RouterConnectionService(string username, string password, string deviceAddress)
        {
            _username = username;
            _password = password;
            _deviceAddress = deviceAddress;
        }

        public string GetPageContent(string webPage)
        {
            string pageContent;
            HttpWebResponse httpWebResponse = HttpBasicAuthenticationBypassService.DoWebRequest(_username, _password, _deviceAddress,
                                                                          webPage, 80, "GET", "application/json");

            if (httpWebResponse.StatusCode == HttpStatusCode.OK)
            {
                pageContent = new StreamReader(httpWebResponse.GetResponseStream() ?? throw new InvalidOperationException()).ReadToEnd();
            }
            else
            {
                throw new ArgumentException($"Could not query the DD-WRT router, HttpStatus: {httpWebResponse.StatusCode}");
            }

            return pageContent;
        }


        public bool Reboot()
        {
            var endPoint = "applyuser.cgi";
            var body = "submit_button=Management&action=Reboot&change_action=&submit_type=&commit=1";

            HttpWebResponse httpWebResponse = HttpBasicAuthenticationBypassService.DoWebRequest(_username, _password, _deviceAddress,
                endPoint, 80, "POST", "application/x-www-form-urlencoded",
                accept: "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9",
                referrer: "Management.asp", //"http://192.168.0.1/Management.asp"
                postData:body
                );

            if (httpWebResponse.StatusCode == HttpStatusCode.OK)
            {
                return true;
            }

            throw new ArgumentException($"Could not query the DD-WRT router, HttpStatus: {httpWebResponse.StatusCode}");
        }
    }
}

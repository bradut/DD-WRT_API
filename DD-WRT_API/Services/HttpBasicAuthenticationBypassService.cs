using System;
using System.IO;
using System.Net;
using System.Text;
using log4net;
using log4net.Config;

namespace DD_WRT_API.Services
{
    public static class HttpBasicAuthenticationBypassService
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(HttpBasicAuthenticationBypassService).FullName);

        static HttpBasicAuthenticationBypassService()
        {
            XmlConfigurator.Configure();
        }

        // Bypass Http Basic Authentication
        // https://stackoverflow.com/questions/8228393/c-sharp-http-basic-authentication-bypass
        public static HttpWebResponse DoWebRequest(
            string username, string password, string url, string webpage, int port = 80,
            string requestMethod = "GET", string contentType = "image/jpeg",
            string accept = "image/png,image/jpeg,application/json,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
            string referrer = null,
            string postData = null
            )
        {
            string path = url + ":" + port + "/" + webpage;
            string userData = username + ":" + password;

            byte[] authBytes = Encoding.UTF8.GetBytes(userData.ToCharArray());
            string reqShortHostTemp = url;
            string reqShortHost = reqShortHostTemp.Replace("http://", "");

            var uri = new Uri(path);
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
  
            httpWebRequest.Method = requestMethod;
            httpWebRequest.ContentType = contentType;
            httpWebRequest.PreAuthenticate = false;
            httpWebRequest.Accept = accept;
            httpWebRequest.UserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.1; .NET CLR 1.0.3705;)";
            if (!string.IsNullOrWhiteSpace(referrer))
            {
                httpWebRequest.Referer = url + "/" + referrer;
            }

            httpWebRequest.Headers["Authorization"] = "Basic " + Convert.ToBase64String(authBytes);
            httpWebRequest.Headers.Add("Accept-Language: en-us,en;q=0.5");
            httpWebRequest.Headers.Add("Accept-Encoding: gzip,deflate");
            httpWebRequest.Headers.Add("Accept-Charset: ISO-8859-1,utf-8;q=0.7,*;q=0.7");
            httpWebRequest.Headers.Add("Keep-Alive: 1000");

            httpWebRequest.KeepAlive = true;
            httpWebRequest.ReadWriteTimeout = 320000;
            httpWebRequest.Timeout = 320000;
            httpWebRequest.Host = reqShortHost;
            httpWebRequest.AllowAutoRedirect = true;

            // add body content
            if (!string.IsNullOrWhiteSpace(postData))
            {
                var encoding = new ASCIIEncoding();
                byte[] byte1 = encoding.GetBytes(postData);

                // Set the content length of the string being posted.
                httpWebRequest.ContentLength = byte1.Length;

                Stream newStream = httpWebRequest.GetRequestStream();

                newStream.Write(byte1, 0, byte1.Length);
            }
            

            HttpWebResponse httpWebResponse;
            try
            {
                httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            }
            catch (Exception ex)
            {
                _logger.Error(ex);
                throw;
            }

            return httpWebResponse;
        }
    }
}

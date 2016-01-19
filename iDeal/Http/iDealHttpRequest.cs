using System.IO;
using System.Net;
using System.Text;
using iDeal.Base;
using iDeal.SignatureProviders;

namespace iDeal.Http
{
    public class iDealHttpRequest : IiDealHttpRequest
    {
        public iDealResponse SendRequest(iDealRequest idealRequest, ISignatureProvider signatureProvider, string url,
            IiDealHttpResponseHandler iDealHttpResponseHandler)
        {
            ServicePointManager.ServerCertificateValidationCallback =
                ((sender, certificate, chain, sslPolicyErrors) => true);

            // Create request
            var request = (HttpWebRequest) WebRequest.Create(url);
            request.ProtocolVersion = HttpVersion.Version11;
            request.ContentType = "text/xml";
            request.Method = "POST";
            // request.Proxy = new WebProxy("192.168.1.8", 8080);

            // Set content
            string xml = idealRequest.ToXml(signatureProvider);
            byte[] postBytes = Encoding.ASCII.GetBytes(xml);

            // Send
            using (Stream requestStream = request.GetRequestStream())
            {
                requestStream.Write(postBytes, 0, postBytes.Length);
            }

            // Return result
            using (WebResponse response = request.GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    return iDealHttpResponseHandler.HandleResponse(reader.ReadToEnd(), signatureProvider);
                }
            }
        }
    }
}
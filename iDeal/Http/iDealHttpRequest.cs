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
            request.ContentType = "text/xml charset=UTF-8";
            request.Method = "POST";

            // Set content
            string xml = idealRequest.ToXml(signatureProvider);
            byte[] postBytes = Encoding.UTF8.GetBytes(xml);

            request.ContentLength = postBytes.Length;
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
                    var responseXml = reader.ReadToEnd();
                    return iDealHttpResponseHandler.HandleResponse(responseXml, signatureProvider);
                }
            }
        }
    }
}
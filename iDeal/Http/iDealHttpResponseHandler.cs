using System.IO;
using System.Xml.Linq;
using iDeal.Base;
using iDeal.Directory;
using iDeal.SignatureProviders;
using iDeal.Status;
using iDeal.Transaction;

namespace iDeal.Http
{
    public class iDealHttpResponseHandler : IiDealHttpResponseHandler
    {
        public iDealResponse HandleResponse(string response, ISignatureProvider signatureProvider)
        {
            XElement xDocument = XElement.Parse(response);

            signatureProvider.VerifyResponseSignature(response);

            switch (xDocument.Name.LocalName)
            {
                case "DirectoryRes":
                    return new DirectoryResponse(response);

                case "AcquirerTrxRes":
                    return new TransactionResponse(response);

                case "AcquirerStatusRes":
                    var statusResponse = new StatusResponse(response);

                    return statusResponse;

                case "AcquirerErrorRes":
                    throw new iDealException(xDocument);

                default:
                    throw new InvalidDataException("Unknown response");
            }
        }
    }
}
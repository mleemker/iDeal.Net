using System.Xml.Linq;
using iDeal.Base;
using iDeal.SignatureProviders;

namespace iDeal.Directory
{
    public class DirectoryRequest : iDealRequest
    {
        public DirectoryRequest(string merchantId, int? subId)
        {
            MerchantId = merchantId;
            MerchantSubId = subId ?? 0; // If no sub id is specified, sub id should be 0
        }

        public override string MessageDigest
        {
            get
            {
                return CreateDateTimestamp + MerchantId + MerchantSubId;
            }
        }

        /// <summary>
        /// Creates xml representation of directory request
        /// </summary>
        public override string ToXml(ISignatureProvider signatureProvider)
        {
            var directoryRequestXmlMessage = new XDocument(
                new XDeclaration("1.0", "UTF-8", null),
                new XElement(Xml.Ns + "DirectoryReq",
                    new XAttribute("version", "3.3.1"),
                    new XElement(Xml.Ns + "createDateTimestamp", CreateDateTimestamp),
                    new XElement(Xml.Ns + "Merchant",
                        new XElement(Xml.Ns + "merchantID", MerchantId.PadLeft(9, '0')),
                        new XElement(Xml.Ns + "subID", "0"))));

            return signatureProvider.SignRequestXml(directoryRequestXmlMessage);
        }
    }
}
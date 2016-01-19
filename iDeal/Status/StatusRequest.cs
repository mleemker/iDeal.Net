using System;
using System.Xml.Linq;
using iDeal.Base;
using iDeal.SignatureProviders;

namespace iDeal.Status
{
    public class StatusRequest : iDealRequest
    {
        private string transactionId;

        /// <summary>
        /// Unique 16 digits number, assigned by the acquirer to the transaction
        /// </summary>
        public string TransactionId
        {
            get
            {
                return transactionId;
            }
            private set
            {
                if (string.IsNullOrWhiteSpace(value) || value.Length != 16)
                {
                    throw new InvalidOperationException("TransactionId must contain exactly 16 characters");
                }
                transactionId = value;
            }
        }

        public override string MessageDigest
        {
            get
            {
                return CreateDateTimestamp + MerchantId.PadLeft(9, '0') + MerchantSubId + TransactionId;
            }
        }

        public StatusRequest(string merchantId, int? subId, string transactionId)
        {
            MerchantId = merchantId;
            MerchantSubId = subId ?? 0; // If no sub id is specified, sub id should be 0
            TransactionId = transactionId;
        }

        public override string ToXml(ISignatureProvider signatureProvider)
        {
            var directoryRequestXmlMessage = new XDocument(
                new XDeclaration("1.0", "UTF-8", null),
                new XElement(Xml.Ns + "AcquirerStatusReq",
                    new XAttribute("version", "3.3.1"),
                    new XElement(Xml.Ns + "createDateTimestamp", CreateDateTimestamp),
                    new XElement(Xml.Ns + "Merchant",
                        new XElement(Xml.Ns + "merchantID", MerchantId.PadLeft(9, '0')),
                        new XElement(Xml.Ns + "subID", MerchantSubId)),
                    new XElement(Xml.Ns + "Transaction",
                        new XElement(Xml.Ns + "transactionID", TransactionId))));

            return signatureProvider.SignRequestXml(directoryRequestXmlMessage);
        }
    }
}
using System;
using System.Xml.Linq;

namespace iDeal.Base
{
    public class iDealException : Exception
    {
        public DateTime CreateDateTimestamp { get; private set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorDetail { get; set; }
        public string ConsumerMessage { get; set; }

        public iDealException()
        {
        }

        public iDealException(XElement xDocument)
        {
            CreateDateTimestamp = DateTime.Parse(xDocument.Element(Xml.Ns + "createDateTimestamp").Value);
            ErrorCode = xDocument.Element(Xml.Ns + "Error").Element(Xml.Ns + "errorCode").Value;
            ErrorMessage = xDocument.Element(Xml.Ns + "Error").Element(Xml.Ns + "errorMessage").Value;
            ErrorDetail = xDocument.Element(Xml.Ns + "Error").Element(Xml.Ns + "errorDetail").Value;
            ConsumerMessage = xDocument.Element(Xml.Ns + "Error").Element(Xml.Ns + "consumerMessage").Value;
        }
    }
}
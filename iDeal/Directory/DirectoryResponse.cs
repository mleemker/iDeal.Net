using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using iDeal.Base;

namespace iDeal.Directory
{
    public class DirectoryResponse : iDealResponse
    {
        private readonly IList<Issuer> issuers = new List<Issuer>();

        public string DirectoryDateTimeStamp { get; private set; }

        public DateTime DirectoryDateTimeStampLocalTime
        {
            get
            {
                return DateTime.Parse(DirectoryDateTimeStamp);
            }
        }

        public IList<Issuer> Issuers
        {
            get
            {
                return new ReadOnlyCollection<Issuer>(issuers);
            }
        }

        public DirectoryResponse(string xmlDirectoryResponse)
        {
            // Parse document
            XElement xDocument = XElement.Parse(xmlDirectoryResponse);

            // Create datetimestamp
            CreateDateTimestamp = xDocument.Element(Xml.Ns + "createDateTimestamp").Value;

            // Acquirer id
            AcquirerId = (int) xDocument.Element(Xml.Ns + "Acquirer").Element(Xml.Ns + "acquirerID");

            // Directory datetimestamp
            DirectoryDateTimeStamp =
                xDocument.Element(Xml.Ns + "Directory").Element(Xml.Ns + "directoryDateTimestamp").Value;

            // Get list of countries
            foreach (XElement country in xDocument.Element(Xml.Ns + "Directory").Elements(Xml.Ns + "Country"))
            {
                // Get list of issuers
                foreach (XElement issuer in country.Elements(Xml.Ns + "Issuer"))
                {
                    issuers.Add(new Issuer(issuer.Element(Xml.Ns + "issuerID").Value,
                        issuer.Element(Xml.Ns + "issuerName").Value));
                }
            }
        }
    }
}
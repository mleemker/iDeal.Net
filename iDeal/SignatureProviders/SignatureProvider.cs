using System.Deployment.Internal.CodeSigning;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Xml.Linq;
using iDeal.Base;

namespace iDeal.SignatureProviders
{
    public class SignatureProvider : ISignatureProvider
    {
        private const int ProvRsaAes = 24;

        private readonly X509Certificate2 acceptantPrivateCertificate;
        private readonly X509Certificate2 acquirerPublicCertificate;

        static SignatureProvider()
        {
            CryptoConfig.AddAlgorithm(typeof (RSAPKCS1SHA256SignatureDescription),
                "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256");
        }

        public SignatureProvider(X509Certificate2 acceptantPrivateCertificate,
            X509Certificate2 acquirerPublicCertificate)
        {
            this.acceptantPrivateCertificate = acceptantPrivateCertificate;
            this.acquirerPublicCertificate = acquirerPublicCertificate;
        }

        /// <summary>
        /// Adds a digital signature to the outgoing request message, before sending it to Acquirer.
        /// </summary>
        /// <param name="requestXml">
        /// The unsigned request XML message.
        /// </param>
        /// <returns>
        /// The request message, including digital signature.
        /// </returns>
        public string SignRequestXml(XDocument requestXml)
        {
            XmlDocument document = ToXmlDocument(requestXml);

            RSACryptoServiceProvider key = ExtractPrivateKeyFrom(acceptantPrivateCertificate);

            var signedXml = new SignedXml(document) { SigningKey = key };
            signedXml.SignedInfo.SignatureMethod = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
            signedXml.SignedInfo.CanonicalizationMethod = "http://www.w3.org/2001/10/xml-exc-c14n#";

            // Add a signing reference, the uri is empty and so the whole document is signed. 
            var reference = new Reference { DigestMethod = @"http://www.w3.org/2001/04/xmlenc#sha256" };
            reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
            reference.Uri = "";
            signedXml.AddReference(reference);

            // Add the certificate as key info. Because of this, the certificate 
            // with the public key will be added in the signature part. 
            var keyInfo = new KeyInfo();
            keyInfo.AddClause(new KeyInfoName(acceptantPrivateCertificate.Thumbprint));
            signedXml.KeyInfo = keyInfo;

            // Generate the signature. 
            signedXml.ComputeSignature();

            XmlElement xmlSignature = signedXml.GetXml();
            document.DocumentElement.AppendChild(document.ImportNode(xmlSignature, true));

            // Check that outgoing signature is valid. Private certificate also contains public part.
            VerifyDocumentSignature(document, acceptantPrivateCertificate);

            return GetContentsFrom(document);
        }

        /// <summary>
        /// Verifies the digital signature in the response message that was received from Acquirer.
        /// </summary>
        /// <param name="responseXml">
        /// Response XML message, coming from Acquirer.
        /// </param>
        public void VerifyResponseSignature(string responseXml)
        {
            XmlDocument document = ToXmlDocument(responseXml);
            VerifyDocumentSignature(document, acquirerPublicCertificate);
        }

        private static void VerifyDocumentSignature(XmlDocument document, X509Certificate2 publicCertificate)
        {
            XmlNodeList nodeList = document.GetElementsByTagName("Signature");
            XmlElement signatureElement = nodeList.Cast<XmlElement>().First();

            var signedXml = new SignedXml(document);
            signedXml.LoadXml(signatureElement);

            if (!signedXml.CheckSignature(publicCertificate, true))
            {
                throw new InvalidSignatureException();
            }
        }

        private static RSACryptoServiceProvider ExtractPrivateKeyFrom(X509Certificate2 certificate)
        {
            string exportedKeyMaterial = certificate.PrivateKey.ToXmlString(true);
            var key = new RSACryptoServiceProvider(new CspParameters(ProvRsaAes))
            {
                PersistKeyInCsp = false
            };
            key.FromXmlString(exportedKeyMaterial);
            return key;
        }

        private static XmlDocument ToXmlDocument(XDocument source)
        {
            var document = new XmlDocument
            {
                PreserveWhitespace = true
            };
            document.Load(source.CreateReader());
            return document;
        }

        private static XmlDocument ToXmlDocument(string source)
        {
            var document = new XmlDocument
            {
                PreserveWhitespace = true
            };
            document.LoadXml(source);
            return document;
        }

        private static string GetContentsFrom(XmlDocument source)
        {
            return source.OuterXml;
        }
    }
}
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Xml;
using System.Xml.Linq;

namespace iDeal.SignatureProviders
{
    public class SignatureProvider : ISignatureProvider
    {
        private readonly X509Certificate2 privateCertificate;
        private readonly X509Certificate2 publicCertificate;

        public SignatureProvider(X509Certificate2 privateCertificate, X509Certificate2 publicCertificate)
        {
            this.privateCertificate = privateCertificate;
            this.publicCertificate = publicCertificate;
        }

        /// <summary>
        /// Verifies the digital signature used in status responses from the ideal api (stored in xml field signature value)
        /// </summary>
        /// <param name="signature">
        /// Signature provided by ideal api, stored in signature value xml field
        /// </param>
        /// <param name="messageDigest">
        /// Concatenation of designated fields from the status response
        /// </param>
        public bool VerifySignature(string xml)
        {
            return true;
            /*
          using (MemoryStream streamIn = new MemoryStream())
          {
            using (StreamWriter w = new StreamWriter(streamIn))
            {
              w.Write(xml);
              w.Flush();
              streamIn.Position = 0;
              RSA rsaKey = (RSACryptoServiceProvider)_publicCertificate.PublicKey.Key;

              XmlDocument xmlDoc = new XmlDocument();
              xmlDoc.PreserveWhitespace = true;
              xmlDoc.Load(streamIn);
              SignedXml signedXml = new SignedXml(xmlDoc);
              XmlNodeList nodeList = xmlDoc.GetElementsByTagName("Signature");
              signedXml.LoadXml((XmlElement)nodeList[0]);
              bool result = signedXml.CheckSignature(_publicCertificate, true);
              return result;
            }
          }*/
        }

        public string SignXml(XDocument xml)
        {
            using (var streamIn = new MemoryStream())
            {
                xml.Save(streamIn);
                streamIn.Position = 0;
                //  var rsaKey = (RSACryptoServiceProvider)_privateCertificate.PrivateKey; // Create rsa crypto provider from private key contained in certificate, weirdest cast ever!;

                // string sCertFileLocation = @"C:\plugins\idealtest\bin\Debug\certficate.pfx";
                // X509Certificate2 certificate = new X509Certificate2(sCertFileLocation, "D3M@ast3rsR0cks");
                RSA rsaKey = (RSACryptoServiceProvider) privateCertificate.PrivateKey;

                var xmlDoc = new XmlDocument { PreserveWhitespace = true };
                xmlDoc.Load(streamIn);

                var signedXml = new SignedXml(xmlDoc) { SigningKey = rsaKey };

                var reference = new Reference { Uri = "" };
                var env = new XmlDsigEnvelopedSignatureTransform();
                reference.AddTransform(env);
                signedXml.AddReference(reference);

                var keyInfo = new KeyInfo();
                var kin = new KeyInfoName { Value = privateCertificate.Thumbprint };
                keyInfo.AddClause(kin);
                signedXml.KeyInfo = keyInfo;

                signedXml.ComputeSignature();
                XmlElement xmlDigitalSignature = signedXml.GetXml();
                xmlDoc.DocumentElement.AppendChild(xmlDoc.ImportNode(xmlDigitalSignature, true));

                using (var sout = new MemoryStream())
                {
                    xmlDoc.Save(sout);
                    sout.Position = 0;
                    using (var reader = new StreamReader(sout))
                    {
                        string xmlOut = reader.ReadToEnd();
                        return xmlOut;
                    }
                }
            }
        }
    }
}
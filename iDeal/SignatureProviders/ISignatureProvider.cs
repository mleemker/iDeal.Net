using System.Xml.Linq;

namespace iDeal.SignatureProviders
{
    public interface ISignatureProvider
    {
        /// <summary>
        /// Adds a digital signature to the outgoing request message, before sending it to Acquirer.
        /// </summary>
        /// <param name="requestXml">
        /// The unsigned request XML message.
        /// </param>
        /// <returns>
        /// The request message, including digital signature.
        /// </returns>
        string SignRequestXml(XDocument requestXml);

        /// <summary>
        /// Verifies the digital signature in the response message that was received from Acquirer.
        /// </summary>
        /// <param name="responseXml">
        /// Response XML message, coming from Acquirer.
        /// </param>
        void VerifyResponseSignature(string responseXml);
    }
}
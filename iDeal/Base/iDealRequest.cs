using System;
using iDeal.SignatureProviders;

namespace iDeal.Base
{
    public abstract class iDealRequest
    {
        private string merchantId;
        private int subId;

        /// <summary>
        /// Unique identifier of merchant
        /// </summary>
        public string MerchantId
        {
            get
            {
                return merchantId;
            }
            protected set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new InvalidOperationException("MerchantId does not contain a value");
                }
                if (value.Contains(" "))
                {
                    throw new InvalidOperationException("MerchantId cannot contain whitespaces");
                }
                if (value.Length > 9)
                {
                    throw new InvalidOperationException("MerchantId cannot contain more than 9 characters.");
                }
                merchantId = value;
            }
        }

        /// <summary>
        /// Sub id of merchant, usually 0
        /// </summary>
        public int MerchantSubId
        {
            get
            {
                return subId;
            }
            protected set
            {
                if (value < 0 || value > 6)
                {
                    throw new InvalidOperationException("SubId must contain a value ranging from 0 to 6");
                }
                subId = value;
            }
        }

        /// <summary>
        /// Create datetimestamp of request
        /// </summary>
        public string CreateDateTimestamp { get; private set; }

        public abstract string MessageDigest { get; }

        public abstract string ToXml(ISignatureProvider signatureProvider);

        protected iDealRequest()
        {
            CreateDateTimestamp = DateTime.Now.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'");
        }
    }
}
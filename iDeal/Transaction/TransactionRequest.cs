﻿using System;
using System.Globalization;
using System.Xml.Linq;
using iDeal.Base;
using iDeal.SignatureProviders;

namespace iDeal.Transaction
{
    public class TransactionRequest : iDealRequest
    {
        private string merchantReturnUrl;
        private string purchaseId;
        private TimeSpan? expirationPeriod;
        private string description;
        private string entranceCode;

        /// <summary>
        /// Unique identifier of issuer
        /// </summary>
        public string IssuerId { get; private set; }

        /// <summary>
        /// Url to which consumer is redirected after authorizing the payment
        /// </summary>
        public string MerchantReturnUrl
        {
            get
            {
                return merchantReturnUrl;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new InvalidOperationException("Merchant url is required");
                }
                merchantReturnUrl = value.Trim();
            }
        }

        /// <summary>
        /// Unique id determined by the acceptant, which will eventuelly show on the bank account
        /// </summary>
        public string PurchaseId
        {
            get
            {
                return purchaseId;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new InvalidOperationException("Purchase id is required");
                }
                if (value.Length > 16)
                {
                    throw new InvalidOperationException("Purchase id cannot contain more than 16 characters");
                }
                purchaseId = value;
            }
        }

        /// <summary>
        /// Amount measured in cents
        /// </summary>
        public decimal Amount { get; private set; }

        /// <summary>
        /// Time until consumer has to have paid, otherwise the transaction is marked as expired by the issuer (consumer's bank)
        /// </summary>
        public TimeSpan? ExpirationPeriod
        {
            get
            {
                return expirationPeriod;
            }
            set
            {
                if (value.HasValue)
                {
                    if (value.Value.TotalMinutes < 1)
                    {
                        throw new InvalidOperationException("Minimum expiration period is one minute");
                    }
                    if (value.Value.TotalMinutes > 60)
                    {
                        throw new InvalidOperationException("Maximum expiration period is 1 hour");
                    }
                }
                expirationPeriod = value;
            }
        }

        /// <summary>
        /// Description ordered product (no html tags!)
        /// </summary>
        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                if (value.Trim().Length > 32)
                {
                    throw new InvalidOperationException("Description cannot contain more than 32 characters");
                }
                description = value.Trim();
            }
        }

        /// <summary>
        /// Unique code generated by acceptant by which consumer can be identified
        /// </summary>
        public string EntranceCode
        {
            get
            {
                return entranceCode;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new InvalidOperationException("Entrance code is required");
                }
                if (value.Length > 40)
                {
                    throw new InvalidOperationException("Entrance code cannot contain more than 40 characters");
                }
                entranceCode = value;
            }
        }

        public TransactionRequest(string merchantId, int? subId, string issuerId, string merchantReturnUrl,
            string purchaseId, decimal amount, TimeSpan? expirationPeriod, string description, string entranceCode)
        {
            MerchantId = merchantId;
            MerchantSubId = subId ?? 0; // If no sub id is specified, sub id should be 0
            IssuerId = issuerId;
            MerchantReturnUrl = merchantReturnUrl;
            PurchaseId = purchaseId;
            Amount = amount;
            ExpirationPeriod = expirationPeriod ?? TimeSpan.FromMinutes(30); // Default 30 minutes expiration
            Description = description;
            EntranceCode = entranceCode;
        }

        public override string ToXml(ISignatureProvider signatureProvider)
        {
            var directoryRequestXmlMessage = new XDocument(
                new XDeclaration("1.0", "UTF-8", null),
                new XElement(Xml.Ns + "AcquirerTrxReq",
                    new XAttribute("version", "3.3.1"),
                    new XElement(Xml.Ns + "createDateTimestamp", CreateDateTimestamp),
                    new XElement(Xml.Ns + "Issuer",
                        new XElement(Xml.Ns + "issuerID", IssuerId.PadLeft(4, '0'))),
                    new XElement(Xml.Ns + "Merchant",
                        new XElement(Xml.Ns + "merchantID", MerchantId.PadLeft(9, '0')),
                        new XElement(Xml.Ns + "subID", "0"),
                        new XElement(Xml.Ns + "merchantReturnURL", MerchantReturnUrl)),
                    new XElement(Xml.Ns + "Transaction",
                        new XElement(Xml.Ns + "purchaseID", PurchaseId),
                        new XElement(Xml.Ns + "amount", Amount.ToString("0.##", CultureInfo.InvariantCulture)),
                        new XElement(Xml.Ns + "currency", "EUR"),
                        new XElement(Xml.Ns + "expirationPeriod",
                            "PT" + Convert.ToInt32(Math.Floor(ExpirationPeriod.Value.TotalSeconds)) + "S"),
                        new XElement(Xml.Ns + "language", "nl"),
                        new XElement(Xml.Ns + "description", Description),
                        new XElement(Xml.Ns + "entranceCode", EntranceCode))));

            return signatureProvider.SignRequestXml(directoryRequestXmlMessage);
        }
    }
}
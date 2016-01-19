using System;

namespace iDeal.Base
{
    public abstract class iDealResponse
    {
        public int AcquirerId { get; protected set; }

        public string CreateDateTimestamp { get; protected set; }

        public DateTime CreateDateTimestampLocalTime
        {
            get
            {
                return DateTime.Parse(CreateDateTimestamp);
            }
        }
    }
}
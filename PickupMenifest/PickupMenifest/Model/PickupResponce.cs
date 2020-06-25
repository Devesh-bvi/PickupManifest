using System;
using System.Collections.Generic;
using System.Text;

namespace PickupMenifest.Model
{
    public class PickupResponce
    {
        /// <summary>
        /// pickupStatus
        /// </summary>
        public string pickupStatus { get; set; }

        /// <summary>
        /// message
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// status_code
        /// </summary>
        public int status_code { get; set; }


        /// <summary>
        /// response
        /// </summary>
        public response response { get; set; }
    }
    public class response
    {
        /// <summary>
        /// pickupTokenNumber
        /// </summary>
        public string pickupTokenNumber { get; set; }

        /// <summary>
        /// pickupScheduledDate
        /// </summary>
        public string pickupScheduledDate { get; set; }

        /// <summary>
        /// status
        /// </summary>
        public int status { get; set; }

        /// <summary>
        /// pickupGeneratedDate
        /// </summary>
        public pickupGeneratedDate pickupGeneratedDate { get; set; }

        /// <summary>
        /// data
        /// </summary>
        public string data { get; set; }
    }
    public class pickupGeneratedDate
    {
        /// <summary>
        /// date
        /// </summary>
        public string date { get; set; }

        /// <summary>
        /// timezoneType
        /// </summary>
        public int timezoneType { get; set; }

        /// <summary>
        /// timezone
        /// </summary>
        public string timezone { get; set; }
    }
}

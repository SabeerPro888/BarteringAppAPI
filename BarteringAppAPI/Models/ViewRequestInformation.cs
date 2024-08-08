using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BarteringAppAPI.Models
{
    public class ViewRequestInformation
    {
        public String Receiver_name { get; set; }
        public String Sender_name { get; set; }

        public String RequestedItemName { get; set; }

        public int Offer_id { get; set; }

        public int Price { get; set; }

        public String RequestInformation { get; set; }

        public String status { get; set; }
    }
}
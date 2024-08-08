using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BarteringAppAPI.Models
{
    public class RequestsInformation
    {
       public String Receiver_name { get; set; }
        public String Sender_name { get; set; }

        public String RequestedItemName { get; set; }

        public int Offer_id { get; set; }

        public int Price { get; set; }

        public  List<int> OfferedItemsIds { get; set; }

        public String RequestInformation { get; set; }

        public String status { get; set; }

        public double? Rating { get; set; }


       public  String ConfirmOfferRequestReceiver { get; set; }
       public  String ConfirmOfferRequestSeder { get; set; }


        public int? senderId { get; set; }
        public int? ReceiverId { get; set; }

        public String SenderProfilePic { get; set; }
        public String ReceiverProfilePic { get; set; }



    }
}
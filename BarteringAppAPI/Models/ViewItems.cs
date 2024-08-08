using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BarteringAppAPI.Models
{
    public class ViewItems
    {
       /* "Item_name": "Core i7 Computer",
        "Price": 80000,
        "Description": "Core i7 3770 , 16GB Ram, 4GB RX 570 GPU, Custom RGB Casing, Liquid cooler.",
        "Barter_for": "Core i7 7th gen laptop",
        "Image_01": "~/Content/Images/3_1.jpg",
        "User_name": "sabirUllah",*/

        public String Item_name { get; set; }
        public  String Description { get; set; }
        public Nullable<int> Price { get; set; }
        public int Verification_id { get; set; }
        public String Barter_for { get; set; }
        public String Image_01 { get; set; }
        public String Image_02 { get; set; }
        public String AttributesJson { get; set; }

        public String Image_03 { get; set; }

        public String Image_04 { get; set; }

        public String Image_05 { get; set; }

        public String User_name { get; set; }
        public string Verification_status { get; set; }

        public int Item_id { get; set; }

        public String message { get; set; }

        public String status { get; set; }


        public double? Rating { get; set; }


        public String isSold { get; set; }

        public String ProfilePic { get; set; }

        public int User_id { get; set; }

        public String Category { get; set; }

        public string subCategory { get; set; }

        public List<String> BarterItems { get; set; }

        public String date { get; set; }
    }
}
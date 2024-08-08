using BarteringAppAPI.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Http;

namespace BarteringAppAPI.Controllers
{
    public class IndexController : ApiController
    {
        BarteringAppEntities db = new BarteringAppEntities();

        [HttpGet]
        public HttpResponseMessage Login(String Email, String Password)
        {

            String email = Email;
            String pass = Password;
            try
            {
                var user = db.Users.Where(u => u.Email == email && u.Password == pass).FirstOrDefault();
                if (user == null)
                {

                    var admin = db.Admins.Where(a => a.User_name == email && a.Password == pass).FirstOrDefault();
                    if (admin != null)
                    {
                        return Request.CreateResponse("Admin");

                    }
                    else if (admin == null)
                    {
                        var trustedParty = db.Trusted_party.Where(a => a.User_name == email && a.Password == pass).FirstOrDefault();
                        if (trustedParty != null)
                        {
                            return Request.CreateResponse("Trusted");

                        }
                        else
                        {
                            return Request.CreateResponse("Invalid Email and Password");
                        }

                    }
                    else
                    {
                        return Request.CreateResponse("Invalid Email and Password");
                    }
                }
                else
                {
                    return Request.CreateResponse("User");

                }
            }
            catch (Exception e)
            {
                return Request.CreateResponse(e.Message);
            }
        }



        [HttpPost]
        public String SignUP(string User_name, string Password, string Contact, string Email, string Location, string Gender)
        {

            var user = db.Users.Where(x => x.Email == Email).FirstOrDefault();
            if (user == null)
            {
                User u = new User
                {
                    User_name = User_name,
                    Password = Password,
                    Email = Email,
                    Location = Location,
                    Contact = Contact,
                    Gender = Gender,
                    Rating = 0
                };
                db.Users.Add(u);
                db.SaveChanges();
                return "Signed up Successfully";
            }
            return "Email Already Exists";
        }





        [HttpPost]
        public int UploadItem()
        {
            var request = HttpContext.Current.Request;

            string email = request["Email"];
            var userid = db.Users.Where(x => x.Email == email).FirstOrDefault();

            string name = request["Item_name"];
            string catagory = request["Category"];
            string description = request["Description"];
            string barter_for = request["Barter_for"];
            int price = int.Parse(request["Price"]);


            Item itm = new Item();
            itm.Item_name = name;
            itm.Price = price;
            itm.Verification_status = "Not Verified";
            itm.Category = catagory;
            itm.Description = description;
            itm.Barter_for = barter_for;
            itm.User_id = userid.User_id;
            db.Items.Add(itm);
            db.SaveChanges();

            Item_images img = new Item_images();
            img.Item_id = itm.Item_id;
            for (int i = 1; i < 6; i++)
            {

                var photo = request.Files["Image_0" + i];
                if (photo != null)
                {
                    string photoName = (img.Item_id + "_" + i) + "." + photo.FileName.Split('.')[1];
                    photo.SaveAs(HttpContext.Current.Server.MapPath("~/Content/Images/" + photoName));
                    if (i == 1)
                    {
                        img.Image_01 = photoName;
                    }
                    else if (i == 2)
                    {
                        img.Image_02 = photoName;
                    }
                    else if (i == 3)
                    {
                        img.Image_03 = photoName;
                    }
                    else if (i == 4)
                    {
                        img.Image_04 = photoName;
                    }
                    else if (i == 5)
                    {
                        img.Image_05 = photoName;
                    }
                }
            }
            db.Item_images.Add(img);
            db.SaveChanges();
            var lastItemId = db.Items
    .Where(x => x.User_id == userid.User_id)
    .OrderByDescending(x => x.Item_id)
    .Select(x => x.Item_id)
    .FirstOrDefault();


            return lastItemId;
        }

        [HttpGet]
        public List<ViewItems> ShowAllItems(String email)
        {
            List<ViewItems> itemslist = new List<ViewItems>();

            String userId = getUserId(email);
            int id = int.Parse(userId);
            var data = from i in db.Items
                       join img in db.Item_images
                       on i.Item_id equals img.Item_id
                       where i.isSold == "No"
                       select new
                       {
                           i.User_id,
                           i.Item_id,
                           i.Item_name,
                           i.Price,
                           i.Description,
                           i.Verification_status,
                           i.Barter_for,
                           img.Image_01,
                           img.Image_02,
                           img.Image_03,
                           img.Image_04,
                           img.Image_05,
                           i.Category,
                           i.SubCategory,

                       };


            var finalData = from i in data
                            join u in db.Users
                            on i.User_id equals u.User_id
                            where u.User_id != id
                            select new ViewItems
                            {
                                Item_id = i.Item_id,
                                Item_name = i.Item_name,
                                Price = (int)i.Price,
                                Description = i.Description,
                                Barter_for = i.Barter_for,
                                Image_01 = i.Image_01,
                                Image_02 = i.Image_02,
                                Image_03 = i.Image_03,
                                Image_04 = i.Image_04,
                                Image_05 = i.Image_05,
                                Verification_status = i.Verification_status,
                                User_name = u.User_name,
                                Rating = u.Rating,
                                ProfilePic = u.Profile_Pic,
                                User_id = u.User_id,
                                Category = i.Category,
                                subCategory = i.SubCategory



                            };

            itemslist.AddRange(finalData);







            return itemslist;
        }



        [HttpGet]
        public List<ViewItems> ShowAllItems1(String email)
        {
            List<ViewItems> itemslist = new List<ViewItems>();

            String userId = getUserId(email);
            int id = int.Parse(userId);

            // Step 1: Get the items and images
            var data = from i in db.Items
                       join img in db.Item_images on i.Item_id equals img.Item_id
                       where i.isSold == "No"
                       select new
                       {
                           i.User_id,
                           i.Item_id,
                           i.Item_name,
                           i.Price,
                           i.Description,
                           i.Verification_status,
                           i.Barter_for,
                           img.Image_01,
                           img.Image_02,
                           img.Image_03,
                           img.Image_04,
                           img.Image_05,
                           i.Category,
                           i.SubCategory
                       };

            // Step 2: Join with Users table
            var itemWithUserData = from i in data
                                   join u in db.Users on i.User_id equals u.User_id
                                   where u.User_id != id
                                   select new
                                   {
                                       i.Item_id,
                                       i.Item_name,
                                       i.Price,
                                       i.Description,
                                       i.Barter_for,
                                       i.Image_01,
                                       i.Image_02,
                                       i.Image_03,
                                       i.Image_04,
                                       i.Image_05,
                                       i.Verification_status,
                                       u.User_name,
                                       u.Rating,
                                       u.Profile_Pic,
                                       u.User_id,
                                       i.Category,
                                       i.SubCategory
                                   };

            // Step 3: Join with BarterFor table to get barter information
            var finalData = from i in itemWithUserData
                            join b in db.WishList on i.Item_id equals b.Item_id into barterGroup
                            from bg in barterGroup.DefaultIfEmpty()
                            group bg by new
                            {
                                i.Item_id,
                                i.Item_name,
                                i.Price,
                                i.Description,
                                i.Barter_for,
                                i.Image_01,
                                i.Image_02,
                                i.Image_03,
                                i.Image_04,
                                i.Image_05,
                                i.Verification_status,
                                i.User_name,
                                i.Rating,
                                i.Profile_Pic,
                                i.User_id,
                                i.Category,
                                i.SubCategory
                            } into g
                            select new ViewItems
                            {
                                Item_id = g.Key.Item_id,
                                Item_name = g.Key.Item_name,
                                Price = (int)g.Key.Price,
                                Description = g.Key.Description,
                                Barter_for = g.Key.Barter_for,
                                Image_01 = g.Key.Image_01,
                                Image_02 = g.Key.Image_02,
                                Image_03 = g.Key.Image_03,
                                Image_04 = g.Key.Image_04,
                                Image_05 = g.Key.Image_05,
                                Verification_status = g.Key.Verification_status,
                                User_name = g.Key.User_name,
                                Rating = g.Key.Rating,
                                ProfilePic = g.Key.Profile_Pic,
                                User_id = g.Key.User_id,
                                Category = g.Key.Category,
                                subCategory = g.Key.SubCategory,
                                BarterItems = g.Select(x => x.BarterFor).ToList()
                            };

            itemslist.AddRange(finalData);

            return itemslist;
        }
        /* [HttpGet]
         public HttpResponseMessage GetItemDetails(int itemId)
         {
             try
             {
                 // Fetch the item details from the database based on the provided itemId
                 var data = (from i in db.Items
                             where i.Item_id == itemId
                             join img in db.Item_images on i.Item_id equals img.Item_id
                             join u in db.Users on i.User_id equals u.User_id
                             select new
                             {
                                 i.Item_id,
                                 i.Item_name,
                                 i.Price,
                                 i.Description,
                                 i.Verification_status,
                                 i.Barter_for,
                                 img.Image_01,
                                 img.Image_02,
                                 img.Image_03,
                                 img.Image_04,
                                 img.Image_05,
                                 User_name = u.User_name
                             }).FirstOrDefault();

                 if (data != null)
                 {
                     // Create a ViewItems object to hold the item details
                     ViewItems item = new ViewItems
                     {
                         Item_id = data.Item_id,
                         Item_name = data.Item_name,
                         Description = data.Description,
                         Barter_for = data.Barter_for,
                         Image_01 = data.Image_01,
                         Image_02 = data.Image_02,
                         Image_03 = data.Image_03,
                         Image_04 = data.Image_04,
                         Image_05 = data.Image_05,
                         Verification_status = data.Verification_status,
                         User_name = data.User_name
                     };

                     // Return the item details as a response
                     return Request.CreateResponse(HttpStatusCode.OK, item);
                 }
                 else
                 {
                     // Return a message indicating that the item with the given ID was not found
                     return Request.CreateResponse(HttpStatusCode.NotFound, "Item not found");
                 }
             }
             catch (Exception e)
             {
                 // Return an error message if an exception occurs
                 return Request.CreateResponse(HttpStatusCode.InternalServerError, e.Message);
             }
         }*/

        [HttpGet]
        public ViewItems GetItemDetails(string id)
        {
            int itemId = int.Parse(id);

            // Fetch the item details from the database based on the provided itemId
            var itemDetails = (from i in db.Items
                               where i.Item_id == itemId
                               join img in db.Item_images on i.Item_id equals img.Item_id
                               join u in db.Users on i.User_id equals u.User_id
                               select new
                               {
                                   i.Item_id,
                                   i.Item_name,
                                   i.Price,
                                   i.Description,
                                   i.Verification_status,
                                   i.Barter_for,
                                   img.Image_01,
                                   img.Image_02,
                                   img.Image_03,
                                   img.Image_04,
                                   img.Image_05,
                                   User_name = u.User_name,
                                   u.Rating,
                                   u.Profile_Pic,
                                   u.User_id,
                                   i.Category,
                                   i.SubCategory
                               }).FirstOrDefault();

            if (itemDetails != null)
            {
                // Fetch attributes associated with the item
                var attributes = db.uploadedItemsAttributes
                    .Where(attr => attr.Item_id == itemId)
                    .ToDictionary(attr => attr.attribute_name, attr => attr.attribute_value);

                // Serialize attributes to JSON string
                string attributesJson = JsonConvert.SerializeObject(attributes);

                // Fetch barter items associated with the item
                var barterItems = db.WishList
                    .Where(b => b.Item_id == itemId)
                    .Select(b => b.BarterFor) // Adjust as per your database structure
                    .ToList();

                // Create a ViewItems object to hold the item details along with attributes and barterItems
                ViewItems item = new ViewItems
                {
                    Item_id = itemDetails.Item_id,
                    Item_name = itemDetails.Item_name,
                    Description = itemDetails.Description,
                    Barter_for = itemDetails.Barter_for,
                    Image_01 = itemDetails.Image_01,
                    Image_02 = itemDetails.Image_02,
                    Image_03 = itemDetails.Image_03,
                    Image_04 = itemDetails.Image_04,
                    Image_05 = itemDetails.Image_05,
                    Verification_status = itemDetails.Verification_status,
                    User_name = itemDetails.User_name,
                    Price = itemDetails.Price,
                    AttributesJson = attributesJson,
                    Rating = itemDetails.Rating,
                    ProfilePic = itemDetails.Profile_Pic,
                    User_id = itemDetails.User_id,
                    Category = itemDetails.Category,
                    subCategory = itemDetails.SubCategory,
                    BarterItems = barterItems // Assign the list of barterItems to the item
                };

                // Return the item details along with attributes and barterItems as a response
                return item;
            }
            else
            {
                // Return a message indicating that the item with the given ID was not found
                // Adjust this part based on your error handling or return type requirement
                return null; // or return an appropriate response
            }
        }



        [HttpGet]
        public HttpResponseMessage SearchByCategory()
        {
            var request = HttpContext.Current.Request;
            string category = request["Category"];
            var data = from i in db.Items
                       join img in db.Item_images
                       on i.Item_id equals img.Item_id
                       select new
                       {
                           i.User_id,
                           i.Item_id,
                           i.Item_name,
                           i.Price,
                           i.Description,
                           i.Category,
                           i.Barter_for,
                           img.Image_01
                       };
            var finalData = from i in data
                            join u in db.Users
                            on i.User_id equals u.User_id
                            select new
                            {
                                i.User_id,
                                i.Item_id,
                                i.Item_name,
                                i.Price,
                                i.Description,
                                i.Category,
                                i.Barter_for,
                                i.Image_01,
                                u.User_name,
                                u.Rating,

                            };

            var filterData = finalData.Where(x => x.Category == category).ToList();
            if (filterData.Count != 0)
            {
                return Request.CreateResponse(filterData);
            }

            return Request.CreateResponse(finalData);

        }


        [HttpPost]

        /*        public HttpResponseMessage SendOffer()
                {
                    try
                    {
                        var request = HttpContext.Current.Request;
                        int receiver_id = int.Parse(request["Receiver_id"]);
                        int sender_id = int.Parse(request["Sender_id"]);
                        int requested_item = int.Parse(request["Requested_item"]);
                        int no_of_item = int.Parse(request["No_of_item"]);
                        float price = float.Parse(request["Price"]);

                        Offer o = new Offer();
                        o.Requested_item = requested_item;
                        o.Receiver_id = receiver_id;
                        o.Sender_id = sender_id;
                        o.Price = price;
                        db.Offers.Add(o);
                        db.SaveChanges();
                        Request r = new Request();
                        r.Offer_id = o.Offer_id;
                        r.Date = "" + DateTime.Now + "";
                        r.Req_status = "Pending";
                        db.Requests.Add(r);
                        db.SaveChanges();
                        for (int i = 0; i < no_of_item; i++)
                        {

                            int offer_items = int.Parse(request["item" + i]);
                            Offered_items of = new Offered_items();
                            of.Item_id = offer_items;
                            of.Offer_id = o.Offer_id;
                            db.Offered_items.Add(of);
                            db.SaveChanges();
                        }

                        return Request.CreateResponse("Offer Sent Successfully");
                    }
                    catch (Exception e)
                    {
                        return Request.CreateResponse(e.Message);
                    }
                }
        */

        [HttpGet]
        public HttpResponseMessage ViewOffer(String email)
        {
            String Stringid = getUserId(email);
            try
            {
                //user_id,username,requested for rating,offer id
                int id = int.Parse(Stringid);
                var data = from r in db.Offers
                           join d in db.Requests
                           on r.Offer_id equals d.Offer_id
                           where r.Receiver_id == id
                           select new
                           {
                               r.Offer_id,
                               r.Receiver_id,
                               r.Sender_id,
                               r.Requested_item,
                               d.Date,
                               d.Req_status,
                               d.Req_id,
                               r.Price,
                           };
                var data2 = from d in data
                            join u in db.Users
                            on d.Sender_id equals u.User_id
                            select new
                            {
                                d.Offer_id,
                                d.Receiver_id,
                                d.Sender_id,
                                d.Requested_item,
                                d.Date,
                                d.Req_status,
                                d.Req_id,
                                d.Price,
                                u.User_name,
                                u.Rating,

                            };
                var finaldata = from d in data2
                                join i in db.Items
                                on d.Requested_item equals i.Item_id
                                select new
                                {
                                    d.User_name,
                                    d.Rating,
                                    i.Item_name,
                                    d.Date,
                                    d.Price,
                                    d.Offer_id,
                                    d.Receiver_id,
                                    d.Sender_id,
                                    d.Requested_item,
                                    d.Req_status,
                                    d.Req_id,



                                };

                return Request.CreateResponse(finaldata);
            }
            catch (Exception e)
            {
                return Request.CreateResponse(e.Message);
            }
        }

        [HttpGet]
        public HttpResponseMessage OfferDetails()
        {
            var request = HttpContext.Current.Request;
            int id = int.Parse(request["Offer_id"]);
            var data = from oi in db.Offered_items
                       join i in db.Items
                       on oi.Item_id equals i.Item_id
                       where oi.Offer_id == id
                       select new
                       {
                           i.Item_name,
                           i.Price
                       };
            return Request.CreateResponse(data);
        }


        [HttpPost]
        public void AcceptOrRejectOffer(int Numstatus, int Offerid)
        {
            try
            {
                var RequestId = db.Requests.Where(x => x.Offer_id == Offerid).FirstOrDefault();
                var Offer = db.Offers.Where(x => x.Offer_id == Offerid).FirstOrDefault();

                String status;
                int id = RequestId.Req_id;
                if (Numstatus == 1)
                {
                    status = "Accepted";
                    var offeredItems = db.Offered_items.Where(x => x.Offer_id == Offerid).ToList();
                    foreach (var s in offeredItems)
                    {
                        var item = db.Items.Where(x => x.Item_id == s.Item_id).FirstOrDefault();
                        item.isSold = "Yes";
                    }
                    var RequestedItem = db.Items.Where(x => x.Item_id == Offer.Requested_item).FirstOrDefault();
                    RequestedItem.isSold = "Yes";

                }
                else
                {
                    status = "Rejected";
                }
                var updateTable = db.Requests.Where(s => s.Req_id == id).FirstOrDefault();
                updateTable.Req_status = status;



                db.SaveChanges();
            }
            catch (Exception e)
            {

            }
        }

        [HttpGet]
        public List<ViewItems> MyItems(string Email)
        {
            try
            {
                // Get the user's ID based on the provided email
                string userIdString = getUserId(Email);
                int userId = int.Parse(userIdString);

                // Fetch items owned by the user
                var itemsData = (from item in db.Items
                                 where item.User_id == userId
                                 select new
                                 {
                                     item.Item_id,
                                     item.Item_name,
                                     item.Price,
                                     item.Description,
                                     item.Verification_status,
                                     item.Barter_for,
                                     item.isSold,
                                     item.Category,
                                     item.SubCategory,
                                     item.User_id
                                 }).ToList();

                // Fetch item images for each item
                var itemImages = db.Item_images.ToList(); // Adjust if filtering is needed per item later

                // Fetch barter items associated with each item
                var barterItemsData = (from item in db.Items
                                       join bi in db.WishList on item.Item_id equals bi.Item_id into gj
                                       from subbi in gj.DefaultIfEmpty()
                                       where item.User_id == userId
                                       select new
                                       {
                                           ItemId = item.Item_id,
                                           BarterItem = subbi.BarterFor // Adjust based on your database structure
                                       }).ToList();

                // Create ViewItems objects combining item details, images, and barter items
                var finalData = itemsData.Select(item => new ViewItems
                {
                    User_id = (int)item.User_id,
                    Item_id = item.Item_id,
                    Item_name = item.Item_name,
                    Price = (int)item.Price,
                    Description = item.Description,
                    Verification_status = item.Verification_status,
                    Barter_for = item.Barter_for,
                    isSold = item.isSold,
                    Category = item.Category,
                    subCategory = item.SubCategory,
                    Image_01 = itemImages.FirstOrDefault(img => img.Item_id == item.Item_id)?.Image_01,
                    Image_02 = itemImages.FirstOrDefault(img => img.Item_id == item.Item_id)?.Image_02,
                    Image_03 = itemImages.FirstOrDefault(img => img.Item_id == item.Item_id)?.Image_03,
                    Image_04 = itemImages.FirstOrDefault(img => img.Item_id == item.Item_id)?.Image_04,
                    Image_05 = itemImages.FirstOrDefault(img => img.Item_id == item.Item_id)?.Image_05,
                    BarterItems = barterItemsData.Where(bi => bi.ItemId == item.Item_id)
                                                 .Select(bi => bi.BarterItem)
                                                 .ToList()
                }).ToList();

                return finalData;
            }
            catch (Exception e)
            {
                // Handle exceptions appropriately, log, or rethrow
                throw e;
            }
        }



        [HttpPost]
        public HttpResponseMessage SendVerificationRequest(String Item_id)
        {
            try
            {
                var request = HttpContext.Current.Request;
                int id = int.Parse(Item_id);
                Verification_requests v = new Verification_requests();
                v.Item_id = id;
                v.Trusted_party_id = 1;
                db.Verification_requests.Add(v);
                db.SaveChanges();
                return Request.CreateResponse("Request Sent Successfully");
            }
            catch (Exception e)
            {
                return Request.CreateResponse(e.Message);
            }
        }
        [HttpPost]
        public HttpResponseMessage VerifyRequest(int id, String status, String message)
        {
            try
            {
                var request = HttpContext.Current.Request;
                /* int id = int.Parse(request["Verification_id"]);
                 string status = request["Status"];*/
                var data = db.Verification_requests.Where(x => x.Verification_id == id).Select(s => new { s.Item_id }).FirstOrDefault();
                if (data != null)
                {
                    int item_id = int.Parse(data.Item_id.ToString());
                    var itemData = db.Items.Where(x => x.Item_id == item_id).FirstOrDefault();
                    if (data != null)
                    {
                        itemData.Verification_status = status;
                        db.SaveChanges();
                        Notification n = new Notification();
                        n.Item_id = itemData.Item_id;
                        n.message = message;
                        n.Status = "Unread";
                        n.User_id = itemData.User_id;

                        db.Notification.Add(n);
                        db.SaveChanges();
                        deleteVerificationRequest(id);

                        return Request.CreateResponse(itemData.Item_id + "  " + itemData.Item_name + "  Request Has been" + status);
                    }
                }

                return Request.CreateResponse("Request Sent Successfully");
            }
            catch (Exception e)
            {
                return Request.CreateResponse(e.Message);
            }
        }

        public void deleteVerificationRequest(int verificationid)
        {
            var data = db.Verification_requests.Where(x => x.Verification_id == verificationid).FirstOrDefault();
            if (data != null)
            {
                db.Verification_requests.Remove(data);
                db.SaveChanges();
            }
            else
            {
                Console.WriteLine("Verification request not found.");
            }
        }
        /*  [HttpGet]
          public List<ViewItems> ViewVerificationRequests()
          {
              try
              {
                  var request = HttpContext.Current.Request;
                  var data = from v in db.Verification_requests
                             join i in db.Items
                             on v.Item_id equals i.Item_id
                             select new
                             {
                                 i.Item_id,
                                 i.Item_name,
                                 i.Price,
                                 i.Description,
                                 i.Category,
                                 i.User_id,
                                 v.Verification_id,
                                 i.Verification_status,


                             };


                  var finaldata = from d in data
                                  join img in db.Item_images
                                  on d.Item_id equals img.Item_id
                                  join u in db.Users
                                  on d.User_id equals u.User_id
                                  select new ViewItems
                                  {
                                      Item_id = d.Item_id,
                                      Item_name = d.Item_name,
                                      Description = d.Description,
                                      Image_01 = img.Image_01,
                                      User_name = u.User_name,
                                      Verification_status = d.Verification_status,
                                      Image_02 = img.Image_02,
                                      Image_03 = img.Image_03,
                                      Image_04 = img.Image_04,
                                      Image_05 = img.Image_05,
                                      Price = (int)d.Price,
                                      Verification_id = d.Verification_id,
                                      ProfilePic = u.Profile_Pic,
                                      Rating = u.Rating

                                  };
                  return finaldata.ToList();
              }

              catch (Exception e)
              {
                  // Handle exceptions appropriately
                  // You might want to log the exception details
                  Console.WriteLine(e);
                  return null; // or return an empty list or throw an HttpResponseException
              }
          }

       
  */

        [HttpGet]
        public List<Category> getCategories()
        {
            return db.Categories.ToList();
        }
        [HttpGet]
        public List<ViewItems> ViewVerificationRequests()
        {
            try
            {
                var request = HttpContext.Current.Request;

                // Get the main data with joins
                var data = from v in db.Verification_requests
                           join i in db.Items on v.Item_id equals i.Item_id
                           join img in db.Item_images on i.Item_id equals img.Item_id
                           join u in db.Users on i.User_id equals u.User_id
                           select new
                           {
                               i.Item_id,
                               i.Item_name,
                               i.Description,
                               i.Category,
                               i.User_id,
                               v.Verification_id,
                               i.Verification_status,
                               i.Price,
                               img.Image_01,
                               img.Image_02,
                               img.Image_03,
                               img.Image_04,
                               img.Image_05,
                               u.User_name,
                               u.Profile_Pic,
                               u.Rating
                           };

                // Group by Item_id to get the BarterFor items
                var result = from d in data
                             join b in db.WishList on d.Item_id equals b.Item_id into bf
                             select new
                             {
                                 d.Item_id,
                                 d.Item_name,
                                 d.Description,
                                 d.Category,
                                 d.User_id,
                                 d.Verification_id,
                                 d.Verification_status,
                                 d.Price,
                                 d.Image_01,
                                 d.Image_02,
                                 d.Image_03,
                                 d.Image_04,
                                 d.Image_05,
                                 d.User_name,
                                 d.Profile_Pic,
                                 d.Rating,
                                 BarterForItems = bf.Select(b => b.BarterFor).ToList() // List of BarterFor items
                             };

                // Create the final list of ViewItems
                var finalData = result.ToList().Select(f => new ViewItems
                {
                    Item_id = f.Item_id,
                    Item_name = f.Item_name,
                    Description = f.Description,
                    Category = f.Category,
                    User_id = (int)f.User_id,
                    Verification_id = f.Verification_id,
                    Verification_status = f.Verification_status,
                    Price = (int)f.Price,
                    Image_01 = f.Image_01,
                    Image_02 = f.Image_02,
                    Image_03 = f.Image_03,
                    Image_04 = f.Image_04,
                    Image_05 = f.Image_05,
                    User_name = f.User_name,
                    ProfilePic = f.Profile_Pic,
                    Rating = f.Rating,
                    BarterItems = f.BarterForItems // Assign the list of BarterFor items
                }).ToList();

                return finalData;
            }
            catch (Exception e)
            {
                // Handle exceptions appropriately
                // You might want to log the exception details
                Console.WriteLine(e);
                return null; // or return an empty list or throw an HttpResponseException
            }
        }



        [HttpGet]
        public List<subc> getSubCategoriess(string category)
        {


            // Perform a join between Categories and SubCategories tables based on Category_id
            var subCategories = (from sub in db.Subcategories
                                 join cat in db.Categories on sub.category_id equals cat.Category_id
                                 where cat.Category_name == category
                                 select new subc
                                 {
                                     subcategory_id = sub.subcategory_id,
                                     subcategory_name = sub.subcategory_name,
                                     category_id = cat.Category_id
                                 }).ToList();

            return subCategories;
        }



        [HttpGet]
        public String getUserId(String email)
        {
            var user = db.Users.Where(x => x.Email == email).FirstOrDefault();

            return user.User_id.ToString();
        }



        [HttpGet]
        public List<String> getSubcategorybrands(String subcategoryname)
        {
            int subid = getsubcategoryid(subcategoryname);
            List<String> brandsname = new List<string>();
            var brands = db.Brands.Where(s => s.subcategory_id == subid).ToList();
            if (brands != null && brands.Count > 0)
            {
                foreach (Brands b in brands)
                {
                    brandsname.Add(b.brand_name);


                }
                return brandsname;
            }
            else
            {
                List<String> attributes = getsubcategoryAttributes(subcategoryname);

                return attributes;
            }


        }






        [HttpGet]
        public List<String> getmodels(String brandname)
        {
            List<String> models = new List<string>();
            var brandid = db.Brands.Where(s => s.brand_name == brandname).FirstOrDefault();

            var model = db.Models.Where(x => x.brand_id == brandid.brand_id).ToList();

            foreach (Models.Models m in model)
            {
                models.Add(m.model_name);
            }
            return models;
        }

        [HttpGet]
        public List<String> getsubcategoryAttributes(String subcategory)
        {

            List<String> subcatattributes = new List<string>();

            int subid = getsubcategoryid(subcategory);


            var attributes = db.Subcategory_Attributes.Where(s => s.subcategory_id == subid).ToList();

            foreach (Subcategory_Attributes sb in attributes)
            {
                subcatattributes.Add(sb.attribute_name);
            }

            return subcatattributes;
        }

        [HttpGet]
        public List<String> getattributevalues(String subcategory, String attributename)
        {
            List<String> attributevalues = new List<String>();
            int subcat = getsubcategoryid(subcategory);
            var attributes = db.Subcategory_Attributes.Where(s => s.subcategory_id == subcat && s.attribute_name == attributename).FirstOrDefault();

            var values = db.Attribute_Values.Where(x => x.attribute_id == attributes.attribute_id).ToList();

            foreach (Attribute_Values s in values)
            {
                attributevalues.Add(s.value);
            }

            return attributevalues;
        }

        [HttpPost]
        public int UploadItem2()
        {
            var request = HttpContext.Current.Request;

            // Extract request parameters from form data
            string email = request.Form["Email"];
            var userid = db.Users.FirstOrDefault(x => x.Email == email);
            string name = request.Form["Item_name"];
            string subcategory = request.Form["subCategory"];
            string category = request.Form["Category"];
            string description = request.Form["Description"];
            /*            string barterFor = request.Form["Barter_for"];
            */
            string attributesJson = request.Form["Attributes"];
            int price = int.Parse(request.Form["Price"]);
            string model = request.Form["Model"];
            string brand = request.Form["Brand"];

            // Deserialize attributes from JSON
            Dictionary<string, string> attributes = JsonConvert.DeserializeObject<Dictionary<string, string>>(attributesJson);

            // Create and save item
            Item itm = new Item
            {
                Item_name = name,
                Price = price,
                Verification_status = "Not Verified",
                SubCategory = subcategory,
                Description = description,
                User_id = userid.User_id,
                Category = category,
                isSold = "No",
                Date = DateTime.Today
            };

            db.Items.Add(itm);
            db.SaveChanges();

            // Save item images
            Item_images img = new Item_images { Item_id = itm.Item_id };
            for (int i = 1; i < 6; i++)
            {
                var photo = request.Files["Image_0" + i];
                if (photo != null)
                {
                    string photoName = (img.Item_id + "_" + i) + "." + photo.FileName.Split('.')[1];
                    photo.SaveAs(HttpContext.Current.Server.MapPath("~/Content/Images/" + photoName));
                    switch (i)
                    {
                        case 1:
                            img.Image_01 = photoName;
                            break;
                        case 2:
                            img.Image_02 = photoName;
                            break;
                        case 3:
                            img.Image_03 = photoName;
                            break;
                        case 4:
                            img.Image_04 = photoName;
                            break;
                        case 5:
                            img.Image_05 = photoName;
                            break;
                    }
                }
            }

            db.Item_images.Add(img);
            db.SaveChanges();

            // Save model and brand attributes if not null
            if (!string.IsNullOrEmpty(brand))
            {
                db.uploadedItemsAttributes.Add(new uploadedItemsAttributes { Item_id = itm.Item_id, attribute_name = "Brand", attribute_value = brand });
            }
            if (!string.IsNullOrEmpty(model))
            {
                db.uploadedItemsAttributes.Add(new uploadedItemsAttributes { Item_id = itm.Item_id, attribute_name = "Model", attribute_value = model });
            }

            // Save other attributes
            foreach (var attribute in attributes)
            {
                uploadedItemsAttributes uploadedAttribute = new uploadedItemsAttributes
                {
                    Item_id = itm.Item_id,
                    attribute_name = attribute.Key,
                    attribute_value = attribute.Value
                };
                db.uploadedItemsAttributes.Add(uploadedAttribute);
            }

            string barterForItemsJson = request.Form["barterForItems"];
            List<string> barterForItems = JsonConvert.DeserializeObject<List<string>>(barterForItemsJson);

            // Save BarterFor items
            foreach (string barterForItem in barterForItems)
            {
                WishList barterForEntry = new WishList
                {
                    Item_id = itm.Item_id,
                    BarterFor = barterForItem
                };
                db.WishList.Add(barterForEntry);
            }

            db.SaveChanges();

            return itm.Item_id;
        }


        [HttpGet]
        public int getsubcategoryid(string subcategory)
        {
            var obj = db.Subcategories.Where(x => x.subcategory_name == subcategory).FirstOrDefault();

            if (obj != null)
            {
                return obj.subcategory_id;
            }
            else
            {
                return -1; // Or another appropriate default value
            }
        }



        [HttpGet]
        public List<ViewItems> GetNotificationsWithItemsAndImages(string email)
        {
            int userId = int.Parse(getUserId(email)); // Assuming getUserId returns an int

            var notificationsWithItemsAndImages = (from img in db.Item_images
                                                   join item in db.Items on img.Item_id equals item.Item_id
                                                   where item.User_id == userId
                                                   join notification in db.Notification on item.Item_id equals notification.Item_id
                                                   select new ViewItems
                                                   {
                                                       Item_id = item.Item_id,
                                                       Item_name = item.Item_name,
                                                       Description = item.Description,
                                                       Barter_for = item.Barter_for,
                                                       Image_01 = img.Image_01,
                                                       Image_02 = img.Image_02,
                                                       Image_03 = img.Image_03,
                                                       Image_04 = img.Image_04,
                                                       Image_05 = img.Image_05,
                                                       User_name = item.User.User_name,
                                                       Verification_status = item.Verification_status,
                                                       Price = item.Price,
                                                       message = notification.message,
                                                       status = notification.Status

                                                   }).ToList();

            return notificationsWithItemsAndImages;
        }

        [HttpGet]
        public int GetNumOfNotifications(string email)
        {
            var itemslist = GetNotificationsWithItemsAndImages(email);
            var filteredList = itemslist.Where(s => s.status == "Unread");

            return filteredList.Count();
        }

        [HttpGet]
        public double getAverageOfItems(String attribute)
        {
            double sum = 0;
            double average = 0;
            List<uploadedItemsAttributes> listOfTotalItems = db.uploadedItemsAttributes.Where(x => x.attribute_value == attribute).ToList();
            if (listOfTotalItems != null)
            {
                var items = db.Items.ToList();
                foreach (uploadedItemsAttributes s in listOfTotalItems)
                {
                    foreach (Item i in items)
                    {
                        if (i.Item_id == s.Item_id)
                        {
                            sum = sum + (int)(i.Price);
                        }
                    }
                }
                if (listOfTotalItems.Count() >= 1)
                {
                    average = sum / listOfTotalItems.Count();
                }
                else
                {
                    average = 0;
                }
            }
            return average;
        }


        /* [HttpPost]
         public HttpResponseMessage GetRecommendedPrice()
         {
             try
             {
                 var request = HttpContext.Current.Request;
                 List<string> specs = request.Form.GetValues("attribute")?.ToList();

                 if (specs == null || specs.Count == 0)
                 {
                     return Request.CreateResponse("No data Received");
                 }
                 string mainSpec = specs[0];
                 var initialQuery = db.ItemSpecification
                     .Where(s => s.selectedSpecification == mainSpec)
                     .Select(s => s.Item_id)
                     .Distinct()
                     .ToList();

                 List<int?> ids = new List<int?>();


                 for (int j = 1; j < specs.Count; j++)
                 {
                     string spec = specs[j];

                     var filteredIds = db.ItemSpecification
                         .Where(s => s.selectedSpecification == spec && initialQuery.Contains(s.Item_id))
                         .Select(s => s.Item_id)
                         .Distinct()
                         .ToList();



                     initialQuery = filteredIds;
                 }
                 ids.AddRange(initialQuery);

                 int totalPrice = 0;
                 int average = 0;
                 int id;
                 if (ids.Count != 0)
                 {
                     for (int i = 0; i < ids.Count; i++)
                     {
                         id = int.Parse(ids[i].ToString());
                         int price = int.Parse(db.Items.Where(s => s.Item_id == id).Select(s => s.Price).FirstOrDefault().ToString());
                         totalPrice += price;
                     }
                     average = totalPrice / ids.Count;
                 }
                 return Request.CreateResponse(average);
             }
             catch (Exception e)
             {
                 return Request.CreateResponse("Average Return Fail Function name is GetRecommended Price " + e.Message);
             }
         }*/
        [HttpGet]
        public List<ViewItems> GetRecommendation(string email)
        {

            var request = HttpContext.Current.Request;
            int userId = db.Users.Where(u => u.Email == email).Select(u => u.User_id).FirstOrDefault();
            var barterForItems = db.Items
                .Where(i => i.User_id == userId)
                .Select(i => i.Barter_for)
                .Distinct()
                .ToList();  //anything

            var itemSpecifications = db.uploadedItemsAttributes
                .Where(ispec => barterForItems.Contains(ispec.attribute_value)).Select(s => s.Item_id)
                .ToList();

            var myItemsIds = db.Items
                .Where(i => i.User_id == userId)
                .Select(i => i.Item_id)
                .ToList();

            var myItems = db.uploadedItemsAttributes
                .Where(ispec => myItemsIds.Contains((int)ispec.Item_id))
                .Select(ispec => ispec.attribute_value)
                .ToList();

            var hisItems = db.Items
                .Where(ispec => myItems.Contains(ispec.Barter_for) && itemSpecifications.Contains(ispec.Item_id))
                .Select(ispec => new { ispec.Item_id })
                .ToList();
            List<ViewItems> itemslist = new List<ViewItems>();


            foreach (var h in hisItems)
            {
                if (h.Item_id != null)
                {

                    ViewItems s1 = new ViewItems();
                    s1 = GetItemDetails(h.Item_id.ToString());

                    itemslist.Add(s1);

                }


            }



            return itemslist;




        }

        [HttpPost]
        public String SendOffer(int senderId, int RequestItemId, int price, String RequestDescription)
        {
            try
            {
                var request = HttpContext.Current.Request;


                var receiver = db.Items.FirstOrDefault(x => x.Item_id == RequestItemId);
                var selectedItemIdsJson = request["SelectedItemIds"];

                int[] selectedItemIds = JsonConvert.DeserializeObject<int[]>(selectedItemIdsJson);


                int? receiverId = receiver.User_id;

                Offer o = new Offer
                {
                    Requested_item = RequestItemId,
                    Receiver_id = receiverId,
                    Sender_id = senderId,
                    Price = price,
                    RequestDescription = RequestDescription
                };
                db.Offers.Add(o);
                db.SaveChanges();

                Request r = new Request
                {
                    Offer_id = o.Offer_id,
                    Date = DateTime.Today.ToString("dd-MM-yyyy"),
                    Req_status = "Pending",
                    ConfirmOfferRequestReceiver = "No",
                    ConfirmOfferRequestSender = "No"
                };
                db.Requests.Add(r);
                db.SaveChanges();

                if (selectedItemIds != null && selectedItemIds.Length > 0)
                {
                    foreach (int i in selectedItemIds)
                    {
                        Offered_items of = new Offered_items();
                        of.Item_id = i;
                        of.Offer_id = o.Offer_id;
                        db.Offered_items.Add(of);
                    }
                    db.SaveChanges();
                    return HttpStatusCode.OK.ToString();
                }
                else
                {
                    return "SelectedItems list is empty";
                }








            }
            catch (Exception ex)
            {
                // Log the exception with stack trace for detailed debugging
                var message = $"Internal server error: {ex.Message} \n Stack Trace: {ex.StackTrace}";
                // Consider logging to a file or monitoring system
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, message).ToString();
            }
        }

        [HttpGet]
        public List<RequestsInformation> viewRequest(string email)
        {

            var pendingRequests = db.Requests.Where(x => x.Req_status == "Pending");
            List<Offer> OffersList = new List<Offer>();


            foreach (var s in pendingRequests)
            {
                var pendingOffer = db.Offers.FirstOrDefault(x => x.Offer_id == s.Offer_id);
                if (pendingOffer != null)
                {
                    OffersList.Add(pendingOffer);
                }
            }




            string receiver = getUserId(email);
            int receiverId = int.Parse(receiver);


            // Assuming you have a DbContext named `db`
            var receiverOffers = OffersList.Where(x => x.Receiver_id == receiverId).ToList();


            List<RequestsInformation> requests = new List<RequestsInformation>();

            foreach (var offer in receiverOffers)
            {

                // Get the sender name
                var sender = db.Users.FirstOrDefault(u => u.User_id == offer.Sender_id);

                // Get the requested item name
                var requestedItem = db.Items.FirstOrDefault(i => i.Item_id == offer.Requested_item)?.Item_name;

                var price = db.Offers.FirstOrDefault(i => i.Offer_id == offer.Offer_id);

                List<int> offeredItemsIds = db.Offered_items
                    .Where(oi => oi.Offer_id == offer.Offer_id)
                    .Select(oi => oi.Item_id ?? 0)  // Coalesce null to 0
                    .ToList();



                var requestInfo = new RequestsInformation
                {
                    Receiver_name = email,
                    Sender_name = sender.User_name,
                    RequestedItemName = requestedItem,
                    Offer_id = offer.Offer_id,
                    Price = (int)price.Price,
                    OfferedItemsIds = offeredItemsIds,
                    RequestInformation = offer.RequestDescription,
                    Rating = sender.Rating,
                    SenderProfilePic = sender.Profile_Pic

                };

                // Add the offered item IDs to the list


                // Add the object to the list
                requests.Add(requestInfo);
            }

            return requests;
        }

        [HttpGet]
        public List<ViewItems> getAllItemsForOfferId(int OfferId)
        {
            var itemids = db.Offered_items.Where(x => x.Offer_id == OfferId).ToList();
            List<ViewItems> ItemsList = new List<ViewItems>();
            foreach (var s in itemids)
            {
                ViewItems v1 = new ViewItems();
                v1 = GetItemDetails(s.Item_id.ToString());
                ItemsList.Add(v1);
            }
            return ItemsList;
        }

        /*   [HttpGet]
           public List<ViewItems> GetRecommendation1(string email)
           {

               int userId = db.Users.Where(u => u.Email == email).Select(u => u.User_id).FirstOrDefault();
               var barterForItems = db.Items
                   .Where(i => i.User_id == userId)
                   .Select(i => i.Barter_for)
                   .Distinct()
                   .ToList();

               var itemSpecifications = db.uploadedItemsAttributes
                   .Where(ispec => barterForItems.Contains(ispec.attribute_value)).Select(s => s.Item_id)
                   .ToList();

               var myItemsIds = db.Items
                   .Where(i => i.User_id == userId)
                   .Select(i => i.Item_id)
                   .ToList();

               var myItems = db.uploadedItemsAttributes
                   .Where(ispec => myItemsIds.Contains((int)ispec.Item_id))
                   .Select(ispec => ispec.attribute_value)
                   .ToList();

               var hisItems = db.Items
                   .Where(ispec => myItems.Contains(ispec.Barter_for) && itemSpecifications.Contains(ispec.Item_id))
                   .Select(ispec => new { ispec.Item_id })
                   .ToList();
               var items = from h in hisItems
                           join i in db.Items
                           on h.Item_id equals i.Item_id
                           where i.User_id != userId
                           select new
                           {
                               i.User_id,
                               i.Item_id,
                               i.Item_name,
                               i.Price,
                               i.Description,
                               i.Barter_for,
                               i.Verification_status,
                               i.isSold

                           };

               var data = from i in items
                          join img in db.Item_images
                          on i.Item_id equals img.Item_id
                          select new
                          {
                              i.User_id,
                              i.Item_id,
                              i.Item_name,
                              i.Price,
                              i.Description,
                              i.Barter_for,
                              i.Verification_status,
                              img.Image_01
                          };



               var finalData = from i in data
                               join u in db.Users
                               on i.User_id equals u.User_id
                               orderby i.Item_id descending
                               select new
                               {
                                   i.User_id,
                                   i.Item_id,
                                   i.Item_name,
                                   i.Price,
                                   i.Description,
                                   i.Barter_for,
                                   i.Image_01,
                                   u.User_name,
                                   i.Verification_status,
                                   u.Rating,
                               };

               List<ViewItems> itemslist = new List<ViewItems>();

               foreach (var h in finalData)
               {
                   if (h.Item_id != null)
                   {

                       ViewItems s1 = new ViewItems();
                       s1 = GetItemDetails(h.Item_id.ToString());

                       itemslist.Add(s1);

                   }


               }



               return itemslist;
           }*/
/*
        [HttpGet]
        public HttpResponseMessage GetRecommendation1(string email)
        {
            try
            {
                var request = HttpContext.Current.Request;
                int userId = db.Users.Where(u => u.Email == email).Select(u => u.User_id).FirstOrDefault();
                var item_id = db.Items
                    .Where(i => i.User_id == userId && i.isSold != "Yes")
                    .Select(i => i.Item_id)
                    .Distinct()
                    .ToList();

                // Get the 'Barter_for' values of user's items from the new Barter table[smarte phone and tablte]
                var barterForItems = db.WishList
                    .Where(b => item_id.Contains((int)b.Item_id))
                    .Select(b => b.BarterFor)
                    .Distinct()
                    .ToList();

                var barterForItemsIds = db.WishList
                    .Where(b => item_id.Contains((int)b.Item_id))
                    .Select(b => b.Item_id)
                    .Distinct()
                    .ToList();

                // Normalize 'selectedSpecification' for item specifications //item ka nam
                var itemSpecifications = db.uploadedItemsAttributes
                    .Where(ispec => barterForItems.Contains(ispec.attribute_value))
                    .Select(s => s.Item_id)
                    .ToList();

                var myItemsIds = db.Items
                    .Where(i => i.User_id == userId && i.isSold != "Yes")
                    .Select(i => i.Item_id)
                    .ToList();
                var mycategory = db.Items
                    .Where(i => i.User_id == userId && i.isSold != "Yes")
                    .Select(i => i.Category)
                    .ToList();

                var mysubcategory = db.Items
                    .Where(i => i.User_id == userId)
                    .Select(i => i.SubCategory)
                    .ToList();

                // Normalize 'selectedSpecification' for user's item specifications
                var myItems = db.uploadedItemsAttributes
                    .Where(ispec => myItemsIds.Contains((int)ispec.Item_id))
                    .Select(ispec => ispec.attribute_value)
                    .ToList();

                // Find matching items based on normalized 'Barter_for' and 'selectedSpecification'


                var hisItems = db.WishList
                    .Where(b => myItems.Contains(b.BarterFor) || (mycategory.Contains(b.BarterFor) || mysubcategory.Contains(b.BarterFor)))
                    .Select(b => b.Item_id)
                    .Distinct()
                    .ToList();

                var hisItemsBf = db.WishList
                    .Where(b => myItems.Contains(b.BarterFor) || (mycategory.Contains(b.BarterFor) || mysubcategory.Contains(b.BarterFor)))
                    .Select(b => b.BarterFor)
                    .ToList();


                var chkcategory = db.Items.Where(s => (hisItems.Contains(s.Item_id) && itemSpecifications.Contains(s.Item_id)) || (barterForItems.Contains(s.Category) || barterForItems.Contains(s.SubCategory))).Select(b => b.Item_id)
                    .Distinct()
                    .ToList();
                ;


                var matchBoth = hisItems.Where(s => barterForItemsIds.Contains(s));

                var categoriesID = db.Items.Where(s => barterForItems.Contains(s.Category) || barterForItems.Contains(s.SubCategory))
                                 .Select(b => b.Item_id)
                                 .Distinct()
                                 .ToList();
                var chkBarterFor = db.WishList.Where(s => categoriesID.Contains((int)s.Item_id)).Select(s => s.BarterFor);
                var confirm = barterForItems.Where(s => chkBarterFor.Contains(s));


                var category = db.Items.Where(s => chkcategory.Contains(s.Item_id) && s.isSold != "Yes")
                    .Select(s => s.Item_id)
                    .ToList();


                var items = from h in category
                            join i in db.Items on h equals i.Item_id
                            where i.User_id != userId
                            select new
                            {
                                i.User_id,
                                i.Item_id,
                                i.Item_name,
                                i.Price,
                                i.Description,
                                i.Verification_status,
                                i.Barter_for,
                                i.Category,
                                i.SubCategory,
                                
                               
                                
                            };

                var data = from i in items
                           join img in db.Item_images on i.Item_id equals img.Item_id
                           select new
                           {
                               i.User_id,
                               i.Item_id,
                               i.Item_name,
                               i.Price,
                               i.Description,
                               img.Image_01,
                               i.Barter_for,
                               i.Verification_status,
                               i.Category,
                               i.SubCategory,
                               img.Image_02,
                               img.Image_03,
                               img.Image_04,
                               img.Image_05
                               
                           };



                var finalData = from i in data
                                join u in db.Users on i.User_id equals u.User_id
                                orderby i.Item_id descending
                                select new
                                {
                                    i.User_id,
                                    i.Item_id,
                                    i.Item_name,
                                    i.Barter_for,
                                    i.Price,
                                    i.Description,
                                    i.Image_01,
                                    i.Image_02,
                                    i.Image_03,
                                    i.Image_04,
                                    i.Image_05,                                 
                                   u.User_name,
                                    i.Verification_status,
                                    u.Rating,
                                    i.Category,
                                    i.SubCategory,

                                   
                                };



              *//*  Item_id = i.Item_id,
                                Item_name = i.Item_name,
                                Price = (int)i.Price,
                                Description = i.Description,
                                Barter_for = i.Barter_for,
                                Image_01 = i.Image_01,
                                Image_02 = i.Image_02,
                                Image_03 = i.Image_03,
                                Image_04 = i.Image_04,
                                Image_05 = i.Image_05,
                                Verification_status = i.Verification_status,
                                User_name = u.User_name,
                                Rating = u.Rating,
                                ProfilePic = u.Profile_Pic,
                                User_id = u.User_id,
                                Category = i.Category,
                                subCategory = i.SubCategory*//*

                return Request.CreateResponse(HttpStatusCode.OK, finalData);
            }
            catch (Exception e)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error in GetRecommendation: " + e.Message);
            }
        }
*/


        public HttpResponseMessage GetRecommendation1(string email)
        {
            try
            {

                List<ViewItems> itemsList = new List<ViewItems>();
                var userID = db.Users.Where(s => s.Email == email).Select(s => s.User_id).FirstOrDefault();

                var userItems = db.Items.Where(s => s.User_id == userID).Select(s => s.Item_id).ToList();

                var wishList = db.WishList.Where(s => userItems.Contains((int)s.Item_id)).Select(s => s.BarterFor).ToList();

                var matchedItems = db.uploadedItemsAttributes.Where(s => wishList.Contains(s.attribute_value)).Select(s => s.Item_id).ToList();

                var data2 = db.Items.Where(s => matchedItems.Contains(s.Item_id) && s.isSold != "Yes" && s.User_id != userID).Select(i => new {
                    i.User_id,
                    i.Item_id,
                    i.Item_name,
                    i.Barter_for,
                    i.Price,
                    i.Description,
                    i.Verification_status,
                    i.Category,
                    i.SubCategory

                }).ToList();

                var itemWithUserData = from i in data2
                                       join u in db.Users on i.User_id equals u.User_id
                                       where u.User_id != userID
                                       select new
                                       {
                                           i.Item_id,
                                           i.Item_name,
                                           i.Price,
                                           i.Description,
                                           i.Barter_for,                                        
                                           i.Verification_status,
                                           u.User_name,
                                           u.Rating,
                                           u.Profile_Pic,
                                           u.User_id,
                                           i.Category,
                                           i.SubCategory
                                       };

                var itemImages = from i in itemWithUserData
                                 join img in db.Item_images
                                 on i.Item_id equals img.Item_id
                                 select new
                                 {
                                     i.User_id,
                                     i.Item_id,
                                     i.Item_name,
                                     i.Barter_for,
                                     i.Price,
                                     i.Description,
                                     i.Verification_status,
                                     img.Image_01,
                                     img.Image_02,
                                     img.Image_03,
                                     img.Image_04,
                                     img.Image_05,
                                     i.Category,
                                     i.SubCategory,
                                     i.User_name,
                                     i.Rating,
                                     i.Profile_Pic
                                     
                                 };


                // Step 3: Join with BarterFor table to get barter information
                var finalData1 = from i in itemImages
                                join b in db.WishList on i.Item_id equals b.Item_id into barterGroup
                                from bg in barterGroup.DefaultIfEmpty()
                                group bg by new
                                {
                                    i.Item_id,
                                    i.Item_name,
                                    i.Price,
                                    i.Description,
                                    i.Barter_for,
                                    i.Image_01,
                                    i.Image_02,
                                    i.Image_03,
                                    i.Image_04,
                                    i.Image_05,
                                    i.Verification_status,
                                    i.User_name,
                                    i.Rating,
                                    i.Profile_Pic,
                                    i.User_id,
                                    i.Category,
                                    i.SubCategory
                                } into g
                                select new ViewItems
                                {
                                    Item_id = g.Key.Item_id,
                                    Item_name = g.Key.Item_name,
                                    Price = (int)g.Key.Price,
                                    Description = g.Key.Description,
                                    Barter_for = g.Key.Barter_for,
                                    Image_01 = g.Key.Image_01,
                                    Image_02 = g.Key.Image_02,
                                    Image_03 = g.Key.Image_03,
                                    Image_04 = g.Key.Image_04,
                                    Image_05 = g.Key.Image_05,
                                    Verification_status = g.Key.Verification_status,
                                    User_name = g.Key.User_name,
                                    Rating = g.Key.Rating,
                                    ProfilePic = g.Key.Profile_Pic,
                                    User_id = g.Key.User_id,
                                    Category = g.Key.Category,
                                    subCategory = g.Key.SubCategory,
                                    BarterItems = g.Select(x => x.BarterFor).ToList()
                                };


                itemsList.AddRange(finalData1);


                return Request.CreateResponse(finalData1);
            }
            catch (Exception er)
            {
                return Request.CreateResponse(er);
            }
        }



        /*    [HttpPost]
            public void SubmitRating([FromBody] RatingModel ratingModel)
            {
                if (ratingModel == null || ratingModel.RatingValue < 1 || ratingModel.RatingValue > 5)
                {
                    return BadRequest("Invalid rating data.");
                }

                // Add the rating to the database
                Rating rating = new Rating
                {
                    UserId = ratingModel.UserId,
                    RatedByUserId = ratingModel.RatedByUserId,
                    RatingValue = ratingModel.RatingValue,
                    OfferId = ratingModel.OfferId,
                    Timestamp = DateTime.Now
                };

                db.Ratings.Add(rating);
                db.SaveChanges();

                // Update the user's average rating
                var userRatings = db.Ratings.Where(r => r.UserId == ratingModel.UserId).ToList();
                float averageRating = userRatings.Average(r => r.RatingValue);
                int ratingCount = userRatings.Count;

                var user = db.Users.FirstOrDefault(u => u.UserId == ratingModel.UserId);
                user.AverageRating = averageRating;
                user.RatingCount = ratingCount;

                db.SaveChanges();

                return Ok();
            }
    */

        [HttpGet]
        public List<RequestsInformation> ViewHistory(string email)
        {
            // Fetch the user's ID
            var userId = getUserId(email);
            int userIdInt = int.Parse(userId);

            // Fetch all offers where the user is either the sender or the receiver
            var userOffers = db.Offers
                .Where(o => o.Sender_id == userIdInt || o.Receiver_id == userIdInt)
                .ToList();

            // Get the offer IDs associated with the user's offers
            var offerIds = userOffers.Select(o => o.Offer_id).ToList();

            // Fetch all requests associated with the user's offer IDs
            var userRequests = db.Requests
                .Where(r => offerIds.Contains((int)r.Offer_id) && r.Req_status == "Accepted")
                .ToList();

            // Initialize a list to store the request information
            var requests = new List<RequestsInformation>();

            foreach (var offer in userOffers)
            {
                // Get the request associated with this offer
                var request = userRequests.FirstOrDefault(r => r.Offer_id == offer.Offer_id);

                // If the request is null, continue to the next offer
                if (request == null)
                {
                    continue;
                }

                // Get sender and receiver names
                var sender = db.Users.FirstOrDefault(u => u.User_id == offer.Sender_id);
                var receiver = db.Users.FirstOrDefault(u => u.User_id == offer.Receiver_id);

                // Get the requested item name
                var requestedItem = db.Items.FirstOrDefault(i => i.Item_id == offer.Requested_item)?.Item_name;

                // Get the price
                int price = (int)offer.Price;

                // Get offered item IDs
                var offeredItemsIds = db.Offered_items
                    .Where(oi => oi.Offer_id == offer.Offer_id)
                    .Select(oi => oi.Item_id ?? 0)
                    .ToList();

                // Create the request information object
                var requestInfo = new RequestsInformation
                {
                    Receiver_name = receiver.User_name,
                    Sender_name = sender.User_name,
                    RequestedItemName = requestedItem,
                    Offer_id = offer.Offer_id,
                    Price = price,
                    OfferedItemsIds = offeredItemsIds,
                    RequestInformation = offer.RequestDescription,
                    status = request.Req_status,
                    senderId = offer.Sender_id,
                    ReceiverId = offer.Receiver_id,
                    ConfirmOfferRequestReceiver = request.ConfirmOfferRequestReceiver,
                    ConfirmOfferRequestSeder = request.ConfirmOfferRequestSender,
                    SenderProfilePic = sender.Profile_Pic,
                    ReceiverProfilePic = receiver.Profile_Pic
                };

                // Add the object to the list
                requests.Add(requestInfo);
            }

            return requests;
        }


        [HttpGet]
        public List<ViewItems> GetRecommendationLastUploaded(string email)
        {
            // Retrieve the user ID based on the provided email
            int userId = db.Users.Where(u => u.Email == email).Select(u => u.User_id).FirstOrDefault();

            // Get the last uploaded item by the user
            var lastUploadedItem = db.Items
                .Where(i => i.User_id == userId)
                .OrderByDescending(i => i.Item_id) // Assuming there's an UploadedDate field
                .FirstOrDefault();

            if (lastUploadedItem == null)
            {
                return new List<ViewItems>(); // Return an empty list if no items are found
            }

            // Get the Barter_for value of the last uploaded item
            string barterFor = lastUploadedItem.Barter_for;

            // Get item IDs that have attributes matching the Barter_for value of the last uploaded item
            var itemSpecifications = db.uploadedItemsAttributes
                .Where(ispec => ispec.attribute_value == barterFor)
                .Select(s => s.Item_id)
                .ToList();

            // Get attributes of the last uploaded item
            var myItemAttributes = db.uploadedItemsAttributes
                .Where(ispec => ispec.Item_id == lastUploadedItem.Item_id)
                .Select(ispec => ispec.attribute_value)
                .ToList();

            // Find items that match the attributes of the last uploaded item and exclude user's own items
            var recommendedItems = db.Items
                .Where(i => myItemAttributes.Contains(i.Barter_for) &&
                            itemSpecifications.Contains(i.Item_id) &&
                            i.User_id != userId) // Exclude user's own items
                .Select(i => new { i.Item_id })
                .ToList();

            // Create a list of ViewItems
            List<ViewItems> itemsList = new List<ViewItems>();

            // Populate the list with details of recommended items
            foreach (var item in recommendedItems)
            {
                if (item.Item_id != null)
                {
                    ViewItems viewItem = GetItemDetails(item.Item_id.ToString());
                    itemsList.Add(viewItem);
                }
            }

            return itemsList;
        }

        [HttpGet]
        public int getRequestsCount(String email)
        {
            String id = getUserId(email);
            int UserId = int.Parse(id);
            var OffersReceived = db.Offers.Where(x => x.Receiver_id == UserId).ToList();
            int RequestCount = 0;
            foreach (var s in OffersReceived)
            {
                var Request = db.Requests.Where(x => x.Offer_id == s.Offer_id).FirstOrDefault();
                if (Request.Req_status == "Accepted" || Request.Req_status == "Rejected")
                {

                }
                else
                {
                    RequestCount++;
                }
            }
            return RequestCount;

        }

        //rating=sumOfTotalRating/totalRatings
        [HttpPost]
        public void GiveRating(int RatingGiver, int RatingTaker, float value)
        {
            var existingRating = db.Rating
                                    .FirstOrDefault(r => r.rated_by_user_id == RatingGiver && r.user_id == RatingTaker);

            if (existingRating != null)
            {
                // Update existing rating
                existingRating.rating_value = (int)value;
                existingRating.Date = DateTime.Today.ToString("dd-MM-yyyy");
            }
            else
            {
                // Add new rating
                Rating rating = new Rating();
                rating.rated_by_user_id = RatingGiver;
                rating.rating_value = (int)value;
                rating.user_id = RatingTaker;
                rating.Date = DateTime.Today.ToString("dd-MM-yyyy");

                db.Rating.Add(rating);
            }

            db.SaveChanges();

            // Update user's rating count and average rating
            var user = db.Users.FirstOrDefault(x => x.User_id == RatingTaker);
            if (user != null)
            {
                var totalRatings = db.Rating.Where(x => x.user_id == RatingTaker).ToList();
                int? sumOfTotalRatings = totalRatings.Sum(r => r.rating_value);
                user.rating_count = totalRatings.Count();
                int? RatingAverage = sumOfTotalRatings / totalRatings.Count();

                user.Rating = RatingAverage;

                db.SaveChanges();
            }
        }

        [HttpGet]
        public int? GetReceiverId(int OfferId)
        {
            var receiverId = db.Offers.Where(x => x.Offer_id == OfferId).FirstOrDefault();
            return receiverId.Receiver_id;
        }

        [HttpGet]
        public int? GetSenderId(int OfferId)
        {
            var SenderId = db.Offers.Where(x => x.Offer_id == OfferId).FirstOrDefault();
            return SenderId.Sender_id;
        }



        [HttpGet]
        public User getUserDetails(String email)
        {
            var userDetails = db.Users.Where(x => x.Email == email).FirstOrDefault();

            User s1 = new User();
            s1.Email = email;
            s1.User_id = userDetails.User_id;
            s1.Gender = userDetails.Gender;
            s1.Location = userDetails.Location;
            s1.Password = userDetails.Password;
            s1.Contact = userDetails.Contact;
            s1.Profile_Pic = userDetails.Profile_Pic;
            s1.rating_count = userDetails.rating_count;
            s1.Rating = userDetails.Rating;
            s1.User_name = userDetails.User_name;

            return s1;

        }
        [HttpPost]
        public void UpdateProfile(String email)
        {

            var request = HttpContext.Current.Request;
            String user = getUserId(email);
            int userId = int.Parse(user);
            var UserDetails = db.Users.Where(x => x.User_id == userId).FirstOrDefault();
            String contact = request["contact"];
            String newPassword = request["password"];
            String PP = "";

            var photo = request.Files["image"];
            if (photo != null)
            {
                string photoName = (user + "_") + "." + photo.FileName.Split('.')[1];
                photo.SaveAs(HttpContext.Current.Server.MapPath("~/Content/Images/" + photoName));
                PP = photoName;
                UserDetails.Profile_Pic = photoName;
            }
            if (contact != "")
            {
                UserDetails.Contact = contact;
            }
            if (newPassword != "")
            {
                UserDetails.Password = newPassword;
            }
            db.SaveChanges();


        }

        [HttpPost]
        public void confirmOfferReceiver(int offerId)
        {
            var request = db.Requests.Where(x => x.Offer_id == offerId).FirstOrDefault();
            request.ConfirmOfferRequestReceiver = "Yes";
            db.SaveChanges();
        }


        [HttpPost]
        public void confirmOfferSender(int offerId)
        {
            var request = db.Requests.Where(x => x.Offer_id == offerId).FirstOrDefault();
            request.ConfirmOfferRequestSender = "Yes";
            db.SaveChanges();
        }

        [HttpGet]
        public List<String> GetAttributes(String subcategory)
        {
            List<String> attributes = new List<string>();

            if (string.IsNullOrEmpty(subcategory))
            {
                return attributes; // Return empty list if subcategory is null or empty
            }

            var subcat = db.Subcategories.FirstOrDefault(x => x.subcategory_name == subcategory);

            if (subcat == null)
            {
                return attributes; // Return empty list if subcategory is not found
            }

            var BrandCheck = db.Brands.Where(x => x.subcategory_id == subcat.subcategory_id).ToList();

            if (BrandCheck != null && BrandCheck.Any())
            {
                foreach (var brand in BrandCheck)
                {
                    var models = db.Models.Where(x => x.brand_id == brand.brand_id).ToList();
                    foreach (var model in models)
                    {
                        attributes.Add(model.model_name);
                    }
                }
            }
            else
            {
                var subcatAttributes = db.Subcategory_Attributes.FirstOrDefault(x => x.subcategory_id == subcat.subcategory_id);
                if (subcatAttributes != null)
                {
                    var attributevalues = db.Attribute_Values.Where(x => x.attribute_id == subcatAttributes.attribute_id).ToList();
                    foreach (var attributeValue in attributevalues)
                    {
                        attributes.Add(attributeValue.value);
                    }
                }
            }

            return attributes;
        }
        [HttpGet]
        public List<ViewItems> AdvanceSearch(string category, string subcategory, float? rating, string minPrice, string maxPrice, string email)
        {
            List<ViewItems> itemsList = new List<ViewItems>();

            // Get user ID based on email
            string userId = getUserId(email); // Assuming you have a way to get user ID based on email
            if (string.IsNullOrEmpty(userId))
            {
                // Handle case where userId is not found or valid
                return itemsList; // or throw exception, return error response, etc.
            }

            int id = int.Parse(userId);

            // Query to retrieve items and associated images
            var dataQuery = from i in db.Items
                            join img in db.Item_images on i.Item_id equals img.Item_id
                            where i.isSold == "No"
                            select new
                            {
                                i.User_id,
                                i.Item_id,
                                i.Item_name,
                                i.Price,
                                i.Description,
                                i.Verification_status,
                                i.Barter_for,
                                img.Image_01,
                                img.Image_02,
                                img.Image_03,
                                img.Image_04,
                                img.Image_05,
                                i.Category,
                                i.SubCategory,
                                i.Date
                            };

            if (!string.IsNullOrEmpty(category) && category.ToLower() != "all")
            {
                dataQuery = dataQuery.Where(x => x.Category == category);
            }

            if (!string.IsNullOrEmpty(subcategory) && subcategory.ToLower() != "all")
            {
                dataQuery = dataQuery.Where(x => x.SubCategory == subcategory);
            }




            decimal minPriceValue;
            if (!string.IsNullOrEmpty(minPrice) && decimal.TryParse(minPrice, out minPriceValue))
            {
                dataQuery = dataQuery.Where(x => x.Price >= minPriceValue);
            }

            decimal maxPriceValue;
            if (!string.IsNullOrEmpty(maxPrice) && decimal.TryParse(maxPrice, out maxPriceValue))
            {
                dataQuery = dataQuery.Where(x => x.Price <= maxPriceValue);
            }

            if (rating.HasValue)
            {
                dataQuery = dataQuery.Where(x => db.Users.Any(u => u.User_id == x.User_id && u.Rating >= rating));
            }



            // Projection to ViewItems
            var finalDataQuery = from i in dataQuery
                                 join u in db.Users on i.User_id equals u.User_id
                                 where u.User_id != id // Exclude items from the current user
                                 select new ViewItems
                                 {
                                     Item_id = i.Item_id,
                                     Item_name = i.Item_name,
                                     Price = (int)i.Price,
                                     Description = i.Description,
                                     Barter_for = i.Barter_for,
                                     Image_01 = i.Image_01,
                                     Image_02 = i.Image_02,
                                     Image_03 = i.Image_03,
                                     Image_04 = i.Image_04,
                                     Image_05 = i.Image_05,
                                     Verification_status = i.Verification_status,
                                     User_name = u.User_name,
                                     Rating = u.Rating,
                                     ProfilePic = u.Profile_Pic,
                                     User_id = u.User_id,
                                     Category = i.Category,
                                     subCategory = i.SubCategory,
                                     date = DbFunctions.TruncateTime(i.Date).ToString() // Handle nullable date using DbFunctions

                                 };

            itemsList.AddRange(finalDataQuery);

            return itemsList;
        }
        private string FormatDateString(string dateString)
        {
            // Assuming dateString is in a format you can parse, like "yyyy-MM-dd"
            if (DateTime.TryParse(dateString, out DateTime parsedDate))
            {
                return parsedDate.ToString("yyyy-MM-dd");
            }
            else
            {
                return string.Empty; // Handle invalid date format gracefully
            }
        }


        /*
                [HttpGet]
                public HttpResponseMessage GetRecommendation3(string email)
                {
                    try
                    {
                        var request = HttpContext.Current.Request;
                        int userId = db.Users.Where(u => u.Email == email).Select(u => u.User_id).FirstOrDefault();
                        var item_id = db.Items
                            .Where(i => i.User_id == userId)
                            .Select(i => i.Item_id)
                            .Distinct()
                            .ToList();

                        // Get the 'Barter_for' values of user's items from the new Barter table[smarte phone and tablte]
                        var barterForItems = db.BarterFor
                            .Where(b => item_id.Contains((int)b.Item_id))
                            .Select(b => b.BarterFor1)
                            .Distinct()
                            .ToList();

                        var barterForItemsIds = db.BarterFor
                            .Where(b => item_id.Contains((int)b.Item_id))
                            .Select(b => b.Item_id)
                            .Distinct()
                            .ToList();

                        // Normalize 'selectedSpecification' for item specifications //item ka nam
                        var itemSpecifications = db.uploadedItemsAttributes
                            .Where(ispec => barterForItems.Contains(ispec.attribute_value))
                            .Select(s => s.Item_id)
                            .ToList();

                        var myItemsIds = db.Items
                            .Where(i => i.User_id == userId)
                            .Select(i => i.Item_id)
                            .ToList();
                        var mycategory = db.Items
                            .Where(i => i.User_id == userId)
                            .Select(i => i.Category)
                            .ToList();

                        var mysubcategory = db.Items
                            .Where(i => i.User_id == userId)
                            .Select(i => i.SubCategory)
                            .ToList();

                        // Normalize 'selectedSpecification' for user's item specifications
                        var myItems = db.uploadedItemsAttributes
                            .Where(ispec => myItemsIds.Contains((int)ispec.Item_id))
                            .Select(ispec => ispec.attribute_value)
                            .ToList();

                        // Find matching items based on normalized 'Barter_for' and 'selectedSpecification'
                        var hisItems = db.BarterFor
                            .Where(b => myItems.Contains(b.BarterFor1) || (mycategory.Contains(b.BarterFor1) || mysubcategory.Contains(b.BarterFor1)))
                            .Select(b => b.Item_id)
                            .Distinct()
                            .ToList();

                        var hisItemsBf = db.BarterFor
                            .Where(b => myItems.Contains(b.BarterFor1) || (mycategory.Contains(b.BarterFor1) || mysubcategory.Contains(b.BarterFor1)))
                            .Select(b => b.BarterFor1)
                            .ToList();


                        var chkcategory = db.Items.Where(s => hisItems.Contains(s.Item_id) && (barterForItems.Contains(s.Category) || barterForItems.Contains(s.SubCategory))).Select(b => b.Item_id)
                            .Distinct()
                            .ToList();
                        ;


                        var matchBoth = hisItems.Where(s => barterForItemsIds.Contains(s));

                        *//*var categoriesID=db.Items.Where(s=>barterForItems.Contains(s.Category)||barterForItems.Contains(s.SubCategory))
                                         .Select(b => b.Item_id)
                                         .Distinct()
                                         .ToList();
                        var chkBarterFor = db.Barter_for.Where(s => categoriesID.Contains((int)s.Item_id)).Select(s => s.Barter_for1);
                        var confirm = barterForItems.Where(s => chkBarterFor.Contains(s));
        *//*

                        var category = db.Items.Where(s => chkcategory.Contains(s.Item_id))
                            .Select(s => s.Item_id)
                            .ToList();


                        var items = from h in category
                                    join i in db.Items on h equals i.Item_id
                                    where i.User_id != userId
                                    select new
                                    {
                                        i.User_id,
                                        i.Item_id,
                                        i.Item_name,
                                        i.Price,
                                        i.Description,
                                        i.Verification_status,
                                        i.Barter_for
                                    };

                        var data = from i in items
                                   join img in db.Item_images on i.Item_id equals img.Item_id
                                   select new
                                   {
                                       i.User_id,
                                       i.Item_id,
                                       i.Item_name,
                                       i.Price,
                                       i.Description,
                                       img.Image_01,
                                       i.Barter_for,
                                       i.Verification_status,
                                   };





                        var finalData = from i in data
                                        join u in db.Users on i.User_id equals u.User_id
                                        orderby i.Item_id descending
                                        select new
                                        {
                                            i.User_id,
                                            i.Item_id,
                                            i.Item_name,
                                            i.Barter_for,
                                            i.Price,
                                            i.Description,
                                            i.Image_01,
                                            u.User_name,
                                            i.Verification_status,
                                            u.Rating,
                                        };

                        return Request.CreateResponse(HttpStatusCode.OK, finalData);
                    }
                    catch (Exception e)
                    {
                        return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Error in GetRecommendation: " + e.Message);
                    }
                }
            }*/

        [HttpPost]
        public void updateWishList()
        {
            var request = HttpContext.Current.Request;


            string barterForItemsJson = request.Form["barterForItems"];
            int ItemId = int.Parse(request.Form["ItemId"]);
            List<string> barterForItems = JsonConvert.DeserializeObject<List<string>>(barterForItemsJson);

            var wishes = db.WishList.Where(x => x.Item_id == ItemId);

            db.WishList.RemoveRange(wishes);

            foreach (string s in barterForItems)
            {
                WishList s1 = new WishList();
                s1.BarterFor = s;
                s1.Item_id = ItemId;
                db.WishList.Add(s1);

            }
            db.SaveChanges();



        }

        [HttpGet]
        public List<ViewItems> getRecommendedNotification(String email)
        {
            int userId = int.Parse(getUserId(email));


        }

    }
}
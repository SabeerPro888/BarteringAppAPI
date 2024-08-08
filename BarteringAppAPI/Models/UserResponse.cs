using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BarteringAppAPI.Models
{
    public class UserResponse
    {
        public String email { get; set; }
        public String password { get; set; }
        public String UserType { get; set; }

    }
}
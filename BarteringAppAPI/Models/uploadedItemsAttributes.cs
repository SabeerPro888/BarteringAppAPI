//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BarteringAppAPI.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class uploadedItemsAttributes
    {
        public int id { get; set; }
        public Nullable<int> Item_id { get; set; }
        public string attribute_name { get; set; }
        public string attribute_value { get; set; }
    
        public virtual Item Items { get; set; }
    }
}

//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DA
{
    using System;
    using System.Collections.Generic;
    
    public partial class City
    {
        public City()
        {
            this.General = new HashSet<General>();
        }
    
        public int Id { get; set; }
        public string CityName { get; set; }
        public int ZipCode { get; set; }
    
        public virtual ICollection<General> General { get; set; }
    }
}

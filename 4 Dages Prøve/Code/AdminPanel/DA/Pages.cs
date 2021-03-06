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
    
    public partial class Pages
    {
        public Pages()
        {
            this.Pages1 = new HashSet<Pages>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string Body { get; set; }
        public int FK_Status { get; set; }
        public System.DateTime CreatedDate { get; set; }
        public int SortIndex { get; set; }
        public Nullable<int> FK_Parent { get; set; }
        public int FK_User { get; set; }
        public string SEO_KeyWords { get; set; }
        public string SEO_Description { get; set; }
        public int PageTemplate { get; set; }
        public Nullable<int> Fk_Media { get; set; }
        public int indentlevel { get; set; }
    
        public virtual Media Media { get; set; }
        public virtual ICollection<Pages> Pages1 { get; set; }
        public virtual Pages Pages2 { get; set; }
        public virtual Status Status { get; set; }
        public virtual UserProfile UserProfile { get; set; }
    }
}

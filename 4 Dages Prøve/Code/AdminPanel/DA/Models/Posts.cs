using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace DA
{
    public partial class Posts
    {
        public string tags { get; set; }


       
        
    }

    public class CategoryInPost_Checkup
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public bool isChecked { get; set; }
    }

    public class MediaInPost_Checkup
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public string path { get; set; }
        public bool isChecked { get; set; }
        public int? MediainpostID {get; set; }
    }

    
}

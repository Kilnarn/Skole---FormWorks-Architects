using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DA
{
    public class GetInformation
    {
        public IQueryable<Posts> GetPosts { get; set; }
        public IQueryable<Pages> GetPages { get; set; }
        public IQueryable<Categories> GetCateGories { get; set; }
        public IQueryable<Tags> GetTags { get; set; }


        public Posts NewsPosts { get; set; }

        
    }
}

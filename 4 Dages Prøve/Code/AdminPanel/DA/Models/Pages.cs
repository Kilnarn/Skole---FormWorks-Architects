using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DA.Helpers;

namespace DA
{
    partial class Pages
    {

        public ICollection<Pages> Children { get; set; }




        /*public static ICollection<Pages> ListPages(this ICollection<Pages> source)
        {

            var result = new List<Pages>();
            foreach (Pages pages in source)
            {
                if (pages.Children != null && pages.Children.Count() > 0)
                {
                    var page = new Pages { Id = pages.Id, FK_Parent = pages.FK_Parent };
                    pages.Children = pages.Children.ToList();
                    result.Add(pages);
                }
            }
            return result;

        }*/
        
     
    }    
}

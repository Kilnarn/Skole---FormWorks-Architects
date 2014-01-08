using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DA
{
    public partial class Media
    {
        //public string AttachedIn { get; set; }

        public HttpPostedFileBase File { get; set; }
    }
}

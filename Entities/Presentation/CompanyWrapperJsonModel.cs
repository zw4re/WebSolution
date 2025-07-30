using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Presentation
{
    public class CompanyWrapperJsonModel
    {
        public string code { get; set; }
        public List<CompanyJsonModel> content { get; set; }
    }
}

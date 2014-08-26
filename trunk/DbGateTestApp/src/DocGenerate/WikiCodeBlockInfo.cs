using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbGateTestApp.DocGenerate
{
    public class WikiCodeBlockInfo : Attribute
    {
        public string Id;
        public string Code;

        public WikiCodeBlockInfo(string id,string code)
        {
            Id = id;
            Code = code;
        }
    }
}

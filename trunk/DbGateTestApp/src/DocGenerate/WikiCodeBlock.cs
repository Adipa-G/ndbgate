using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbGateTestApp.DocGenerate
{
    public class WikiCodeBlock : Attribute
    {
        public string Id;

        public WikiCodeBlock(string id)
        {
            Id = id;
        }
    }
}

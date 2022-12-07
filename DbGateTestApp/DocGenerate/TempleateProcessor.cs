using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DbGateTestApp.DocGenerate
{
    public class TempleateProcessor
    {
        private const String WikiBlockTag = "wiki_code_block";

        public static string ProcessTemplate(string templatePath, ICollection<WikiCodeBlockInfo> codeBlocks)
        {
            var templateText = File.ReadAllText(templatePath);

            var pattern = string.Format(@"(<\s*{0}\s*>)([^<]*)(<\/\s*{0}\s*>)",WikiBlockTag);
            var match = Regex.Match(templateText, pattern);
            while (match.Success)
            {
                var blockId = match.Groups[2].Value;
                var blockInfo = codeBlocks.First(cb => blockId.Equals(cb.Id));

                templateText = templateText.Replace(match.Value, blockInfo.Code);
                match = Regex.Match(templateText, pattern);
            }

            return templateText;
        }
    }
}

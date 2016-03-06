using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbGateTestApp.DocGenerate
{
    public class Integrator
    {
        public const string WikiTemplateExtension = "wiki.template";
        public const string WikiExtension = "md";
	  	public const string CsharpSourceExtension = "cs";
	  	
	  	public static void DoProcess(String srcRoot,String srcOut)
	  	{
	  	    var allWikiTemplateFiles = Directory.GetFiles(srcRoot, String.Format("*.{0}", WikiTemplateExtension),
	  	                                                  SearchOption.AllDirectories);
	  	    var allCsFiles = Directory.GetFiles(srcRoot, String.Format("*.{0}", CsharpSourceExtension),
	  	                                        SearchOption.AllDirectories);
	  	    var allCodeBlocks = CodeReader.ReadAndExtractWikiBlocks(allCsFiles);
	  	
	  	    foreach (string wikiTemplateFile in allWikiTemplateFiles)
	  	    {
                string wikiText = TempleateProcessor.ProcessTemplate(wikiTemplateFile, allCodeBlocks);
	  	        var file = new FileInfo(Path.Combine(srcOut,new FileInfo(wikiTemplateFile).Name.Replace(WikiTemplateExtension,WikiExtension)));
                File.WriteAllText(file.FullName,wikiText);
	  	    }
	  	}
    }
}

﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DbGateTestApp.DocGenerate
{
    public class CodeReader
    {
        private static readonly string BlockAttributeName = typeof(WikiCodeBlock).Name;
	  	 	

	  	public static ICollection<WikiCodeBlockInfo> ReadAndExtractWikiBlocks(ICollection<string> sourceFileList)
	  	{
            var allBlocks = new List<WikiCodeBlockInfo>();
	  	    foreach (string srcFile in sourceFileList)
	  	    {
	  	        string fileContent = File.ReadAllText(srcFile);
	  	        allBlocks.AddRange(FindBlocks(fileContent));
	  	    }
	  	    return allBlocks;
	  	}
	  	 	
	  	private static IEnumerable<WikiCodeBlockInfo> FindBlocks(string source)
	  	{
	  	 	int curPos = 0;
	  	 	var infoList = new List<WikiCodeBlockInfo>();

	  	    var match = Regex.Match(source,string.Format(@"(\[\s*{0}\(\s*\"")([^\""]*)(\""\s*\)\s*\])",BlockAttributeName));
	  	    while (match.Success)
	  	 	{
                var info = new WikiCodeBlockInfo(match.Groups[2].Value,ReadBlock(source,match.Index + match.Length));
                infoList.Add(info);
                curPos += match.Length;

	  	 	    match = match.NextMatch();
	  	 	}
	  	    return infoList;
	  	}
	  	 	
	  	private static string ReadBlock(String source, int position)
	  	{
	  	 	var block = new StringBuilder();
	  	 	bool blockStart = false;
	  	 	int bracketCount = 0;
	  	 	char[] chars = source.Substring(position).ToCharArray();
	  	 	
	  	 	foreach (char aChar in chars)
            {
	  	 	    switch (aChar)
	  	 	    {
	  	 	        case '{':
                        blockStart = true;
	  	 	            bracketCount++;
	  	 	            break;
	  	 	        case '}':
	  	 	            bracketCount--;
	  	 	            break;
	  	 	    }
	  	 	
	  	 	    block.Append(aChar);
	  	 	    if (blockStart && bracketCount == 0)
	  	 	    {
	  	 	        break;
	  	 	    }
	  	 	}
	  	 	return block.ToString();
	  	}
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace CQRSCore.TestFramework.Console
{
	class Program
	{
		static void Main(string[] args)
		{
			string fileName = string.Empty;
			string outputPath = string.Empty;
			ParseArguments(args, out fileName, out outputPath);


			List<string> textSpec = File.ReadAllLines(fileName).ToList();
			TextParser p = new TextParser();
			p.Parse(textSpec);
			foreach (var test in p.Tests)
			{
				string outputFile = Path.Combine(outputPath, String.Format(@"{0}_{1}.cs", test.Given, test.When));
				File.WriteAllText(outputFile, test.ToTestClass());
			}
		}

		private static void ParseArguments(string[] args, out string fileName, out string outputPath)
		{
			fileName = string.Empty;
			outputPath = string.Empty;
			foreach (var arg in args)
			{
				if (arg.StartsWith("/input:"))
				{
					fileName = arg.Substring(7);
				}
				if (arg.StartsWith("/output:"))
				{
					outputPath = arg.Substring(8);
				}
			}
		}
	}
}

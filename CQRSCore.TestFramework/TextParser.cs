using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQRSCore.TestFramework
{
	public class TextParser
	{
		private List<TestFile> _tests;
		public void Parse(List<string> specText)
		{
			_tests = new List<TestFile>();
			if (specText.Count == 0) return;
			if (!specText.First().ToLower().StartsWith("subject:"))
				throw new ApplicationException("First line should hold Subject. (Start with Subject:)");
			string subject = specText.First().Substring(8).Trim();
		
			var test = new TestFile(subject);
      foreach (var line in specText.Skip(1))
			{
				var lowerline = line.ToLower();

				if (lowerline.StartsWith("given"))
				{
					test = new TestFile(subject);
					test.Given = lowerline.Replace("given", "").Trim().Replace(" ", "_");
					_tests.Add(test);
				}
				if (lowerline.StartsWith("when"))
				{
					test.When = lowerline.Replace("when", "").Trim().Replace(" ", "_");
				}
				if (lowerline.StartsWith("expect"))
				{
					test.Expects.Add(lowerline.Replace("expect", "").Trim().Replace(" ", "_"));
				}
			}
		}

		public List<TestFile> Tests { get { return _tests; } }
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CQRSCore.TestFramework
{
	public class TestFile
	{
		string _subject;
		public TestFile(string subject)
		{
			_subject = subject;
			Expects = new List<string>();
		}
		public string Subject { get { return _subject; } }

		public string Given { get; set; }

		public string When { get; set; }

		public List<string> Expects { get; private set; }

		public string ToTestClass()
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine("using System;")
				.AppendLine("using System.Text;")
				.AppendLine("using System.Collections.Generic;")
				.AppendLine("using System.Linq;")
				.AppendLine("using Microsoft.VisualStudio.TestTools.UnitTesting;")
				.AppendLine("using CQRSCore;")
				.AppendLine("using CQRSCore.Testing;")
				//Need to add namespace of referenced lib
				.AppendLine()
				.AppendFormat("namespace {0}.Tests", Subject).AppendLine() //should not be subject... needs to be namespace of test project
				.AppendLine("{")
				.AppendLine("\t[TestClass]")
				.AppendFormat("\tpublic class {0}_{1} : AggregateRootTest<{2}>", Given, When, Subject).AppendLine()
				.AppendLine("\t{")
				.AppendLine("\t\tpublic override IEnumerable<Event> Given()")
				.AppendLine("\t\t{")
				.AppendLine("\t\t\treturn new List<Event>();")
				.AppendLine("\t\t}")
				.AppendLine()
				.AppendLine("\t\tpublic void When()")
				.AppendLine("\t\t{")
				.AppendLine("\t\t}")
				.AppendLine();
			foreach (var expect in Expects)
			{
				sb.AppendLine("\t\t[TestMethod]")
					.AppendFormat("\t\tpublic void {0}()", expect).AppendLine()
					.AppendLine("\t\t{")
					.AppendLine("\t\t}")
					.AppendLine();
			}

			sb.AppendLine("\t}");
			sb.AppendLine("}");

			return sb.ToString();
		}
	}
}

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CQRSCore.TestFramework.Tests
{
	/// <summary>
	/// Summary description for UnitTest1
	/// </summary>
	[TestClass]
	public class TextParserTests
	{

		[TestMethod]
		public void Does_NotGeneratTestFile_When_NoLines()
		{
			//Arragne
			TextParser p = new TextParser();
			//Act
			p.Parse(new List<string>());
			//Assert
			Assert.AreEqual(0, p.Tests.Count);

		}

		[TestMethod]
		[ExpectedException(typeof(ApplicationException))]
		public void Does_ThrowException_When_FirstLineDoesNotStartWithSubject()
		{
			//Arragne
			TextParser p = new TextParser();
			//Act
			p.Parse(new List<string>(){ "Given a state" });
		}
		[TestMethod]
		public void Does_NotGeneratTestFile_When_NoLinesAndSubject()
		{
			//Arragne
			TextParser p = new TextParser();
			//Act
			p.Parse(new List<string>() { "subject: subject"});
			//Assert
			Assert.AreEqual(0, p.Tests.Count);

		}

		[TestMethod]
		public void Does_GeneratTestFile_When_LineStartsWithGiven()
		{
			//Arragne
			TextParser p = new TextParser();
			List<string> text = new List<string>() { "subject: subject" };
			text.Add("Given a state");
			//Act
			p.Parse(text);
			//Assert
			Assert.AreEqual(1, p.Tests.Count);
		}

		[TestMethod]
		public void Does_GiveTestFileGivenAState_When_GivenLineEndsWithAState()
		{
			//Arragne
			TextParser p = new TextParser();
			List<string> text = new List<string>() { "subject: subject" };
			text.Add("Given a state");
			//Act
			p.Parse(text);
			//Assert
			Assert.AreEqual("a_state", p.Tests[0].Given);
		}

		[TestMethod]
		public void Does_GiveTestFileWhenAnAction_When_WHenLineEndsWithAnAction()
		{
			//Arragne
			TextParser p = new TextParser();
			List<string> text = new List<string>() { "subject: subject" };
			text.Add("Given a state");
			text.Add("When an action");
			//Act
			p.Parse(text);
			//Assert
			Assert.AreEqual("an_action", p.Tests[0].When);
		}
		[TestMethod]
		public void Does_GiveTestFileExpectAState_When_ExpectLineEndsWithAState()
		{
			//Arragne
			TextParser p = new TextParser();
			List<string> text = new List<string>() { "subject: subject" };
			text.Add("Given a state");
			text.Add("When an action");
			text.Add("Expect a state");
			text.Add("Expect an action");

			//Act
			p.Parse(text);
			//Assert
			Assert.AreEqual("a_state", p.Tests[0].Expects[0]);
		}
		[TestMethod]
		public void Does_GiveTestFileMoreExpects_When_MoreLinesBeginWithExpect()
		{
			//Arragne
			TextParser p = new TextParser();
			List<string> text = new List<string>() { "subject: subject" };
			text.Add("Given a state");
			text.Add("When an action");
			text.Add("Expect a state");
			text.Add("Expect an action");

			//Act
			p.Parse(text);
			//Assert
			Assert.AreEqual(2, p.Tests[0].Expects.Count);
		}



		[TestMethod]
		public void Does_Create2TestFiles_When_TextContainsMoreLinesStartingWithGiven()
		{
			//Arragne
			TextParser p = new TextParser();
			List<string> text = new List<string>() { "subject: subject" };
			text.Add("Given a state");
			text.Add("When an action");
			text.Add("Expect a state");
			text.Add("Expect an action");
			text.Add("Given a state2");
			text.Add("When an action");
			text.Add("Expect a state");
			text.Add("Expect an action");
			//Act
			p.Parse(text);
			//Assert
			Assert.AreEqual(2, p.Tests.Count);
		}

		[TestMethod]
		public void Does_IgnoreEmpty_When_TextContainsEmptyLines()
		{
			//Arragne
			TextParser p = new TextParser();
			List<string> text = new List<string>() { "subject: subject" };
			text.Add("Given a state");
			text.Add("When an action");
			text.Add("Expect a state");
			text.Add("");
			text.Add("Given a state2");
			text.Add("When an action");
			text.Add("Expect a state");
			text.Add("Expect an action");
			//Act
			p.Parse(text);
			//Assert
			Assert.AreEqual(2, p.Tests.Count);
		}



		[TestMethod]
		public void Does_GiveFirstTestOnly1Expect_When_ItHasOnlyOneExpect()
		{
			//Arragne
			TextParser p = new TextParser();
			List<string> text = new List<string>() { "subject: subject" };
			text.Add("Given a state");
			text.Add("When an action");
			text.Add("Expect a state");
			text.Add("");
			text.Add("Given a state2");
			text.Add("When an action");
			text.Add("Expect a state");
			text.Add("Expect an action");
			//Act
			p.Parse(text);
			//Assert
			Assert.AreEqual(1, p.Tests.First().Expects.Count);
		}
	}
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using DayZObfuscatorModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace DayZObfuscatorModel.Tests
{
	[TestClass()]
	public class StringExtensions_Tests
	{
		[TestMethod()]
		public void Escape_Quotes_Test()
		{
			string sample = "\"Hello, world!\"";
			Assert.AreEqual( @"\""Hello, world!\""", sample.Escape() );
		}

		[TestMethod()]
		public void Unescape_Quotes_Test()
		{
			string sample = "\"Hello, world!\"";
			Assert.AreEqual(sample, sample.Escape().Unescape() );
		}

		[TestMethod()]
		public void Escape_Backslash_Test()
		{
			string sample = @"Hello, world\friend!";
			Assert.AreEqual( @"Hello, world\\friend!", sample.Escape() );
		}

		[TestMethod()]
		public void Unescape_Backslash_Test()
		{
			string sample = @"Hello, world\friend!";
			Assert.AreEqual(sample, sample.Escape().Unescape() );
		}

		[TestMethod()]
		public void Escape_Alert_Test()
		{
			string sample = "\aHello, world!";
			Assert.AreEqual( @"\aHello, world!", sample.Escape() );
		}

		[TestMethod()]
		public void Unescape_Alert_Test()
		{
			string sample = "\aHello, world!";
			Assert.AreEqual(sample, sample.Escape().Unescape() );
		}

		[TestMethod()]
		public void Escape_Backspace_Test()
		{
			string sample = "\bHello, world!";
			Assert.AreEqual( @"\bHello, world!", sample.Escape() );
		}

		[TestMethod()]
		public void Unescape_Backspace_Test()
		{
			string sample = "\bHello, world!";
			Assert.AreEqual(sample, sample.Escape().Unescape() );
		}

		[TestMethod()]
		public void Escape_FormFeed_Test()
		{
			string sample = "\fHello, world!";
			Assert.AreEqual( @"\fHello, world!", sample.Escape() );
		}

		[TestMethod()]
		public void Unescape_FormFeed_Test()
		{
			string sample = "\fHello, world!";
			Assert.AreEqual(sample, sample.Escape().Unescape() );
		}

		[TestMethod()]
		public void Escape_NewLine_Test()
		{
			string sample = "\nHello, world!";
			Assert.AreEqual( @"\nHello, world!", sample.Escape() );
		}

		[TestMethod()]
		public void Unescape_NewLine_Test()
		{
			string sample = "\nHello, world!";
			Assert.AreEqual(sample, sample.Escape().Unescape() );
		}

		[TestMethod()]
		public void Escape_CarriageReturn_Test()
		{
			string sample = "\rHello, world!";
			Assert.AreEqual( @"\rHello, world!", sample.Escape() );
		}

		[TestMethod()]
		public void Unescape_CarriageReturn_Test()
		{
			string sample = "\rHello, world!";
			Assert.AreEqual(sample, sample.Escape().Unescape() );
		}

		[TestMethod()]
		public void Escape_HorizontalTab_Test()
		{
			string sample = "\tHello, world!";
			Assert.AreEqual( @"\tHello, world!", sample.Escape() );
		}

		[TestMethod()]
		public void Unescape_HorizontalTab_Test()
		{
			string sample = "\tHello, world!";
			Assert.AreEqual(sample, sample.Escape().Unescape() );
		}

		[TestMethod()]
		public void Escape_VerticalTab_Test()
		{
			string sample = "\vHello, world!";
			Assert.AreEqual( @"\vHello, world!", sample.Escape() );
		}

		[TestMethod()]
		public void Unescape_VerticalTab_Test()
		{
			string sample = "\vHello, world!";
			Assert.AreEqual(sample, sample.Escape().Unescape() );
		}

		[TestMethod()]
		public void Escape_Complex_Test()
		{
			string sample = "\"\aHello,\n\r\t\vworld\\friend!\b\f\"";
			Assert.AreEqual( @"\""\aHello,\n\r\t\vworld\\friend!\b\f\""", sample.Escape() );
		}

		[TestMethod()]
		public void Unescape_Complex_Test()
		{
			string sample = "\"\aHello,\n\r\t\vworld\\friend!\b\f\"";
			Assert.AreEqual(sample, sample.Escape().Unescape() );
		}
	}
}
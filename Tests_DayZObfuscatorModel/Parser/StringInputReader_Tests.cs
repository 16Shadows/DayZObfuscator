using Microsoft.VisualStudio.TestTools.UnitTesting;
using DayZObfuscatorModel.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZObfuscatorModel.Parser.Tests
{
	[TestClass()]
	public class StringInputReader_Tests
	{
		[TestMethod()]
		public void Consume_Test()
		{
			string data = "Hello, world!";
			StringInputReader reader = data;
				
			foreach (char c in data)
				Assert.AreEqual(c, reader.Consume());

			Assert.AreEqual('\0', reader.Consume());
		}

		[TestMethod()]
		public void ConsumeN_Test()
		{
			string data = "Hello, world!";
			StringInputReader reader = data;

			Assert.IsTrue(data[0..5].SequenceEqual(reader.Consume(5)));
			Assert.IsTrue(data[5..7].SequenceEqual(reader.Consume(2)));
			Assert.IsTrue(data[7..12].SequenceEqual(reader.Consume(5)));
			Assert.AreEqual(data[12], reader.Consume(1).First());
			Assert.AreEqual('\0', reader.Consume(1).First());
		}

		[TestMethod()]
		public void ConsumeNInvalid_Test()
		{
			string data = "Hello, world!";
			StringInputReader reader = data;

			Assert.ThrowsException<ArgumentOutOfRangeException>(() => reader.Consume(-2));
		}

		[TestMethod()]
		public void Peek_Test()
		{
			string data = "Hello, world!";
			StringInputReader reader = data;

			Assert.AreEqual(data[0], reader.Peek());
			Assert.AreEqual(data[0], reader.Peek());
			reader.Consume(3);
			Assert.AreEqual(data[3], reader.Peek());
			Assert.AreEqual(data[3], reader.Peek());
		}

		[TestMethod()]
		public void PeekBeyondEnd_Test()
		{
			string data = "Hello, world!";
			StringInputReader reader = data;
			
			Assert.IsTrue( data.Concat(Enumerable.Repeat('\0', 1)).SequenceEqual(reader.Peek(data.Length + 1)) );
		}

		[TestMethod()]
		public void PeekNBeyondEnd_Test()
		{
			string data = "Hello, world!";
			StringInputReader reader = data;
				
			reader.Consume(data.Length);

			Assert.AreEqual('\0', reader.Peek());
		}

		[TestMethod()]
		public void PeekN_Test()
		{
			string data = "Hello, world!";
			StringInputReader reader = data;

			Assert.IsTrue(data[0..3].SequenceEqual(reader.Peek(3)));
			Assert.IsTrue(data[0..3].SequenceEqual(reader.Peek(3)));
			reader.Consume(3);
			Assert.IsTrue(data[3..5].SequenceEqual(reader.Peek(2)));
			Assert.IsTrue(data[3..5].SequenceEqual(reader.Peek(2)));
		}

		[TestMethod()]
		public void PeekNInvalid_Test()
		{
			string data = "Hello, world!";
			StringInputReader reader = data;

			Assert.ThrowsException<ArgumentOutOfRangeException>(() => reader.Peek(-2));
		}
	}
}
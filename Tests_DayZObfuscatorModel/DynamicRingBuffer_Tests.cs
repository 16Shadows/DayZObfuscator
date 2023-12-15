using Microsoft.VisualStudio.TestTools.UnitTesting;
using DayZObfuscatorModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Buffers;

namespace DayZObfuscatorModel.Tests
{
	[TestClass()]
	public class DynamicRingBuffer_Tests
	{
		[TestMethod()]
		public void DefaultConstructor_Test()
		{
			DynamicRingBuffer<int> buffer = new DynamicRingBuffer<int>();

			Assert.AreEqual(0, buffer.Count);
			Assert.ThrowsException<InvalidOperationException>(() => buffer[0]);
		}

		[TestMethod()]
		public void CapacityConstructor_Test()
		{
			DynamicRingBuffer<int> buffer = new DynamicRingBuffer<int>(15);

			Assert.AreEqual(15, buffer.Capacity);
			Assert.AreEqual(0, buffer.Count);
			Assert.ThrowsException<InvalidOperationException>(() => buffer[0]);
		}

		[TestMethod()]
		public void InvalidCapacityConstructor_Test()
		{
			Assert.ThrowsException<ArgumentOutOfRangeException>(() =>
			{
				DynamicRingBuffer<int> buffer = new DynamicRingBuffer<int>(-15);
			});
		}

		[TestMethod()]
		public void EnumerableConstructor_Test()
		{
			int[] arr = new int[3] { 1, 2, 3 };

			DynamicRingBuffer<int> buffer = new DynamicRingBuffer<int>(arr);

			Assert.AreEqual(3, buffer.Capacity);
			Assert.AreEqual(3, buffer.Count);
			for (int i = 0; i < arr.Length; i++)
				Assert.AreEqual(arr[i], buffer[i]);
		}

		[TestMethod()]
		public void ParamsArrayConstructor_Test()
		{
			int[] arr = new int[] { 1, 2, 3 };
			DynamicRingBuffer<int> buffer = new DynamicRingBuffer<int>(1, 2, 3);

			Assert.AreEqual(3, buffer.Capacity);
			Assert.AreEqual(3, buffer.Count);
			for (int i = 0; i < arr.Length; i++)
				Assert.AreEqual(arr[i], buffer[i]);
		}

		[TestMethod()]
		public void WriteOverCapacity_Test()
		{
			DynamicRingBuffer<int> buffer = new DynamicRingBuffer<int>( 2, 3, 4, 5 );

			Assert.AreEqual(4, buffer.Capacity);
			Assert.AreEqual(4, buffer.Count);
			Assert.AreEqual(2, buffer[0]);
			Assert.AreEqual(3, buffer[1]);
			Assert.AreEqual(4, buffer[2]);
			Assert.AreEqual(5, buffer[3]);

			buffer.Write(16);

			Assert.AreEqual(4, buffer.Capacity);
			Assert.AreEqual(4, buffer.Count);
			Assert.AreEqual(3, buffer[0]);
			Assert.AreEqual(4, buffer[1]);
			Assert.AreEqual(5, buffer[2]);
			Assert.AreEqual(16, buffer[3]);
		}

		[TestMethod()]
		public void WriteUnderCapacity_Test()
		{
			DynamicRingBuffer<int> buffer = new DynamicRingBuffer<int>( 4 );

			buffer.Write(2);
			buffer.Write(3);
			buffer.Write(4);

			Assert.AreEqual(4, buffer.Capacity);
			Assert.AreEqual(3, buffer.Count);
			Assert.AreEqual(2, buffer[0]);
			Assert.AreEqual(3, buffer[1]);
			Assert.AreEqual(4, buffer[2]);

			buffer.Write(16);

			Assert.AreEqual(4, buffer.Capacity);
			Assert.AreEqual(4, buffer.Count);
			Assert.AreEqual(2, buffer[0]);
			Assert.AreEqual(3, buffer[1]);
			Assert.AreEqual(4, buffer[2]);
			Assert.AreEqual(16, buffer[3]);
		}

		[TestMethod()]
		public void WriteRangeEnumerable_Test()
		{
			DynamicRingBuffer<int> buffer = new DynamicRingBuffer<int>(2, 3, 4, 5);

			Assert.AreEqual(4, buffer.Capacity);
			Assert.AreEqual(4, buffer.Count);
			Assert.AreEqual(2, buffer[0]);
			Assert.AreEqual(3, buffer[1]);
			Assert.AreEqual(4, buffer[2]);
			Assert.AreEqual(5, buffer[3]);

			int[] arr = new int[] { 16, 23 };
			buffer.WriteRange(arr);

			Assert.AreEqual(4, buffer.Capacity);
			Assert.AreEqual(4, buffer.Count);
			Assert.AreEqual(4, buffer[0]);
			Assert.AreEqual(5, buffer[1]);
			Assert.AreEqual(16, buffer[2]);
			Assert.AreEqual(23, buffer[3]);	
		}

		[TestMethod()]
		public void WriteRangeParams_Test()
		{
			DynamicRingBuffer<int> buffer = new DynamicRingBuffer<int>(2, 3, 4, 5);

			Assert.AreEqual(4, buffer.Capacity);
			Assert.AreEqual(4, buffer.Count);
			Assert.AreEqual(2, buffer[0]);
			Assert.AreEqual(3, buffer[1]);
			Assert.AreEqual(4, buffer[2]);
			Assert.AreEqual(5, buffer[3]);

			buffer.WriteRange(16, 23);

			Assert.AreEqual(4, buffer.Capacity);
			Assert.AreEqual(4, buffer.Count);
			Assert.AreEqual(4, buffer[0]);
			Assert.AreEqual(5, buffer[1]);
			Assert.AreEqual(16, buffer[2]);
			Assert.AreEqual(23, buffer[3]);
		}

		[TestMethod()]
		public void AddUnderCapacity_Test()
		{
			DynamicRingBuffer<int> buffer = new DynamicRingBuffer<int>(6);

			buffer.WriteRange(2, 3, 4, 5);

			Assert.AreEqual(6, buffer.Capacity);
			Assert.AreEqual(4, buffer.Count);
			Assert.AreEqual(2, buffer[0]);
			Assert.AreEqual(3, buffer[1]);
			Assert.AreEqual(4, buffer[2]);
			Assert.AreEqual(5, buffer[3]);

			buffer.Add(17);

			Assert.AreEqual(6, buffer.Capacity);
			Assert.AreEqual(5, buffer.Count);
			Assert.AreEqual(2, buffer[0]);
			Assert.AreEqual(3, buffer[1]);
			Assert.AreEqual(4, buffer[2]);
			Assert.AreEqual(5, buffer[3]);
			Assert.AreEqual(17, buffer[4]);
		}

		[TestMethod()]
		public void AddOverCapacity_Test()
		{
			DynamicRingBuffer<int> buffer = new DynamicRingBuffer<int>(4);

			buffer.WriteRange(2, 3, 4, 5);

			Assert.AreEqual(4, buffer.Capacity);
			Assert.AreEqual(4, buffer.Count);
			Assert.AreEqual(2, buffer[0]);
			Assert.AreEqual(3, buffer[1]);
			Assert.AreEqual(4, buffer[2]);
			Assert.AreEqual(5, buffer[3]);

			buffer.Add(17);

			Assert.AreEqual(5, buffer.Capacity);
			Assert.AreEqual(5, buffer.Count);
			Assert.AreEqual(2, buffer[0]);
			Assert.AreEqual(3, buffer[1]);
			Assert.AreEqual(4, buffer[2]);
			Assert.AreEqual(5, buffer[3]);
			Assert.AreEqual(17, buffer[4]);
		}

		[TestMethod()]
		public void AddOverCapacityLarge_Test()
		{
			DynamicRingBuffer<int> buffer = new DynamicRingBuffer<int>(4);

			buffer.WriteRange(2, 3, 4, 5);

			Assert.AreEqual(4, buffer.Capacity);
			Assert.AreEqual(4, buffer.Count);
			Assert.AreEqual(2, buffer[0]);
			Assert.AreEqual(3, buffer[1]);
			Assert.AreEqual(4, buffer[2]);
			Assert.AreEqual(5, buffer[3]);

			for (int i = 0; i < 20; i++)
				buffer.Add(17);

			Assert.AreEqual(24, buffer.Capacity);
			Assert.AreEqual(24, buffer.Count);
			Assert.AreEqual(2, buffer[0]);
			Assert.AreEqual(3, buffer[1]);
			Assert.AreEqual(4, buffer[2]);
			Assert.AreEqual(5, buffer[3]);
			for (int i = 0; i < 20; i++)
				Assert.AreEqual(17, buffer[4+i]);
		}

		[TestMethod()]
		public void GetEnumeratorT_Test()
		{
			int[] arr = new int[5] { 4, 4, 3, 2, 1 };
			DynamicRingBuffer<int> buffer = new DynamicRingBuffer<int>(arr);

			Assert.AreEqual(5, buffer.Capacity);
			Assert.AreEqual(5, buffer.Count);
			int index = 0;
			foreach (int item in buffer)
				Assert.AreEqual(arr[index++], item);
		}

		[TestMethod()]
		public void GetEnumerator_Test()
		{
			object[] arr = new object[5] { 4, 4, 3, 2, 1 };
			DynamicRingBuffer<int> buffer = new DynamicRingBuffer<int>(arr.Select(x => (int)x));

			Assert.AreEqual(5, buffer.Capacity);
			Assert.AreEqual(5, buffer.Count);
			
			int index = 0;
			foreach (object item in (System.Collections.IEnumerable)buffer)
				Assert.AreEqual(arr[index++], item);
		}

		[TestMethod()]
		public void AddRangeEnumerable_Test()
		{
			int[] arr = new int[] { 4, 2, 3 };
			DynamicRingBuffer<int> buffer = new(3);

			Assert.AreEqual(3, buffer.Capacity);
			Assert.AreEqual(0, buffer.Count);

			buffer.AddRange(arr);

			Assert.AreEqual(3, buffer.Capacity);
			Assert.AreEqual(3, buffer.Count);
			for (int i = 0; i < arr.Length; i++)
				Assert.AreEqual(arr[i], buffer[i]);
		}

		[TestMethod()]
		public void AddRangeParams_Test()
		{
			int[] arr = new int[] { 4, 2, 3 };
			DynamicRingBuffer<int> buffer = new(3);

			Assert.AreEqual(3, buffer.Capacity);
			Assert.AreEqual(0, buffer.Count);

			buffer.AddRange(4, 2, 3);

			Assert.AreEqual(3, buffer.Capacity);
			Assert.AreEqual(3, buffer.Count);
			for (int i = 0; i < arr.Length; i++)
				Assert.AreEqual(arr[i], buffer[i]);
		}

		[TestMethod()]
		public void Pop_Test()
		{
			int[] arr = new int[] { 4, 2, 3 };
			DynamicRingBuffer<int> buffer = new(arr);

			Assert.AreEqual(3, buffer.Capacity);
			Assert.AreEqual(3, buffer.Count);

			for (int i = 0; i < arr.Length; i++)
				Assert.AreEqual(arr[i], buffer[i]);

			Assert.AreEqual(4, buffer.Pop());

			Assert.AreEqual(3, buffer.Capacity);
			Assert.AreEqual(2, buffer.Count);
			for (int i = 0; i < arr.Length - 1; i++)
				Assert.AreEqual(arr[i+1], buffer[i]);

			Assert.AreEqual(2, buffer.Pop());
			Assert.AreEqual(3, buffer.Pop());

			buffer.Add(5);

			Assert.AreEqual(5, buffer.Pop());
		}

		[TestMethod()]
		public void PopAdd_Test()
		{
			int[] arr = new int[] { 4, 2, 3 };
			int[] arr2 = new int[] { 2, 3, 5 };
			DynamicRingBuffer<int> buffer = new(arr);

			Assert.AreEqual(3, buffer.Capacity);
			Assert.AreEqual(3, buffer.Count);

			for (int i = 0; i < arr.Length; i++)
				Assert.AreEqual(arr[i], buffer[i]);

			Assert.AreEqual(4, buffer.Pop());
			buffer.Add(5);

			Assert.AreEqual(3, buffer.Capacity);
			Assert.AreEqual(3, buffer.Count);
			for (int i = 0; i < arr2.Length; i++)
				Assert.AreEqual(arr2[i], buffer[i]);
		}

		[TestMethod()]
		public void PopN_Test()
		{
			int[] arr = new int[] { 4, 2, 3 };
			DynamicRingBuffer<int> buffer = new(arr);

			Assert.AreEqual(3, buffer.Capacity);
			Assert.AreEqual(3, buffer.Count);

			for (int i = 0; i < arr.Length; i++)
				Assert.AreEqual(arr[i], buffer[i]);

			int index = 0;
			foreach (int item in buffer.Pop(3))
				Assert.AreEqual(arr[index++], item);

			Assert.AreEqual(3, buffer.Capacity);
			Assert.AreEqual(0, buffer.Count);
		}

		[TestMethod()]
		public void Clear_Test()
		{
			int[] arr = new int[] { 4, 2, 3 };
			DynamicRingBuffer<int> buffer = new(arr);

			Assert.AreEqual(3, buffer.Capacity);
			Assert.AreEqual(3, buffer.Count);

			for (int i = 0; i < arr.Length; i++)
				Assert.AreEqual(arr[i], buffer[i]);

			buffer.Clear();

			Assert.AreEqual(3, buffer.Capacity);
			Assert.AreEqual(0, buffer.Count);

			buffer.AddRange(arr);

			Assert.AreEqual(3, buffer.Capacity);
			Assert.AreEqual(3, buffer.Count);

			for (int i = 0; i < arr.Length; i++)
				Assert.AreEqual(arr[i], buffer[i]);
		}

		[TestMethod()]
		public void Contains_Test()
		{
			int[] arr = new int[] { 4, 2, 3 };
			DynamicRingBuffer<int> buffer = new(arr);

			Assert.AreEqual(3, buffer.Capacity);
			Assert.AreEqual(3, buffer.Count);

			for (int i = 0; i < arr.Length; i++)
				Assert.AreEqual(arr[i], buffer[i]);

			Assert.IsTrue(buffer.Contains(2));
			Assert.IsFalse(buffer.Contains(15));

			buffer[1] = 0;

			Assert.IsFalse(buffer.Contains(2));
		}

		[TestMethod()]
		public void IndexerGet_Test()
		{
			int[] arr = new int[] { 4, 2, 3 };
			DynamicRingBuffer<int> buffer = new(arr);

			Assert.AreEqual(2, arr[1]);
		}

		[TestMethod()]
		public void IndexerInvalidGet_Test()
		{
			DynamicRingBuffer<int> buffer = new(5);

			Assert.ThrowsException<InvalidOperationException>(() => buffer[0]);
		}

		[TestMethod()]
		public void IndexerSet_Test()
		{
			int[] arr = new int[] { 4, 2, 3 };
			DynamicRingBuffer<int> buffer = new(arr);

			Assert.AreEqual(2, arr[1]);
			arr[1] = 5;
			Assert.AreEqual(5, arr[1]);
		}

		[TestMethod()]
		public void IndexerInvalidSet_Test()
		{
			DynamicRingBuffer<int> buffer = new(5);

			Assert.ThrowsException<InvalidOperationException>(() => buffer[0] = 5);
		}
	}
}
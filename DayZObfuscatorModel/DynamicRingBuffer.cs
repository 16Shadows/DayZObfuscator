using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace DayZObfuscatorModel
{
	/// <summary>
	/// A ring buffer which supports dynamic expansion of its size
	/// </summary>
	/// <typeparam name="T">The type to store in ring buffer</typeparam>
	public class DynamicRingBuffer<T> : IEnumerable<T>
	{
		private T[] _Buffer;
		private int _ReadIndex;
		private int _WriteIndex;
		private int _BufferStartIndex;
		private int _Count;
		private int _Capacity;

		public int Capacity => _Capacity;
		public int Count => _Count;
		public bool IsReadOnly => false;
		public bool IsSynchronized => false;
		public object SyncRoot { get; } = new object();

		public DynamicRingBuffer(int capacity)
		{
			if (capacity < 1)
				throw new ArgumentOutOfRangeException(nameof(capacity), $"{nameof(capacity)} should be greater than 0.");

			_Buffer = new T[capacity];
			_ReadIndex = 0;
			_WriteIndex = 0;
			_Capacity = capacity;
			_Count = 0;
			_BufferStartIndex = 0;
		}

		public DynamicRingBuffer(IEnumerable<T> items) : this(1)
		{
			AddRange(items);
		}

		public DynamicRingBuffer(params T[] items) : this((IEnumerable<T>)items) {}

		/// <summary>
		/// Reads or writes stringbuffer at specified index starting from index of the oldest value
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public T this[int index]
		{
			get
			{
				if (Count == 0)
					throw new InvalidOperationException($"{nameof(DynamicRingBuffer<T>)} is empty.");

				return _Buffer[AdjustReadIndex(index)];
			}
			set
			{
				if (Count == 0)
					throw new InvalidOperationException($"{nameof(DynamicRingBuffer<T>)} is empty.");

				_Buffer[AdjustReadIndex(index)] = value;
			}
		}

		/// <summary>
		/// Writes to the ring buffer. If it is full, overwrites the oldest value.
		/// </summary>
		/// <param name="value">The value to write.</param>
		public void Write(T value)
		{
			if (Count < Capacity)
			{
				_Count++;
				_Buffer[AdjustBufferIndex(_WriteIndex++)] = value;
			}
			else
			{
				_Buffer[ AdjustWriteIndex(0) ] = value;
				_WriteIndex = AdjustWriteIndex(1);
				_ReadIndex = AdjustReadIndex(1);
			}
		}

		/// <summary>
		/// Writes a sequence of elements to the ring buffer. If it is full or becomes, overwrites values starting from the oldest.
		/// </summary>
		/// <param name="values"></param>
		public void WriteRange(IEnumerable<T> values)
		{
			foreach (T value in values)
				Write(value);
		}

		public void WriteRange(params T[] values) => WriteRange((IEnumerable<T>)values);

		/// <summary>
		/// Pushes a value into the ring buffer. If it is full, expands the ring buffer.
		/// </summary>
		/// <param name="value"></param>
		public void Add(T value)
		{
			if (Count >= Capacity)
				ExpandBuffer();

			Write(value);
		}

		public void AddRange(IEnumerable<T> values)
		{
			foreach (T value in values)
				Add(value);
		}

		public void AddRange(params T[] values) => AddRange((IEnumerable<T>)values);

		public T Pop()
		{
			T item = this[0];
			_ReadIndex = AdjustReadIndex(1);
			_Count--;
			return item;
		}

		public IEnumerable<T> Pop(int count)
		{
			T[] result = new T[count];
			int index = 0;
			while (count-- > 0)
				result[index++] = Pop();

			return result;
		}

		public IEnumerator<T> GetEnumerator()
		{
			for (int i = 0; i < Count; i++)
				yield return this[i];
			yield break;
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		private void ExpandBuffer()
		{
			_Capacity++;
			if (_Capacity <= _Buffer.Length)
				return;

			double expansionCoef = _Buffer.Length < 256 ? 2 :
									_Buffer.Length < 1024 ? 1.2 : 1.05;

			T[] newBuffer = new T[(int)Math.Ceiling(_Buffer.Length * expansionCoef)];
			int index = 0;
			foreach (T value in this)
				newBuffer[index++] = value;

			_Buffer = newBuffer;
			_BufferStartIndex = 0;
			_ReadIndex = 0;
			_WriteIndex = AdjustWriteIndex(index);
		}

		private int AdjustReadIndex(int index) => AdjustBufferIndex(_ReadIndex + index % _Count);
		private int AdjustWriteIndex(int index) => AdjustBufferIndex(_WriteIndex + index % _Count);
		private int AdjustBufferIndex(int index) => (_BufferStartIndex + index % _Capacity) % _Buffer.Length;

		public void Clear()
		{
			_BufferStartIndex = 0;
			_WriteIndex = 0;
			_ReadIndex = 0;
			_Count = 0;
		}

		public bool Contains(T item)
		{
			foreach (T value in this)
				if (EqualityComparer<T>.Default.Equals(value, item))
					return true;

			return false;
		}
	}
}

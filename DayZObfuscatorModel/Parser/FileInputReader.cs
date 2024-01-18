using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZObfuscatorModel.Parser
{
	public class FileInputReader : IInputReader
	{
		protected readonly DynamicRingBuffer<char> _InputBuffer;
		protected readonly StreamReader _InputStream;

		public FileInputReader(string path)
		{
			ArgumentNullException.ThrowIfNull(path);

			if (!Path.Exists(path) || Path.GetFileName(path) == null)
				throw new ArgumentException($"{nameof(path)} should be a path to an existing file.");

			_InputBuffer = new DynamicRingBuffer<char>(50);
			_InputStream = File.OpenText(path);
		}

		public char Consume()
		{
			if ( _InputBuffer.Count < 1 && Preread(1) < 1)
				return '\0';

			return _InputBuffer.Pop();
		}

		public string Consume(int count)
		{
			if ( _InputBuffer.Count < count)
			{
				int toRead = count - _InputBuffer.Count;
				int read = Preread(toRead);

				if (read < toRead)
					return string.Join("", _InputBuffer.Pop(_InputBuffer.Count).Concat(Enumerable.Repeat('\0', toRead - read)));
			}

			return string.Join("", _InputBuffer.Pop(count));
		}

		public char Peek()
		{
			if ( _InputBuffer.Count < 1 && Preread(1) < 1)
				return '\0';
			else
				return _InputBuffer[0];
		}

		public string Peek(int count)
		{
			if ( _InputBuffer.Count < count)
			{
				int toRead = count - _InputBuffer.Count;
				int read = Preread(toRead);

				if (read < toRead)
					return string.Join("", _InputBuffer.Concat(Enumerable.Repeat('\0', toRead - read)));
			}

			return string.Join("", _InputBuffer.Take(count));
		}

		protected int Preread(int count)
		{
			int totalRead = 0, lastRead;

			char[] buffer = new char[50];

			for (int i = 0; i < count / 50; i++)
			{
				lastRead = _InputStream.Read(buffer, 0, 50);

				totalRead += lastRead;
				
				if (lastRead > 0)
					_InputBuffer.AddRange(buffer);

				if (lastRead < 50)
					return totalRead;
			}

			count %= 50;
			if (count > 0)
			{
				lastRead = _InputStream.Read(buffer, 0, count);
				
				totalRead += lastRead;

				if (lastRead > 0)
					_InputBuffer.AddRange(buffer.Take(count));
			}
			
			return totalRead;
		}
	}
}

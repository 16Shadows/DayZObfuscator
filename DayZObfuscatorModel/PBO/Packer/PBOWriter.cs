using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DayZObfuscatorModel.PBO.Packer
{
	public class PBOWriter : BinaryWriter
	{
		public PBOWriter(Stream outStream) : base(outStream)
		{
		}

		override public void Write(string value)
		{
			base.Write(Encoding.UTF8.GetBytes(value));
			base.Write((byte)0);
		}

		public void Write(Stream stream)
		{
			const int chunkSize = 8*1024;

			long toWrite = stream.Length - stream.Position;

			//Write in chunks of 8KB
			byte[] buffer = new byte[chunkSize];

			int readBytes;
			for (; toWrite > 0; toWrite -= readBytes)
			{
				readBytes = stream.Read(buffer, 0, chunkSize);
				base.Write(buffer, 0, readBytes);
			}
		}
	}
}

﻿using System.Text;

namespace DayZObfuscatorModel.PBO.Packer
{
	public class PBOWriter : BinaryWriter
	{
		public PBOWriter(Stream outStream) : base(outStream)
		{
		}

		override public void Write(string value)
		{
			base.Write(Encoding.ASCII.GetBytes(value));
			base.Write((byte)0);
		}

		public void Write(Stream stream)
		{
			const int chunkSize = 8*1024;

			//Write in chunks of 8KB
			byte[] buffer = new byte[chunkSize];

			int readBytes;
			while((readBytes = stream.Read(buffer, 0, chunkSize)) > 0)
				base.Write(buffer, 0, readBytes);
		}
	}
}

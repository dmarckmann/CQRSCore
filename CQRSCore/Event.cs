using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace CQRSCore
{
	[Serializable]
	public class Event : Message
	{
		public int Version;

		public byte[] ToBinary()
		{
			using (var memoryStream = new MemoryStream())
			{
				new BinaryFormatter().Serialize(memoryStream, this);
				return memoryStream.ToArray();
			}
		}

		public static Event FromBinary(byte[] data)
		{
			using (var memoryStream = new MemoryStream(data))
			{
				return (Event)new BinaryFormatter().Deserialize(memoryStream);
			}
		}
	}

}
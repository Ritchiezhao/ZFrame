using System;
using System.IO;
using ProtoBuf;

namespace zf.core
{
	public static class ProtobufHelper
	{
		public static byte[] ToBytes(object message)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				Serializer.Serialize(ms, message);
				return ms.ToArray();
			}
		}

		public static T FromBytes<T>(byte[] bytes)
		{
			T t;
			using (MemoryStream ms = new MemoryStream(bytes, 0, bytes.Length))
			{
				t = Serializer.Deserialize<T>(ms);
			}
			return t;
		}

		public static T FromBytes<T>(byte[] bytes, int index, int length)
		{
			T t;
			using (MemoryStream ms = new MemoryStream(bytes, index, length))
			{
				t = Serializer.Deserialize<T>(ms);
			}
			return t;
		}

		public static object FromBytes(Type type, byte[] bytes)
		{
			object t;
			using (MemoryStream ms = new MemoryStream(bytes, 0, bytes.Length))
			{
				t = Serializer.NonGeneric.Deserialize(type, ms);
			}
			return t;
		}

		public static object FromBytes(Type type, byte[] bytes, int index, int length)
		{
			object t;
			using (MemoryStream ms = new MemoryStream(bytes, index, length))
			{
				t = Serializer.NonGeneric.Deserialize(type, ms);
			}
			return t;
		}
	}
}
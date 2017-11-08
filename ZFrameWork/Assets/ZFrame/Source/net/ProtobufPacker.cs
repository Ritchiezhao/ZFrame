using System;
using zf.core;

namespace zf.net
{
	public class ProtobufPacker : IMessagePacker
	{
		public byte[] SerializeToByteArray(object obj)
		{
			return ProtobufHelper.ToBytes(obj);
		}

		public object DeserializeFrom(Type type, byte[] bytes)
		{
			return ProtobufHelper.FromBytes(type, bytes);
		}

		public object DeserializeFrom(Type type, byte[] bytes, int index, int count)
		{
			return ProtobufHelper.FromBytes(type, bytes, index, count);
		}

		public T DeserializeFrom<T>(byte[] bytes)
		{
			return ProtobufHelper.FromBytes<T>(bytes);
		}

		public T DeserializeFrom<T>(byte[] bytes, int index, int count)
		{
			return ProtobufHelper.FromBytes<T>(bytes, index, count);
		}

        public string SerializeToText(object obj)
        {
            throw new NotImplementedException();
        }
    }
}

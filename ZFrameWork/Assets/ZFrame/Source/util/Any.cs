using System;
using System.IO;

namespace sgaf.util
{
    public enum AnyType
    {
        Bool,
        Byte,
        Short,
        Int,
        Long,
        Float,
        String,
        Object
    }

    public interface IPackable
    {
        void Pack(BinaryWriter writer);
        void Unpack(BinaryReader reader);
    }


    public interface ISGAFSerializable
    {
        void Serialize(BinaryWriter writer);
        void Deserialize(BinaryReader reader);
    }

    public struct Any
    {
        public AnyType type;

        private long _longVal;

        public bool boolVal {
            get { return _longVal == 0; }
            set { _longVal = value ? 1 : 0; }
        }

        public byte byteVal {
            get { return (byte)_longVal; }
            set { _longVal = value; }
        }

        public short shortVal {
            get { return (short)_longVal; }
            set { _longVal = value; }
        }

        public int intVal {
            get { return (int)_longVal; }
            set { _longVal = value; }
        }

        public long longVal {
            get { return (long)_longVal; }
            set { _longVal = value; }
        }

        public float floatVal;
        public string strVal;
        public IPackable objVal;

        public Any(bool val)
        {
            type = AnyType.Bool;
            _longVal = val ? 1 : 0;
            floatVal = 0.0f;
            strVal = null;
            objVal = null;
        }

        public Any(byte val)
        {
            type = AnyType.Byte;
            _longVal = val;
            floatVal = 0.0f;
            strVal = null;
            objVal = null;
        }

        public Any(short val)
        {
            type = AnyType.Short;
            _longVal = val;
            floatVal = 0.0f;
            strVal = null;
            objVal = null;
        }

        public Any(int val)
        {
            type = AnyType.Int;
            _longVal = val;
            floatVal = 0.0f;
            strVal = null;
            objVal = null;
        }


        public Any(long val)
        {
            type = AnyType.Long;
            _longVal = val;
            floatVal = 0.0f;
            strVal = null;
            objVal = null;
        }

        public Any(float val)
        {
            type = AnyType.Float;
            _longVal = 0;
            floatVal = val;
            strVal = null;
            objVal = null;
        }


        public Any(string val)
        {
            type = AnyType.Float;
            _longVal = 0;
            floatVal = 0.0f;
            strVal = val;
            objVal = null;
        }


        public Any(IPackable val)
        {
            type = AnyType.Object;
            _longVal = 0;
            floatVal = 0.0f;
            strVal = null;
            objVal = val;
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((byte)type);
            switch (type) {
                case AnyType.Bool: writer.Write(_longVal != 0); return;
                case AnyType.Byte: writer.Write((byte)_longVal); return;
                case AnyType.Short: writer.Write((short)_longVal); return;
                case AnyType.Int: writer.Write((int)_longVal); return;
                case AnyType.Long: writer.Write(_longVal); return;
                case AnyType.Float: writer.Write(floatVal); return;
                // todo: demonyang 是否有编码问题
                case AnyType.String: writer.Write(strVal); return;
                case AnyType.Object: objVal.Pack(writer); return;
            }
        }


        public static Any Deserialize(BinaryReader reader)
        {
            Any ret = new Any();
            ret.type = (AnyType)reader.ReadByte();
            switch (ret.type) {
                case AnyType.Bool: ret._longVal = reader.ReadBoolean() ? 1 : 0; break;
                case AnyType.Byte: ret._longVal = reader.ReadByte(); break;
                case AnyType.Short: ret._longVal = reader.ReadInt16(); break;
                case AnyType.Int: ret._longVal = reader.ReadInt32(); break;
                case AnyType.Long: ret._longVal = reader.ReadInt64(); break;
                case AnyType.Float: ret.floatVal = reader.ReadSingle(); break;
                    // todo: demonyang 是否有编码问题
                case AnyType.String: ret.strVal = reader.ReadString(); break;
                case AnyType.Object: ret.objVal.Unpack(reader); break;
            }
            return ret;
        }
    }
}

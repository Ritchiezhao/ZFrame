//#define DEBUG_INFO

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using LitJson;



namespace ZFEditor
{

    public class DataDebugInfo
    {
        public string ownerTID;
        public string fileName;

        public override string ToString()
        {
            return string.Format("\nAt : '{0}' :  '{1}'", fileName, ownerTID);
        }
    }

    public class MapKV
    {
        public GData key;
        public GData val;
    }

	// GClassType -> GObject
	// GStructType、Enum、Basic types -> GData
	public class GData
	{
        protected string mType;

        public GStructField fieldInfo;

		#region Values
		protected bool inst_bool;
        protected byte inst_byte;
        protected short inst_short;
        protected int inst_int;
		protected float inst_float;
		protected string inst_string;
		protected string inst_tid;
		protected string inst_enum;
		protected string inst_bitfield;
        protected List<GData> inst_array;
		protected IDictionary<string, GData> inst_object;
        protected List<MapKV> inst_map;
        #endregion

        public bool inheritParent = false;
        public bool isKVArray = false;

		// display info //
		public bool display_foldout = false;

        public DataDebugInfo debugInfo;

        public GData(string type)
        {
            mType = type;
        }

        public GData Clone()
        {
            GData ret = new GData(mType);

            ret.inst_bool = inst_bool;
            ret.inst_byte = inst_byte;
            ret.inst_short = inst_short;
            ret.inst_int = inst_int;
            ret.inst_float = inst_float;
            if (inst_string != null)
                ret.inst_string = string.Copy(inst_string);
            if (inst_tid != null)
                ret.inst_tid = string.Copy(inst_tid);
            if (inst_enum != null)
                ret.inst_enum = string.Copy(inst_enum);
            if (inst_bitfield != null)
                ret.inst_bitfield = string.Copy(inst_bitfield);

            if (inst_array != null) {
                ret.inst_array = new List<GData>();
                for (int i = 0; i < inst_array.Count; ++i)
                    ret.inst_array.Add(inst_array[i].Clone());
            }

            if (inst_object != null) {
                ret.inst_object = new Dictionary<string, GData>();
                var e = inst_object.GetEnumerator();
                while (e.MoveNext()) {
                    ret.inst_object.Add(e.Current.Key, e.Current.Value.Clone());
                }
            }

            if (inst_map != null) {
                ret.inst_map = new List<MapKV>();
                for (int i = 0; i < inst_map.Count; ++i) {
                    ret.inst_map.Add(new MapKV() {key = inst_map[i].key.Clone(), val = inst_map[i].val.Clone()});
                }
            }

            return ret;
        }


        public virtual bool Build()
        {
            // sub build
            if (inst_array != null) {
                for (int i = 0; i < inst_array.Count; ++i) {
                    if (inst_array[i] != null) {
                        if (!inst_array[i].Build())
                            return false;
                    }
                }
            }

            if (inst_map != null) {
                for (int i = 0; i < inst_map.Count; ++i) {
                    if (inst_map[i] != null) {
                        if (!inst_map[i].key.Build())
                            return false;
                        if (!inst_map[i].val.Build())
                            return false;
                    }
                }
            }

            if (inst_object != null) {
                var e = inst_object.GetEnumerator();
                while (e.MoveNext()) {
                    if (e.Current.Value != null) {
                        if (!e.Current.Value.Build())
                            return false;
                    }
                }
            }

            return true;
        }

		public bool Bool
		{
			get { return inst_bool; }
			set { inst_bool = value; }
		}

        public byte Byte {
            get { return inst_byte; }
            set { inst_byte = value; }
        }

        public short Short {
            get { return inst_short; }
            set { inst_short = value; }
		}

        public int Int {
            get { return inst_int; }
            set { inst_int = value; }
        }

		public float Float
		{
			get { return inst_float; }
			set { inst_float = value; }
		}

		public string String
		{
			get { return inst_string; }
			set { inst_string = value; }
		}

        public string Enum
		{
			get { return inst_enum; }
			set { inst_enum = value; }
		}

		public string BitField
		{
			get { return inst_bitfield; }
			set { inst_bitfield = value; }
		}

		public string TID
		{
			get { return inst_tid; }
			set { inst_tid = value; }
		}

        public IDictionary<string, GData> Obj
        {
            get { return inst_object; }
            set { inst_object = value; }
        }

        public List<GData> Array
        {
            get { return inst_array; }
            set { inst_array = value; }
        }


        public List<MapKV> Map {
            get { return inst_map; }
            set { inst_map = value; }
        }

		public string Type
		{
			get { return mType; }
		}

		public T GetType<T>() where T : GType
		{
			return GTypeManager.Instance.GetType(this.Type) as T;
		}

		public GData GetField(string fieldName)
		{
			GData ret;
			if (inst_object != null && inst_object.TryGetValue(fieldName, out ret))
				return ret;
			else
				return null;
		}

		public int Count
		{
			get {
                if (inst_array != null)
                    return inst_array.Count;
                if (inst_object != null)
                    return inst_object.Count;
                if (inst_map != null)
                    return inst_map.Count;
                return 0;
            }
		}

		public GData GetArrayIndex(int index)
		{
			if (inst_array == null) {
                GLog.LogError("GData.GetIndex: " + mType + " is not an array" + debugInfo);
                return null;
            }

			if (index < inst_array.Count) {
                return inst_array[index];
            } else {
                GLog.LogError("GData.GetIndex: " + index + " is out of index " + inst_array.Count + debugInfo);
                return null;
            }
		}

        public bool Equals(GData b)
        {
            if (mType.Equals(GType.Bool)) {
                return inst_bool == b.inst_bool;
            } else if (mType.Equals(GType.Byte)) {
                return inst_byte == b.inst_byte;
            } else if (mType.Equals(GType.Short)) {
                return inst_short == b.inst_short;
            } else if (mType.Equals(GType.Int)) {
                return inst_int == b.inst_int;
            } else if (mType.Equals(GType.Float)) {
                return inst_float == b.inst_float;
            } else if (mType.Equals(GType.String)) {
                return inst_string.Equals(b.inst_string);
            } else if (mType.Equals(GType.TID)) {
                return inst_tid.Equals(b.inst_tid);
            } else {
                // todo: demonyang not implement
                return false;
            }
        }

        public GData GetMapItem(GData key)
        {
            if (inst_map == null)
                return null;
            
            for (int i = 0; i < inst_map.Count; ++i) {
                if (inst_map[i].key.Equals(key)) {
                    return inst_map[i].val;
                }
            }
            return null;
        }

		public virtual GData GetFieldWithInheritance(string fieldName)
		{
			// 只有GObject有继承关系 //
			return GetField(fieldName);
		}


        public void InheriteParent(GData parent)
        {
            if (parent == null)
                return;
            
            if (this.mType.Equals(GType.Array)) {
                GLog.Log("inherit array '{0}' {1}", this.fieldInfo.Name, debugInfo);
                if (!this.isKVArray) {
                    int count = parent.inst_array.Count;
                    for (int i = 0; i < count; ++i) {
                        inst_array.Insert(i, parent.inst_array[i].Clone());
                    }
                } else {
                    int count = parent.inst_array.Count;
                    for (int i = 0; i < count; ++i) {
                        if (inst_array.Count <= i)
                            inst_array.Add(parent.inst_array[i].Clone());
                        else if (inst_array[i] == null)
                            inst_array[i] = parent.inst_array[i].Clone();
                    }
                }

                for (int i = 0; i < inst_array.Count; ++i) {
                    if (inst_array[i] == null)
                        GLog.LogError("Array item can't be null. Index '{0}' of '{1}' {2}", i, this.fieldInfo.Name, debugInfo);
                }
            } else if (mType.Equals(GType.Map)) {
                GLog.Log("inherit map '{0}' {1}", this.fieldInfo.Name, debugInfo);
                IList<MapKV> parentData = parent.inst_map;
                for (int i = 0; i < parentData.Count; ++i) {
                    if (GetMapItem(parentData[i].key) == null) {
                        inst_map.Insert(0, new MapKV() { key = parentData[i].key.Clone(), val = parentData[i].val.Clone() });
                    }
                }
            }
        }

		public virtual bool Validate()
		{
			// TODO: demonyang validate
			return true;
		}

		public virtual bool WriteJson(ref JsonData jsonData)
		{
			if (Validate() == false)
				return false;

            GType tp = GTypeManager.Instance.GetType(mType);
            return tp == null ? false : tp.WriteJson(ref jsonData, this);
		}

        public virtual bool WriteBinary(BinaryWriter writer, GObject inObj)
		{
            GType tp = GTypeManager.Instance.GetType(mType);
            return tp.WriteBinary(writer, this, inObj);
		}
	}


}

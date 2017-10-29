
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using System.Text;

namespace sgaf.util
{
    public class StringAtom
    {
        protected static Hashtable hashTable = new Hashtable();
        protected static int lastId = 10; // 10以下预留,null 空字符串等

        public static StringAtom Null = new StringAtom(0, null);
        public static StringAtom Empty = new StringAtom(1, "");

        public override bool Equals(object obj)
        {
            return Equals(obj as StringAtom);
        }

        public bool Equals(StringAtom other)
        {
            if (other == null) {
                return false;
            }
            return other.Id == this.Id;
        }

        public static bool operator ==(StringAtom lhs, StringAtom rhs)
        {
            if (ReferenceEquals(lhs, rhs)) {
                return true;
            }

            if (((object)lhs == null) || ((object)rhs == null)) {
                return false;
            }

            //return string.Compare(lhs.Str, rhs.Str) == 0;
            return lhs.Id == rhs.Id;
        }

        public static bool operator !=(StringAtom lhs, StringAtom rhs)
        {
            if (ReferenceEquals(lhs, rhs)) {
                return false;
            }

            if (((object)lhs==null) || ((object)rhs==null)) {
                return true;
            }

            return lhs.Id != rhs.Id;
            //return string.Compare(lhs.Str, rhs.Str) != 0;
        }

        public int Id { get; private set; }
        public string Str { get; private set; }

        public StringAtom(int id, string str)
        {
            Id = id;
            Str = str;
        }

        /// <summary>
        /// Manuals the add.
        /// </summary>
        public static void ManualAdd(string s, int id)
        {
            lock (hashTable.SyncRoot) {
                hashTable[s] = id;
            }
        }

        /// <summary>
        /// Sets the alloc identifier.
        /// </summary>
        /// <param name="id">Identifier.</param>
        public static void SetAllocId(int id)
        {
            lastId = id;
        }

        /// <summary>
        /// Gets the string atom.
        /// </summary>
        /// <returns>The string atom.</returns>
        /// <param name="s">S.</param>
        public static StringAtom FromStr(string s)
        {
            if (s == null)
                return Null;
            else if (s.Length == 0)
                return Empty;

            if (!hashTable.ContainsKey(s)) {
                lock (hashTable.SyncRoot) {
                    hashTable.Add(s, new StringAtom(++lastId, s));
                }
            }

            return hashTable[s] as StringAtom;
        }

        /// <summary>
        /// Serves as a hash function for a <see cref="T:sgaf.util.StringAtom"/> object.
        /// </summary>
        /// <returns>A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a
        /// hash table.</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }


        /// <summary>
        /// Load from reader.
        /// </summary>
        public static StringAtom FromReader(BinaryReader reader)
        {
            int len = reader.ReadInt32();
            if (len <= 0)
                return FromStr(null);

            string str = Encoding.UTF8.GetString(reader.ReadBytes(len));
            return FromStr(str);
        }


        /// <summary>
        /// 方便打日志
        /// </summary>
        public override string ToString()
        {
            return Str;
        }
    }
}


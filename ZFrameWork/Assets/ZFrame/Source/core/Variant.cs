using System;
using System.Collections.Generic;
using System.Linq;

namespace zf.core
{
    [Serializable]
    public class Variant<T>
    {
        public Variant(T value)
        {
            _value = value;
        }

        public T Value { get { return _value; } set { _value = value; } }

        private T _value;

        public T GetValue() {
            return _value;
        }

        public void SetValue(T value) {
            _value = value;
        }

        public override string ToString() { return _value.ToString(); }
        public static implicit operator Variant<T>(T value) { var variant = new Variant<T>(value); return variant; }
    }

    public class BoardProperty<T>
    {
        public string name { get; set; }

        T defaultValue;

        public BoardProperty(string name, T defaultValue) {
            this.name = name;
            this.defaultValue = defaultValue;
        }

        public BoardProperty(string name):this (name, default(T)) {
            
        }

        public T GetDefault() {
            return defaultValue;
        }
    }
}

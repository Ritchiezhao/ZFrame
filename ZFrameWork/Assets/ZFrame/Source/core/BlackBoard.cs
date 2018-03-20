using System;
using System.Collections.Generic;

namespace zf.core
{
    [Serializable]
    public class BlackBoard
    {
        protected Dictionary<string,object> nameToValues;

        public BlackBoard()
        {
            nameToValues = new Dictionary<string, object> ();
        }

        public void Set<T>(BoardProperty<T> prop, T value)
        {
            object obj;
            if (nameToValues.TryGetValue (prop.name, out obj)) {
                Variant<T> v = obj as Variant<T>;
                if (v != null) {
                    v.Value = value;
                } else {
                    obj = new Variant<T> (value);
                    nameToValues[prop.name] = obj;
                }
            } else {
                Variant<T> newvar = new Variant<T> (value);
                nameToValues.Add (prop.name, newvar);
            }
        }

        public T Get<T>(BoardProperty<T> prop)
        {
            object obj;
            if (nameToValues.TryGetValue (prop.name, out obj)) {
                if (obj is Variant<T>) {
                    return ((Variant<T>)obj).Value;
                } else {
                    throw new ArgumentException (String.Format ("variant {0} was not a variant", prop.name));
                }
            } else {
                nameToValues.Add (prop.name, prop.GetDefault ());
                return ((Variant<T>)nameToValues [prop.name]).Value;
            }
        }

        public void SetByName<T>(string name, T value)
        {
            object obj;
            if (nameToValues.TryGetValue (name, out obj)) {
                Variant<T> v = obj as Variant<T>;
                if (v != null) {
                    v.Value = value;
                } else {
                    obj = new Variant<T> (value);
                    nameToValues[name] = obj;
                }
            } else {
                Variant<T> newvar = new Variant<T> (value);
                nameToValues.Add (name, newvar);
            }
            //nameToValues [name] = new Variant<T> (value);
        }

        public T GetByName<T>(string name)
        {
            object obj;
            if (nameToValues.TryGetValue (name, out obj)) {
                if (obj is Variant<T>) {
                    return ((Variant<T>)obj).Value;
                } else {
                    throw new ArgumentException (String.Format ("variant {0} was not a variant", name));
                }
            } else {
                throw new ArgumentException (String.Format ("not found variant {0}", name));
            }

        }
    }
}


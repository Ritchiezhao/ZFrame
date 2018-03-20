using System;
using System.Collections.Generic;

namespace zf.core
{  

    [Serializable]
    public class TypeBoard<T>
    {
        public Dictionary<string,T> nameToTValue;

        public TypeBoard()
        {
            nameToTValue = new Dictionary<string,T>();
        }

        public T GetValue(string name)
        {
            T outval;
            if (nameToTValue.TryGetValue (name, out outval)) {
                return outval;
            }
            throw new ArgumentException (String.Format ("not found var {0}", name));
        }

        public T GetValue(BoardProperty<T> prop)
        {
            T outval;
            if (nameToTValue.TryGetValue (prop.name, out outval)) {
                return outval;
            }
            nameToTValue.Add (prop.name, prop.GetDefault ());
            return nameToTValue [prop.name];
        }

        public void SetValue(string name, T value)
        {
            nameToTValue [name] = value;
        }
    }

    [Serializable]
    public class WhiteBoard
    {
        protected TypeBoard<bool>    boolBoard;
        protected TypeBoard<int>     intBoard;
        protected TypeBoard<uint>    uintBoard;
        protected TypeBoard<float>   floatBoard;
        protected TypeBoard<Fixed>   fixedBoard;
        protected TypeBoard<string>  stringBoard;
        protected TypeBoard<Vector2> vec2Board;
        protected TypeBoard<Vector3> vec3Board;
        protected TypeBoard<Vector4> vec4Board;
        protected TypeBoard<WhiteBoard> whiteBoard;
        protected BlackBoard blackBoard;

        public WhiteBoard (bool lazyCreate=true)
        {
            if (!lazyCreate) {
                boolBoard = new TypeBoard<bool> ();
                intBoard = new TypeBoard<int> ();
                uintBoard = new TypeBoard<uint> ();
                floatBoard = new TypeBoard<float> ();
                fixedBoard = new TypeBoard<Fixed> ();
                stringBoard = new TypeBoard<string> ();
                vec2Board = new TypeBoard<Vector2> ();
                vec3Board = new TypeBoard<Vector3> ();
                vec4Board = new TypeBoard<Vector4> ();
            }
        }

        public void SetBool(string name, bool value)
        {
            if (boolBoard == null) {
                boolBoard = new TypeBoard<bool> ();
            }
            boolBoard.SetValue (name, value);
        }

        public void Set(BoardProperty<bool> prop, bool value)
        {
            if (boolBoard == null) {
                boolBoard = new TypeBoard<bool> ();
            }
            boolBoard.SetValue (prop.name, value);
        }

        public bool GetBool(string name)
        {
            if (boolBoard != null) {
                return boolBoard.GetValue (name);
            }
            throw new ArgumentException (String.Format ("not found var {0}", name)); 
        }

        public bool Get(BoardProperty<bool> prop)
        {
            if (boolBoard == null) {
                boolBoard = new TypeBoard<bool> ();
            }
            return boolBoard.GetValue (prop);
        }

        public void SetInt(string name, int value)
        {
            if (intBoard == null) {
                intBoard = new TypeBoard<int> ();
            }
            intBoard.SetValue (name, value);
        }

        public void Set(BoardProperty<int> prop, int value)
        {
            if (intBoard == null) {
                intBoard = new TypeBoard<int> ();
            }
            intBoard.SetValue (prop.name, value);
        }

        public int GetInt(string name)
        {
            if (intBoard != null) {
                return intBoard.GetValue (name);
            }
            throw new ArgumentException (String.Format ("not found var {0}", name)); 
        }

        public int Get(BoardProperty<int> prop)
        {
            if (intBoard == null) {
                intBoard = new TypeBoard<int> ();
            }
            return intBoard.GetValue (prop);
        }

        public void SetUint(string name, uint value)
        {
            if (uintBoard == null) {
                uintBoard = new TypeBoard<uint> ();
            }
            uintBoard.SetValue (name, value);
        }

        public void Set(BoardProperty<uint> prop, uint value)
        {
            if (uintBoard == null) {
                uintBoard = new TypeBoard<uint> ();
            }
            uintBoard.SetValue (prop.name, value);
        }

        public uint GetUint(string name)
        {
            if (uintBoard != null) {
                return uintBoard.GetValue (name);    
            }
            throw new ArgumentException (String.Format ("not found var {0}", name));
        }

        public uint Get(BoardProperty<uint> prop)
        {
            if (uintBoard == null) {
                uintBoard = new TypeBoard<uint> ();
            }
            return uintBoard.GetValue (prop);    
        }

        public void SetFloat(string name, float value)
        {
            if (floatBoard == null) {
                floatBoard = new TypeBoard<float> ();
            }
            floatBoard.SetValue (name, value);
        }

        public void Set(BoardProperty<float> prop, float value)
        {
            if (floatBoard == null) {
                floatBoard = new TypeBoard<float> ();
            }
            floatBoard.SetValue (prop.name, value);
        }

        public float GetFloat(string name)
        {
            if (floatBoard != null) {
                return floatBoard.GetValue (name);
            }
            throw new ArgumentException (String.Format ("not found var {0}", name));
        }

        public float Get(BoardProperty<float> prop)
        {
            if (floatBoard == null) {
                floatBoard = new TypeBoard<float> ();
            }
            return floatBoard.GetValue (prop);
        }

        public void SetFixed(string name, Fixed value)
        {
            if (fixedBoard == null) {
                fixedBoard = new TypeBoard<Fixed> ();
            }
            fixedBoard.SetValue (name, value);
        }

        public void Set(BoardProperty<Fixed> prop, Fixed value)
        {
            if (fixedBoard == null) {
                fixedBoard = new TypeBoard<Fixed> ();
            }
            fixedBoard.SetValue (prop.name, value);
        }

        public Fixed GetFixed(string name)
        {
            if (fixedBoard != null) {
                return fixedBoard.GetValue (name);    
            }
            throw new ArgumentException (String.Format ("not found var {0}", name));
        }

        public Fixed Get(BoardProperty<Fixed> prop)
        {
            if (fixedBoard == null) {
                fixedBoard = new TypeBoard<Fixed> ();
            }
            return fixedBoard.GetValue (prop);    
        }

        public void SetString(string name, string value)
        {
            if (stringBoard == null) {
                stringBoard = new TypeBoard<string> ();
            }
            stringBoard.SetValue (name, value);
        }

        public void Set(BoardProperty<string> prop, string value)
        {
            if (stringBoard == null) {
                stringBoard = new TypeBoard<string> ();
            }
            stringBoard.SetValue (prop.name, value);
        }

        public string GetString(string name)
        {
            if (stringBoard != null) {
                return stringBoard.GetValue (name);    
            }
            throw new ArgumentException (String.Format ("not found var {0}", name));
        }

        public string Get(BoardProperty<string> prop)
        {
            if (stringBoard == null) {
                stringBoard = new TypeBoard<string> ();
            }
            return stringBoard.GetValue (prop);    
        }


        public void SetVector2(string name, Vector2 value)
        {
            if (vec2Board == null) {
                vec2Board = new TypeBoard<Vector2> ();
            }
            vec2Board.SetValue (name, value);
        }

        public void Set(BoardProperty<Vector2> prop, Vector2 value)
        {
            if (vec2Board == null) {
                vec2Board = new TypeBoard<Vector2> ();
            }
            vec2Board.SetValue (prop.name, value);
        }

        public Vector2 GetVector2(string name)
        {
            if (vec2Board != null) {
                return vec2Board.GetValue (name);    
            }
            throw new ArgumentException (String.Format ("not found var {0}", name));
        }

        public Vector2 Get(BoardProperty<Vector2> prop)
        {
            if (vec2Board == null) {
                vec2Board = new TypeBoard<Vector2> ();
            }
            return vec2Board.GetValue (prop);    
        }

        public void SetVector3(string name, Vector3 value)
        {
            if (vec3Board == null) {
                vec3Board = new TypeBoard<Vector3> ();
            }
            vec3Board.SetValue (name, value);
        }

        public void Set(BoardProperty<Vector3> prop, Vector3 value)
        {
            if (vec3Board == null) {
                vec3Board = new TypeBoard<Vector3> ();
            }
            vec3Board.SetValue (prop.name, value);
        }

        public Vector3 GetVector3(string name)
        {
            if (vec3Board != null) {
                return vec3Board.GetValue (name);    
            }
            throw new ArgumentException (String.Format ("not found var {0}", name));
        }

        public Vector3 Get(BoardProperty<Vector3> prop)
        {
            if (vec3Board == null) {
                vec3Board = new TypeBoard<Vector3> ();
            }
            return vec3Board.GetValue (prop);    
        }

        public void SetVector4(string name, Vector4 value)
        {
            if (vec4Board == null) {
                vec4Board = new TypeBoard<Vector4> ();
            }
            vec4Board.SetValue (name, value);
        }

        public void Set(BoardProperty<Vector4> prop, Vector4 value)
        {
            if (vec4Board == null) {
                vec4Board = new TypeBoard<Vector4> ();
            }
            vec4Board.SetValue (prop.name, value);
        }

        public Vector4 GetVector4(string name)
        {
            if (vec4Board != null) {
                return vec4Board.GetValue (name);    
            }
            throw new ArgumentException (String.Format ("not found var {0}", name));
        }

        public Vector4 Get(BoardProperty<Vector4> prop)
        {
            if (vec4Board == null) {
                vec4Board = new TypeBoard<Vector4> ();
            }
            return vec4Board.GetValue (prop);    
        }

        public void SetWhiteBoard(string name, WhiteBoard value)
        {
            if (whiteBoard == null) {
                whiteBoard = new TypeBoard<WhiteBoard> ();
            }
            whiteBoard.SetValue (name, value);
        }

        public void Set(BoardProperty<WhiteBoard> prop, WhiteBoard value)
        {
            if (whiteBoard == null) {
                whiteBoard = new TypeBoard<WhiteBoard> ();
            }
            whiteBoard.SetValue (prop.name, value);
        }

        public WhiteBoard GetWhiteBoard(string name)
        {
            if (whiteBoard != null) {
                return whiteBoard.GetValue (name);    
            }
            throw new ArgumentException (String.Format ("not found var {0}", name));
        }

        public WhiteBoard Get(BoardProperty<WhiteBoard> prop)
        {
            if (whiteBoard == null) {
                whiteBoard = new TypeBoard<WhiteBoard> ();
            }
            return whiteBoard.GetValue (prop);    
        }

        public void SetExt<T>(BoardProperty<T> prop, T value)
        {
            if (blackBoard == null) {
                blackBoard = new BlackBoard ();
            }
            blackBoard.Set<T> (prop, value);
        }

        public T GetExt<T>(BoardProperty<T> prop)
        {
            if (blackBoard == null) {
                blackBoard = new BlackBoard ();
            }
            return blackBoard.Get<T> (prop);
        }

        public void SetExtByName<T>(string name, T value)
        {
            if (blackBoard == null) {
                blackBoard = new BlackBoard ();
            }
            blackBoard.SetByName<T> (name, value);
        }

        public T GetExtByName<T>(string name)
        {
            if (blackBoard != null) {
                return blackBoard.GetByName<T> (name);
            }
            throw new ArgumentException (String.Format ("not found var {0}", name));
        }
    }
}


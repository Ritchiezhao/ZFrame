using UnityEditor;
using UnityEngine;


namespace ZFEditor
{
    public class UnityGUI
    {
        // styles //
        static GUIStyle mDefaultStyle;

        // Foldout_Property //
        static GUIStyle _foldout_property;
        public static GUIStyle foldout_property
        {
            get {
                if (_foldout_property == null)
                {
                    _foldout_property = new GUIStyle(EditorStyles.foldout);
                    _foldout_property.fontSize = 12;
                    _foldout_property.richText = true;
                    _foldout_property.normal.textColor = Color.white;
                    _foldout_property.focused.textColor = Color.white;
                    _foldout_property.hover.textColor = Color.white;
                    _foldout_property.active.textColor = Color.white;
                    _foldout_property.onNormal.textColor = Color.white;
                    _foldout_property.onFocused.textColor = Color.white;
                    _foldout_property.onHover.textColor = Color.white;
                    _foldout_property.onActive.textColor = Color.white;
                }
                return _foldout_property;
            }
        }

        // Label_ //
        static GUIStyle _label_property;
        public static GUIStyle label_property
        {
            get
            {
                if (_label_property == null)
                {
                    _label_property = new GUIStyle(EditorStyles.foldout);
                    _label_property.fontSize = 12;
                    _label_property.richText = true;
                    _label_property.normal.textColor = Color.white;
                    _label_property.focused.textColor = Color.white;
                    _label_property.hover.textColor = Color.white;
                    _label_property.active.textColor = Color.white;
                    _label_property.onNormal.textColor = Color.white;
                    _label_property.onFocused.textColor = Color.white;
                    _label_property.onHover.textColor = Color.white;
                    _label_property.onActive.textColor = Color.white;
                }
                return _foldout_property;
            }
        }

        // methods //
        public static bool Foldout(Rect rect, bool foldout, string label, GUIStyle style)
        {
            return EditorGUI.Foldout(rect, foldout, label, style);
        }

        /*
        public static string Label(Rect rect, string text, bool editable, GUIStyle style)
        {
            if (editable)
            {
                return GUI.Label(rect, text, style);
            }
        }

        public static string Label(Rect rect, string text, bool editable, FontStyle fontStyle, int fontSize)
        {

        }*/
    }
}
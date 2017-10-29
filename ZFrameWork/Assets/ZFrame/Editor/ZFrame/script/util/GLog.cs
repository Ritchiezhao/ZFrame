using System;
using System.Text;

#if UNITY_EDITOR
using UnityEngine;
#endif

namespace ZFEditor
{
    public class GLog
    {
        public static void Log(string log)
        {
		#if UNITY_EDITOR
            Debug.Log("[LOG]: " + log);
		#else
		    Console.WriteLine("[LOG]: " + log);
		#endif
        }


        public static void Log(string log, params object[] args)
        {
#if UNITY_EDITOR
            Debug.Log("[LOG]: " + string.Format(log, args));
#else
            Console.WriteLine("[LOG]: " + string.Format(log, args));
#endif
        }

        public static void LogError(string log)
        {
		#if UNITY_EDITOR
            Debug.LogError("[ERROR]: " + log);
		#else
		    Console.WriteLine("[ERROR]: " + log);
		#endif
        }


        public static void LogError(string log , params object[] args)
        {
#if UNITY_EDITOR
            Debug.LogError("[ERROR]: " + string.Format(log, args));
#else
            Console.WriteLine("[ERROR]: " + string.Format(log, args));
#endif
        }
    }
}

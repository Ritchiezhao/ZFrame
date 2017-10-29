﻿
using System;
using System.Text;
using System.IO;
using System.Threading;

namespace zf.util
{
    public class Logger
    {
        [ThreadStatic]
        static bool inited = false;
        [ThreadStatic]
        static Action<String> LogHandler;
        [ThreadStatic]
        static Action<String> InfoHandler;
        [ThreadStatic]
        static Action<String> WarningHandler;
        [ThreadStatic]
        static Action<String> ErrorHandler;

        [ThreadStatic]
        static StringBuilder builder;

        static bool registeredUnityMsg = false;
        static string dirPath = null;

        public static void CheckInited()
        {
            if (!inited)
                Init();
        }

        public static void Init()
        {
            inited = true;
#if UNITY_EDITOR
            LogHandler = UnityEngine.Debug.Log;
            InfoHandler = UnityEngine.Debug.Log;
            WarningHandler = UnityEngine.Debug.LogWarning;
            ErrorHandler = UnityEngine.Debug.LogError;
#else
            InitLogFile();
            LogHandler = WriteToFile;
            InfoHandler = WriteToFile;
            WarningHandler = WriteToFile;
            ErrorHandler = WriteToFile;
#if PLATFORM_UNITY
            if (!registeredUnityMsg) {
                registeredUnityMsg = true;
                UnityEngine.Application.logMessageReceived += HandleSysLog;
            }
#endif
#endif
        }

        static string Format(string format, params object[] args)
        {
            if (builder == null)
                builder = new StringBuilder();
            builder.Length = 0;
            builder.AppendFormat(format, args);
            return builder.ToString();
        }

#if PLATFORM_UNITY
        // 额外记录系统的LogError Exception信息 //
        static void HandleSysLog(string logString, string stackTrace, UnityEngine.LogType type)
        {
            switch (type) {
                case UnityEngine.LogType.Log:
                    break;
                case UnityEngine.LogType.Warning:
                    break;
                case UnityEngine.LogType.Error:
                    ErrorHandler(Format("[Sys Error]: {0}\r\n{1}\r\n", logString, stackTrace));
                    break;
                case UnityEngine.LogType.Assert:
                    ErrorHandler(Format("[Sys Assert]: {0}\r\n{1}\r\n", logString, stackTrace));
                    break;
                case UnityEngine.LogType.Exception:
                    ErrorHandler(Format("[Sys Exception]: {0}\r\n{1}\r\n", logString, stackTrace));
                    break;
            }
        }
#endif

        // log
        public static void Log(string log)
        {
            CheckInited();
            if (LogHandler != null) {
                LogHandler(Format("[L {0}]: {1}", DateTime.Now.ToString("MM/dd hh:mm:ss.fff"), log));
            }
        }

        public static void Log<T1>(string format, T1 param1)
        {
#if !RELEASE_MODE
            Log(Format(format, param1));
#endif
        }

        public static void Log<T1, T2>(string format, T1 param1, T2 param2)
        {
#if !RELEASE_MODE
            Log(Format(format, param1, param2));
#endif
        }

        public static void Log<T1, T2, T3>(string format, T1 param1, T2 param2, T3 param3)
        {
#if !RELEASE_MODE
            Log(Format(format, param1, param2, param3));
#endif
        }

        public static void Log(string format, params object[] args)
        {
#if !RELEASE_MODE
            Log(Format(format, args));
#endif
        }

        // info
        public static void Info(string log)
        {
            CheckInited();
            if (LogHandler != null) {
                LogHandler(Format("[I {0}]: {1}", DateTime.Now.ToString("MM/dd hh:mm:ss.fff"), log));
            }
        }

        public static void Info<T1>(string format, T1 param1)
        {
            Info(Format(format, param1));
        }

        public static void Info<T1, T2>(string format, T1 param1, T2 param2)
        {
            Info(Format(format, param1, param2));
        }

        public static void Info<T1, T2, T3>(string format, T1 param1, T2 param2, T3 param3)
        {
            Info(Format(format, param1, param2, param3));
        }

        public static void Info(string format, params object[] args)
        {
            Info(Format(format, args));
        }

        // warning
        public static void Warning(string log)
        {
            CheckInited();
            if (WarningHandler != null) {
                WarningHandler(Format("[W {0}]: {1}", DateTime.Now.ToString("MM/dd hh:mm:ss.fff"), log));
            }
        }


        public static void Warning<T1>(string format, T1 param1)
        {
            Warning(Format(format, param1));
        }


        public static void Warning<T1, T2>(string format, T1 param1, T2 param2)
        {
            Warning(Format(format, param1, param2));
        }


        public static void Warning<T1, T2, T3>(string format, T1 param1, T2 param2, T3 param3)
        {
            Warning(Format(format, param1, param2, param3));
        }

        public static void Warning(string format, params object[] args)
        {
            Warning(Format(format, args));
        }


        // error
        public static void Error(string log)
        {
            CheckInited();
            if (ErrorHandler != null) {
                ErrorHandler(Format("[Error {0}]: {1}", DateTime.Now.ToString("MM/dd hh:mm:ss.fff"), log));
            }
        }


        public static void Error<T1>(string format, T1 param1)
        {
            Error(Format(format, param1));
        }


        public static void Error<T1, T2>(string format, T1 param1, T2 param2)
        {
            Error(Format(format, param1, param2));
        }


        public static void Error<T1, T2, T3>(string format, T1 param1, T2 param2, T3 param3)
        {
            Error(Format(format, param1, param2, param3));
        }

        public static void Error(string format, params object[] args)
        {
            Error(Format(format, args));
        }

        public static void Break()
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Break();
#endif
        }



        // --------------- log to file ----------------------------------------------
        [ThreadStatic]
        static string fileName;
        [ThreadStatic]
        static StringBuilder writeFileBuffer;
        [ThreadStatic]
        static int lastWriteFileTime = 0;

        // 注意：不能在初始化线程中调用 //
        static public void InitLogFile()
        {
            // 创建文件 //
            if (dirPath == null) {
#if PLATFORM_UNITY
#if !UNITY_EDITOR 
                dirPath = Path.Combine(UnityEngine.Application.persistentDataPath, "Logs/");
#else
                dirPath = "Logs/";
#endif
#else
                dirPath = "Logs/";
#endif
            }


            if (!Directory.Exists(dirPath)) {
                Directory.CreateDirectory(dirPath);
            }
            // 删除旧日志 //
            DirectoryInfo info = new DirectoryInfo(dirPath);
            FileInfo[] files = info.GetFiles();
            for (int i = 0; i < files.Length; ++i) {
                long deltaTicks = DateTime.Now.Ticks - files[i].LastWriteTime.Ticks;
                TimeSpan elapsedSpan = new TimeSpan(deltaTicks);
                if (elapsedSpan.Days >= 2) {
                    files[i].Delete();
                }
            }

            fileName = dirPath + "/Log_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + "t" + Thread.CurrentThread.ManagedThreadId + ".log";
        }

        static void WriteToFile(string str)
        {
#if ENCRYPT_LOG
            byte[] encrypted = FXOREncrypter.EncryptToBytes(str);
            logfile.Write(encrypted.Length);
            logfile.Write(encrypted, 0, encrypted.Length);
            logfile.Flush();
#else
            if (writeFileBuffer == null)
                writeFileBuffer = new StringBuilder(1024 * 16);
            writeFileBuffer.AppendLine(str);

            //             if (writeFileBuffer.Length > 1024 * 15
            //                 || (Environment.TickCount - lastWriteFileTime > 2000)
            //                 || lastWriteFileTime == 0) {

            if (true)
            {
                lastWriteFileTime = Environment.TickCount;

                if (writeFileBuffer.Length > 0) {
                    using (StreamWriter logfile = new StreamWriter(fileName, true)) {
                        logfile.Write(writeFileBuffer.ToString());
                    }
                    writeFileBuffer.Length = 0;
                }
            }
#endif
        }
	}
}
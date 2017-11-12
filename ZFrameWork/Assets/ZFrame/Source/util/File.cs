
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR
using UnityEngine;
#endif

using zf.core;

namespace zf.util
{
    public enum FileUri {
        UNKNOWN = 0,
        PATCH_DIR = 1,
        PATCH_LANG_DIR = 2,
        RES_DIR = 3,
        RES_LANG_DIR = 4,
        STREAM_DIR = 5,
        STREAM_LANG_DIR = 6
    };

    public struct FileLoc {
        public FileUri uri;
        public string modPath;

        public FileLoc(FileUri uri, string modPath)
        {
            this.uri = uri;
            this.modPath = modPath;
        }
    }

    public class File
    {
        public static string GetUriPath(FileUri uri, string appLang, string modPath)
        {
            string basePath = "";

#if UNITY_EDITOR
            switch (uri) {
                case FileUri.RES_DIR:
                case FileUri.RES_LANG_DIR:
                    basePath = Path.Combine("Assets/Resources/", modPath);
                    break;
                case FileUri.STREAM_DIR:
                case FileUri.STREAM_LANG_DIR:
                    basePath = Path.Combine(Application.streamingAssetsPath, modPath);
                    break;
                case FileUri.PATCH_DIR:
                case FileUri.PATCH_LANG_DIR:
                    basePath = Path.Combine(Application.persistentDataPath, modPath);
                    break;
            }
#elif SGAF_SERVER
            // todo:
            switch (uri)
            {
                case FileUri.RES_DIR:
                case FileUri.RES_LANG_DIR:
                case FileUri.STREAM_DIR:
                case FileUri.STREAM_LANG_DIR:
                case FileUri.PATCH_DIR:
                case FileUri.PATCH_LANG_DIR:
                    basePath = Path.Combine("Resources/", modPath);
                    break;
            }
#elif SGAF_ROBOT
            // todo:
            switch (uri) {
                case FileUri.RES_DIR:
                case FileUri.RES_LANG_DIR:
                case FileUri.STREAM_DIR:
                case FileUri.STREAM_LANG_DIR:
                case FileUri.PATCH_DIR:
                case FileUri.PATCH_LANG_DIR:
                    basePath = Path.Combine("Resources/", modPath);
                    break;
            }
#elif UNITY_ANDROID || UNITY_IOS
            switch (uri) {
                case FileUri.RES_DIR:
                case FileUri.RES_LANG_DIR:
                    basePath = modPath;
                    break;
                case FileUri.STREAM_DIR:
                case FileUri.STREAM_LANG_DIR:
                    basePath = Path.Combine(Application.streamingAssetsPath, modPath);
                    break;
                case FileUri.PATCH_DIR:
                case FileUri.PATCH_LANG_DIR:
                    basePath = Path.Combine(Application.persistentDataPath, modPath);
                    break;
            }
#endif
            if (uri == FileUri.RES_LANG_DIR || uri == FileUri.PATCH_LANG_DIR || uri == FileUri.STREAM_LANG_DIR)
                return Path.Combine(basePath, appLang);
            else
                return basePath;
        }

        static string GetPathWithoutExt(string path)
        {
            if (string.IsNullOrEmpty(path))
                return "";
            string ext = Path.GetExtension(path);
            return path.Substring(0, path.Length - ext.Length);
        }

        /// <summary>
        /// 从OS的文件系统读取二进制文件
        /// </summary>
        public static byte[] RawLoadBytes(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                Logger.Warning("RawLoadBytes, path:{0} not exists", path);
                return null;
            }
            try {
                return System.IO.File.ReadAllBytes(path);
            } catch (Exception ex) {
                Logger.Error("Exception while reading {0}:\n{1}", path, ex);
                return null;
            }
        }

        /// <summary>
        /// 从OS的文件系统读取文本文件，path为 绝对路径 或 相对于Runtime的工作目录
        /// </summary>
        public static string RawLoadText(string path)
        {
            if (!System.IO.File.Exists(path))
                return null;
            try {
                return System.IO.File.ReadAllText(path, Encoding.UTF8);
            } catch (Exception ex) {
                Logger.Error("Exception while reading {0}:\n{1}", path, ex);
                return null;
            }
        }

        /// <summary>
        /// 从OS的文件系统中写文本文件
        /// </summary>
        /// <param name="path">绝对路径 或 相对于Runtime的工作目录</param>
        /// <param name="contents">内容</param>
        public static void RawWriteText(string path, string contents)
        {
            try {
                System.IO.File.WriteAllText(path, contents, Encoding.UTF8);
            } catch (Exception ex) {
                Logger.Error("Exception while writing {0}:\n{1}", path, ex);
                return;
            }
        }

        /// <summary>
        /// Loads the bin.
        /// pathToMod 相对于Mod的路径
        /// </summary>
        public static byte[] LoadBin(FileLoc loc, string appLang, string pathToMod)
        {
            string uriPath = GetUriPath(loc.uri, appLang, loc.modPath);

            if (loc.uri == FileUri.PATCH_DIR || loc.uri == FileUri.PATCH_LANG_DIR) {
                return RawLoadBytes(Path.Combine(uriPath, pathToMod));
            }
            else if (loc.uri == FileUri.RES_DIR || (loc.uri == FileUri.RES_LANG_DIR)) {
#if UNITY_EDITOR || SGAF_SERVER
                return RawLoadBytes(Path.Combine(uriPath, pathToMod));
#elif  UNITY_ANDROID || UNITY_IOS
                string relativeToRes = GetPathWithoutExt(Path.Combine(uriPath, pathToMod));
	            TextAsset ts = Resources.Load(relativeToRes) as TextAsset;
                if (ts == null) {
                    Logger.Error("LoadBin in res failed: {0}    rel:{1}", pathToMod, relativeToRes);
                    return null;
                }
                return ts.bytes;
#endif
            }
            else if (loc.uri == FileUri.STREAM_DIR || loc.uri == FileUri.STREAM_LANG_DIR) {
#if UNITY_EDITOR || SGAF_SERVER || UNITY_IOS
                return RawLoadBytes(Path.Combine(uriPath, pathToMod));
#elif UNITY_ANDROID
                // todo: call android method
                return null;
#endif
            }
            else 
            {
                Logger.Error("Unknown FileUri: " + loc.uri);
                return null;
            }
        }


        /// <summary>
        /// Loads the text.
        /// 注意路径都在Asset目录下，不包含Asset目录
        /// </summary>
        public static string LoadTxt(FileLoc loc, string appLang, string pathToMod)
        {
            string uriPath = GetUriPath(loc.uri, appLang, loc.modPath);
            if (loc.uri == FileUri.PATCH_DIR || loc.uri == FileUri.PATCH_LANG_DIR) {
                return RawLoadText(Path.Combine(uriPath, pathToMod));
            } else if (loc.uri == FileUri.RES_DIR || (loc.uri == FileUri.RES_LANG_DIR)) {
#if UNITY_EDITOR || SGAF_SERVER || SGAF_ROBOT
                return RawLoadText(Path.Combine(uriPath, pathToMod));
#elif UNITY_ANDROID || UNITY_IOS
                string relativeToRes = GetPathWithoutExt(Path.Combine(uriPath, pathToMod));
                TextAsset ts = Resources.Load(relativeToRes) as TextAsset;
                if (ts == null) {
                    Logger.Error("LoadTxt in res failed: {0}    rel:{1}", pathToMod, relativeToRes);
                    return null;
                }
                return ts.text;
#endif
            } else if (loc.uri == FileUri.STREAM_DIR || loc.uri == FileUri.STREAM_LANG_DIR) {
#if UNITY_EDITOR || SGAF_SERVER || UNITY_IOS
                return RawLoadText(Path.Combine(uriPath, pathToMod));
#elif UNITY_ANDROID
                // todo: call android method
                return null;
#endif
            } else {
                Logger.Error("Unknown FileUri: " + loc.uri);
                return null;
            }
        }

        /// <summary>
        /// Write the text.
        /// </summary>
        /// <param name="loc"></param>
        /// <param name="appLang"></param>
        /// <param name="pathToMod"></param>
        /// <param name="contents"></param>
        public static void WriteTxt(FileLoc loc, string appLang, string pathToMod, string contents)
        {
            string uriPath = GetUriPath(loc.uri, appLang, loc.modPath);
            RawWriteText(Path.Combine(uriPath, pathToMod), contents);
        }
    }
}


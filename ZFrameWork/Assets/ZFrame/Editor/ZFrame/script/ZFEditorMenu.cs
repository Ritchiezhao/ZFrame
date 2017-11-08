
using System;
using System.IO;
using ZFEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

using Debug = UnityEngine.Debug;
using File = System.IO.File;
#endif

public class ZFEditorMenu : EditorWindow
{
    static void InitConfig()
    {
        ZFGenerator.config = LitJson.JsonMapper.ToObject<ZFGenerator.Config>(File.ReadAllText("Tools/ZFConfig.json"));
    }

    [MenuItem("ZFEditor/GenerateAll")]
    public static void GenerateAll()
    {
        InitConfig();
        ZFGenerator.GenerateCode();
        ZFGenerator.GenerateProtoBuf();
        ZFGenerator.GenerateBin();
        AssetDatabase.Refresh(ImportAssetOptions.Default);
    }

    [MenuItem("ZFEditor/GenerateCode")]
    static void GenerateCode_Menu()
    {
        InitConfig();
        ZFGenerator.GenerateCode();
        AssetDatabase.Refresh(ImportAssetOptions.Default);
    }

    [MenuItem("ZFEditor/GenerateProto")]
    static void GenerateProtoBuf_Menu()
    {
        InitConfig();
        ZFGenerator.GenerateProtoBuf();
        AssetDatabase.Refresh(ImportAssetOptions.Default);
    }


    [MenuItem("ZFEditor/GenerateCSharpSerialize")]
    static void GenerateCode_CSSerialize()
    {
        InitConfig();
        ZFGenerator.GenerateCSSerialize();
        AssetDatabase.Refresh(ImportAssetOptions.Default);
    }

    [MenuItem("ZFEditor/GenerateRes")]
    public static void GenerateBin_Menu()
    {
        InitConfig();
        ZFGenerator.GenerateBin();
    }

    [MenuItem("ZFEditor/GenerateTables")]
    public static void GenerateTables_Menu()
    {
        InitConfig();
        ZFGenerator.GenerateTables();
        AssetDatabase.Refresh(ImportAssetOptions.Default);
    }
}



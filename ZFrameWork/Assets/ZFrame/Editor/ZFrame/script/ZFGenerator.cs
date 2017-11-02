
using System;
using System.IO;
using ZFEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;


public class ZFGenerator
{

    public class Config
    {
        public string json_types_dir = "PreDefine/TemplateTypes";
        public string assembly_path = "Library/ScriptAssemblies/Assembly-CSharp.dll";
        public string cs_output_dir = "Assets/Generated";

        public string proto_src = "";
        public string proto_output_dir = "";
        public string proto_gentool_dir = "";
        public string res_dir = "Assets/Resources";

        // todo:   fix me，用于控制Mod继承关系
        public string[] mods = { "GameMod", "BattleMod" };

        public bool genFramework = true;
        public BatchInfo batch = null;
    }

    public static Config config = new Config();


    static void LoadTypeDefs()
    {
        // load framework predefine types
        string[] typedefFiles = Directory.GetFiles(config.json_types_dir, "*.json");
        foreach (var jsonFile in typedefFiles) {
            GTypeManager.Instance.LoadJson(File.ReadAllText(jsonFile), jsonFile, null);
        }

        // load batch types
        if (config.batch != null) {
            string[] batchFiles = Directory.GetFiles(config.batch.json_types_dir, "*.json");
            foreach (var jsonFile in batchFiles) {
                GTypeManager.Instance.LoadJson(File.ReadAllText(jsonFile), jsonFile, config.batch);
            }
        }

        GTypeManager.Instance.Build();
    }

    static void LoadAssemblies()
    {
        // load assembly
        GTypeManager.Instance.LoadFromAssembly("Library/ScriptAssemblies/Assembly-CSharp.dll");
    }

	public static void GenerateCode()
	{
        GTypeManager.Instance.Clear();
        LoadTypeDefs();

        // generate code
        if (config.genFramework) {
            string frameCode1;
            GTypeManager.Instance.GenCode_CS_Head(out frameCode1, null);
	        if (!Directory.Exists(config.cs_output_dir))
	            Directory.CreateDirectory(config.cs_output_dir);
	        File.WriteAllText(Path.Combine(config.cs_output_dir, "Templates.cs"), frameCode1);

	        string frameCode2;
	        GTypeManager.Instance.GenCode_CS_Impl(out frameCode2, null);
	        File.WriteAllText(Path.Combine(config.cs_output_dir, "Templates_impl.cs"), frameCode2);

            string frameCode3;
	        GTypeManager.Instance.GenCode_CS_Binding(out frameCode3, null);
	        File.WriteAllText(Path.Combine(config.cs_output_dir, "Templates_binding.cs"), frameCode3);
        }

        if (config.batch != null) {
	        string code;
	        GTypeManager.Instance.GenCode_CS_Head(out code, config.batch);
            if (!Directory.Exists(config.batch.cs_output_dir))
	            Directory.CreateDirectory(config.batch.cs_output_dir);
	        File.WriteAllText(Path.Combine(config.batch.cs_output_dir, "Templates.cs"), code);

	        string code2;
	        GTypeManager.Instance.GenCode_CS_Impl(out code2, config.batch);
	        File.WriteAllText(Path.Combine(config.batch.cs_output_dir, "Templates_impl.cs"), code2);

	        string code3;
	        GTypeManager.Instance.GenCode_CS_Binding(out code3, config.batch);
	        File.WriteAllText(Path.Combine(config.batch.cs_output_dir, "Templates_binding.cs"), code3);
        }

		GLog.Log("Generate code finished!");
	}

	public static void GenerateCSSerialize()
	{
        GTypeManager.Instance.Clear();
        LoadAssemblies();

        // generate code
        if (config.genFramework) {
            string frameCode1;
            GTypeManager.Instance.GenCode_CS_Head(out frameCode1, null);
	        if (!Directory.Exists(config.cs_output_dir))
	            Directory.CreateDirectory(config.cs_output_dir);
	        File.WriteAllText(Path.Combine(config.cs_output_dir, "CSharpSerialize.cs"), frameCode1);

	        string frameCode2;
	        GTypeManager.Instance.GenCode_CS_Impl(out frameCode2, null);
	        File.WriteAllText(Path.Combine(config.cs_output_dir, "CSharpSerialize_impl.cs"), frameCode2);
        }

        if (config.batch != null) {
	        string code;
	        GTypeManager.Instance.GenCode_CS_Head(out code, config.batch);
            if (!Directory.Exists(config.batch.cs_output_dir))
	            Directory.CreateDirectory(config.batch.cs_output_dir);
	        File.WriteAllText(Path.Combine(config.batch.cs_output_dir, "CSharpSerialize.cs"), code);

	        string code2;
	        GTypeManager.Instance.GenCode_CS_Impl(out code2, config.batch);
	        File.WriteAllText(Path.Combine(config.batch.cs_output_dir, "CSharpSerialize_impl.cs"), code2);
        }

		GLog.Log("Generate code finished!");
	}

	/// <summary>
	/// Reads the templates.
	/// - ParentDir
	///     - Template
	///       + Bin
	///     + Script
	///     + Artwork
	/// </summary>
	static void ReadTemplates(string parentDir, string mod)
	{
		string dir = Path.Combine(parentDir, "Template");
		if (Directory.Exists(dir)) {
			string[] jsonFiles = Directory.GetFiles(dir, "*.json");
			foreach (var jsonFile in jsonFiles) {
                GDataManager.Instance.LoadJson(File.ReadAllText(jsonFile), parentDir, Path.GetFileName(jsonFile), mod);
			}
		}
	}


	static void MakeCleanFolderWithExt(string dir, string pattern)
	{
		if (!Directory.Exists(dir)) {
			Directory.CreateDirectory(dir);
		} else {
			foreach (string file in Directory.GetFiles(dir, pattern))
				File.Delete(file);
		}
	}

    // 遍历文件夹
    static void ActionFileInDir(string dir, string pattern, Action<string> action)
    {
        string[] files = Directory.GetFiles(dir, pattern);
        foreach (var file in files)
            if (action != null)
                action(file);

        string[] subDirs = Directory.GetDirectories(dir);
        foreach (var subdir in subDirs)
            ActionFileInDir(subdir, pattern, action);
    }

	public static void GenerateBin()
	{
        GTypeManager.Instance.Clear();
        LoadTypeDefs();

        GDataManager.Instance.Clear();

        GTIDGenerator.Instance.LoadRecord(Path.Combine(config.res_dir, "tids.txt"));
        ActionFileInDir(config.res_dir, "*.json", GTIDGenerator.Instance.PrepareTIDForJsonTmpls);

		// GameApp
		{
			if (Directory.Exists(config.res_dir)) {
				string[] jsonFiles = Directory.GetFiles(config.res_dir, "*.json");
				foreach (var jsonFile in jsonFiles) {
					GDataManager.Instance.LoadJson(File.ReadAllText(jsonFile), config.res_dir, Path.GetFileName(jsonFile), "GameApp");
				}

                GDataManager.Instance.Build();
			}

			Dictionary<string, byte[]> binaries;
			GDataManager.Instance.WriteBinary(config.res_dir, out binaries);

            if (binaries != null && binaries.Count > 0)
			    MakeCleanFolderWithExt(config.res_dir, "*.bytes");
            //else if (File.Exists(config))

			foreach (var kvFiles in binaries) {
				File.WriteAllBytes(Path.Combine(config.res_dir, "GameApp_bin.bytes"), kvFiles.Value);
			}
		}

		// Mods
		{
            // load mod templates
            string[] mods = config.mods;
			List<string> dirs = new List<string>();

			foreach (var mod in mods) {
                string modPath = Path.Combine(config.res_dir, mod);
                if (!Directory.Exists(modPath)) {
                    GLog.LogError("Mod folder '{0}' does not exist!", modPath);
                    continue;
                }
                // mod json
                string[] modJsons = Directory.GetFiles(modPath, "*.json");
                foreach (var modJson in modJsons) {
                    GDataManager.Instance.LoadJson(File.ReadAllText(modJson), modPath, Path.GetFileName(modJson), mod);
                }

                GDataManager.Instance.Build();

                Dictionary<string, byte[]> modBins;
                GDataManager.Instance.WriteBinary(modPath, out modBins);

                foreach (var kvFiles in modBins) {
                    File.WriteAllBytes(Path.Combine(modPath, "mod_bin.bytes"), kvFiles.Value);
                }

                // mod sub directories
				string[] categories = Directory.GetDirectories(modPath);
                foreach (var category in categories) {
					string[] subDirs = Directory.GetDirectories(category);
					foreach (var subDir in subDirs) {
						dirs.Add(subDir);
                        ReadTemplates(subDir, modPath);
					}
				}
                GDataManager.Instance.Build();
			}
            
            // generate mods binaries
            foreach (var dir in dirs) {
                Dictionary<string, byte[]> binaries;
                GDataManager.Instance.WriteBinary(dir, out binaries);

                string binOutputDir = Path.Combine(dir, "Template/Bin");
                if (Directory.Exists(binOutputDir))
                    Directory.Delete(binOutputDir, true);
                
                if (binaries != null && binaries.Count > 0)
                    MakeCleanFolderWithExt(binOutputDir, "*.bytes");

                foreach (var kvFiles in binaries) {
                    File.WriteAllBytes(Path.Combine(binOutputDir, kvFiles.Key + ".bytes"), kvFiles.Value);
                }
            }
		}

		GTIDGenerator.Instance.WriteRecord();
		GLog.Log("Generate res finished!");

		GenerateModsFileCache(config.res_dir);
		GLog.Log("Generate filecache.txt finished!");
	}


    public static void GenerateProtoBuf(string proto_src, string proto_output)
    {
        List<string> protoFiles = new List<string>();
        string curWorkingDir = Directory.GetCurrentDirectory();
        string command = "-i:";
        GetFiles(ref protoFiles, config.proto_src, "*.proto", "*.meta", config.proto_src);
        for (int i = 0; i < protoFiles.Count; ++i) {

            string file = protoFiles[i];
            int temp = file.IndexOf(".proto");
            string fileName = file.Substring(0,temp);

            command += config.proto_src +"/" +file + " ";
            command += "-o:"+ config.proto_output_dir+"/"+fileName + ".cs";
            try {
                Process myprocess = new Process();
                string protoFileName = config.proto_gentool_dir + "/protogen.exe";
                ProcessStartInfo startInfo = new ProcessStartInfo(protoFileName, command);
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.WorkingDirectory = curWorkingDir;
                myprocess.StartInfo = startInfo;
                myprocess.StartInfo.UseShellExecute = false;
                myprocess.Start();
                GLog.Log("command：" + command);
            }
            catch (Exception e0) {
                GLog.Log("启动应用程序时出错！原因：" + e0.Message);
            }
        }
    }

    public static void GenerateProtoBuf()
    {
        Console.WriteLine(Environment.OSVersion.Platform);
        if (config.genFramework)
        {
            GenerateProtoBuf(config.proto_src, config.proto_output_dir);
        }

        if (config.batch != null)
        {
            GenerateProtoBuf(config.batch.proto_src, config.batch.proto_output_dir);
        }

        GLog.Log("Generate ProtoBuf finished!");
    }


    static void GetFiles(ref List<string> ret, string dir, string pattern, string exclude, string rootDir)
	{
		int dirPathLength = rootDir[rootDir.Length - 1] == '/' ? rootDir.Length : rootDir.Length + 1;

		string[] files = Directory.GetFiles(dir, pattern);

		foreach (var file in files) {
			string[] excludeEnds = exclude.Split(';');

			bool needExclude = false;
			foreach (var excludeEnd in excludeEnds) {
				if (file.EndsWith(excludeEnd, StringComparison.Ordinal)) {
					needExclude = true;
					break;
				}
			}

            if (!needExclude) {
                string filePath = file.Substring(dirPathLength);
                filePath = filePath.Replace("\\", "/");
                ret.Add(filePath);
            }			
		}

		foreach (var subDir in Directory.GetDirectories(dir)) {
			GetFiles(ref ret, subDir, pattern, exclude, rootDir);
		}
	}

	public static void GenerateModsFileCache(string rootDir)
	{
		string[] mods = Directory.GetDirectories(rootDir);
		foreach (var mod in mods) {
			List<string> files = new List<string>();
			GetFiles(ref files, mod, "*.*", ".meta;.DS_Store", mod);
			string fileName = Path.Combine(mod, "filecache.txt");
			if (File.Exists(fileName))
				File.Delete(fileName);
			File.WriteAllLines(fileName, files.ToArray());
		}
	}
}


using UnityEngine;
using UnityEditor;

using System.Collections.Generic;
using System.IO;

namespace ZFEditor
{
    public class MainWindow : WindowBase
    {
        public class Config
        {
            public string types_dir = "PreDefine/TemplateTypes";
            public string cs_output_dir = "Assets/Generated";

            public string fb_temp = "PreDefine/GameMsg/Include";
            public string fb_src = "PreDefine/GameMsg/GameMsg.fbs";
            public string fb_output_dir = "Assets/Plugins/Generated/GameMsg";
            public string res_dir = "Assets/Resources";
            public string flatc_path_mac = "Tools/flatc/flatc";
            public string flatc_path_win = "Tools/flatc/flatc.exe";

            // todo:   fix me，用于控制Mod继承关系
            public string[] mods = { "GameMod", "BattleMod" };

            public bool genFramework = true;
            public BatchInfo batch = null;
        }
        static Config config = new Config();

        #region singletons
        // instance //
        public static MainWindow Inst { get; set; }
        public static void Open()
        {
            Inst = EditorWindow.GetWindow<MainWindow>(false, "sgaf editor");
        }

        public static ProjectView Project;
        public static WorkView Workspace;
        #endregion


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


        static void LoadTypeDefs()
        {
            GTypeManager.Instance.Clear();

            // load framework predefine types
            string[] typedefFiles = Directory.GetFiles(config.types_dir, "*.json");
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

        protected override void DoInit()
        {
            GTypeManager.Instance.Clear();
            GDataManager.Instance.Clear();

            // load typedef json
            string[] typedefFiles = Directory.GetFiles(config.types_dir, "*.json");
            foreach (var jsonFile in typedefFiles) {
                GTypeManager.Instance.LoadJson(File.ReadAllText(jsonFile), jsonFile);
            }
            GTypeManager.Instance.Build();

            GTIDGenerator.Instance.LoadRecord(Path.Combine(config.res_dir, "tids.txt"));

            string resJsonDir = config.cs_output_dir;

            // GameApp
            {
                if (Directory.Exists(resJsonDir)) {
                    string[] jsonFiles = Directory.GetFiles(resJsonDir, "*.json");
                    foreach (var jsonFile in jsonFiles) {
                        GDataManager.Instance.LoadJson(File.ReadAllText(jsonFile), resJsonDir, Path.GetFileName(jsonFile), "GameApp");
                    }
                }
            }

            // Mods
            {
                // load mod templates
                string[] mods = Directory.GetDirectories(resJsonDir);
                List<string> dirs = new List<string>();

                foreach (var mod in mods) {
                    string[] categories = Directory.GetDirectories(mod);

                    foreach (var category in categories) {
                        string[] subDirs = Directory.GetDirectories(category);
                        foreach (var subDir in subDirs) {
                            dirs.Add(subDir);
                            ReadTemplates(subDir, mod);
                        }
                    }
                }
            }

            mainPanel = CreateUI<UIContainer>();
            mainPanel.Init_UIContainer(LayoutType.Horizontal);
            mainPanel.EnableMouseResize(true);

            UIPanel left = mainPanel.CreateNode<UIPanel>();
            left.Init("Project", LayoutType.Vertical);
            left.SetMinSize(new Vector2(150, 10));
            left.MaxSize.x = 300;

            Project = new ProjectView();
            Project.Init(left);

            Workspace = new WorkView();
            Workspace.Init(mainPanel.CreateNode<UITabPanel>());

            this.SetSkin(ResCache.GetSkin());
        }
    }


}

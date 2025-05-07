using Il2Cpp;
using MelonLoader;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[assembly: MelonInfo(typeof(Gm_Construct.Core), "Gm_Construct", "1.0.0", "Catalyss", null)]
[assembly: MelonGame("Pigeons at Play", "Mycopunk")]

namespace Gm_Construct
{
    public class Core : MelonMod
    {
        public static MelonLogger.Instance logger;
        public static bool isExampleMapLoaded = false;
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialized.");
            logger = LoggerInstance;
            //Il2Cpp.Global._buildID += "Modded Game";

            //Il2Cpp.Global.DevMode = true;
            //Il2Cpp.Online.appID = 3247750;  //Release Id
            //Il2Cpp.Online.appID = 3581750;  //Demo Id  
            HarmonyInstance.PatchAll();

            LoggerInstance.Msg("ActualPlates initialized!");

            if (persistentBundle == null)
            {
                var assembly = typeof(Core).Assembly;
                foreach (string resName in assembly.GetManifestResourceNames())
                    LoggerInstance.Msg("Found resource: " + resName);

                using (Stream stream = assembly.GetManifestResourceStream("Gm_Construct.resources.gm_construct"))
                {
                    if (stream == null)
                    {
                        LoggerInstance.Error("Failed to find embedded AssetBundle!");
                        return;
                    }

                    byte[] bundleData = new byte[stream.Length];
                    stream.Read(bundleData, 0, bundleData.Length);

                    persistentBundle = Il2CppAssetBundleManager.LoadFromMemory(bundleData);


                    if (persistentBundle == null)
                    {
                        LoggerInstance.Error("Failed to load AssetBundle from memory!");
                        return;
                    }
                    else
                    {
                        LoggerInstance.Msg("Embedded assets loaded successfully.");

                    }
                }
            }
        }


        private static Il2CppAssetBundle persistentBundle;
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            Injected = false;

            LoggerInstance.Msg($"Scene loaded: {sceneName}");

            if (sceneName == "Bridge")
            {
                Scene bridgeScene = SceneManager.GetSceneByName("Bridge");

                if (bridgeScene.IsValid() && bridgeScene.isLoaded)
                {



                    var lvel = persistentBundle.LoadAsset<GameObject>("assets/mods/examplemap/gm_construct.prefab");
                    level = GameObject.Instantiate(lvel,GameObject.FindObjectsOfType<GameObject>(true)[0].transform);
                    level.transform.SetParent(null);
                    Core.logger.Msg("Cleared all GameObjects from Bridge scene.");
                    if (GameObject.Find("DropPodDoor")) GameObject.Find("DropPodDoor").SetActive(false);
                    var wrldinf = UnityEngine.Object.FindObjectsOfType<Renderer>(true);
                    foreach (Renderer item in wrldinf)
                    {
                        foreach (Material mats in item.materials)
                        {
                            if (!item.transform.IsChildOf(level.transform)) continue;
                            var text = mats.mainTexture;
                            mats.shader = Shader.Find("Shader Graphs/PastelObjUV");
                            mats.mainTexture = text;
                        }
                    }

                    foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>(true))
                    {
                        if (go.scene == bridgeScene)
                        {
                            GameObject.Destroy(go);
                        }
                    }
                    DestroyInjector = true;
                }
            }
        }
        private GameObject level;
        private bool DestroyInjector = false;
        private bool Injected = false;

        public string scenepath = "";

        public override void OnUpdate()
        {
            if (DestroyInjector)
            {
                Scene bridgeScene = SceneManager.GetSceneByName("Bridge");

                if (bridgeScene.IsValid() && bridgeScene.isLoaded)
                {
                    foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>(true))
                    {
                        if (go == null || go == level || go.transform.IsChildOf(level.transform)) continue;
                        if (go.scene == bridgeScene)
                        {
                            GameObject.Destroy(go);
                        }
                    }
                }

            }

            if (GameObject.Find("SelectMissionWindow(Clone)") == null || Injected) return;
            if (Keyboard.current.lKey.wasReleasedThisFrame)
            {
                Injected = true;
                var wrldinf = UnityEngine.Object.FindObjectsOfType<MissionSelectButton>(true);
                foreach (MissionSelectButton item in wrldinf)
                {
                    var misst = item.GetComponent<MissionSelectButton>().mission;
                    MissionData CustomMission = new MissionData(misst.seed, misst.Mission, misst.Region, "Bridge", misst.Container);

                    CustomMission.Mission._missionName = "Gm_Construct";
                    CustomMission.Mission._description = "Test Mission Area";
                    item.GetComponent<MissionSelectButton>().mission = CustomMission;
                }
            }
        }
    }
}
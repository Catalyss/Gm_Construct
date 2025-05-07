using Il2Cpp;
using Il2CppMono.Security.Cryptography;
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
                    level = GameObject.Instantiate(lvel);
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

                    SceneManager.MoveGameObjectToScene(level, SceneManager.GetSceneByName("Bridge"));

                    foreach (GameObject go in GameObject.FindObjectsOfType<GameObject>(true))
                    {
                        if (go == null || go == level || go.transform.IsChildOf(level.transform)) continue;
                        if (go.scene == bridgeScene)
                        {
                            GameObject.Destroy(go);
                        }
                    }
                    DestroyInjector = true;
                }
            }
        }

        private void Setuplvl()
        {
            var startlvl = GameObject.Find("StartButtonMission");
            var steplvl = GameObject.Find("StepButtonMission");
            var endlvl = GameObject.Find("EndButtonMission");
            //var gamemanager = GameObject.Find("GameManager");
            var gamemanager = GameObject.Find("GameManager");
            var MissionManager = GameObject.Find("MissionManager");
            var EnemyManager = GameObject.Find("EnemyManager");


            EnemyManager em = EnemyManager.AddComponent<EnemyManager>();
            ClientGenericInteractable cgi = startlvl.AddComponent<ClientGenericInteractable>();
            MissionManager mm = MissionManager.AddComponent<MissionManager>();

            mm.enabled = true;
            em.enabled = true;

            cgi.interactText = "helpp";
            Mission mission = new Mission();
            mission.name = "test";
            mission.AutoStart= true;
            mission.ExtractAtEnd =true;
            mission.MissionType = MissionType.CleanupDetail;
            mission.SetupMission_Server(111);
            mission.StartFirstObjective =true;
            mm.mission = mission;
            mm.missionContainer = CustomMissions.Container;
            //cgi.OnInteract = mm.StartMission_Server();
            //mm.missionData;
        }
        private GameObject level;
        private bool DestroyInjector = false;
        private bool Injected = false;

        public string scenepath = "";
        public MissionData CustomMissions;
        public int ii = 0;

        public override void OnUpdate()
        {
            if (DestroyInjector)
            {
                if (level != null)
                {
                    Scene bridgeScene = SceneManager.GetSceneByName("Bridge");

                    if (bridgeScene.IsValid() && bridgeScene.isLoaded)
                    {
                        var ls = GameObject.FindObjectsOfType<GameObject>(true);
                        if (ii >= 10) { DestroyInjector = false; Setuplvl(); }
                        foreach (GameObject go in ls)
                        {
                            if (go == null || go == level || go.transform.IsChildOf(level.transform)) continue;
                            if (go.scene == bridgeScene)
                            {
                                GameObject.Destroy(go);
                            }
                        }

                        ii++;
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
                    MissionData CustomMission = CustomMissions = new MissionData(misst.seed, misst.Mission, misst.Region, "Bridge", misst.Container);

                    CustomMission.Mission._missionName = "Gm_Construct";
                    CustomMission.Mission._description = "Test Mission Area";
                    item.GetComponent<MissionSelectButton>().mission = CustomMission;
                }
            }
        }
    }
}
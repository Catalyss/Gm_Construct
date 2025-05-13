using Il2Cpp;
using Il2CppInterop.Runtime;
using Il2CppMono.Security.Cryptography;
using MelonLoader;
using Unity.Netcode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
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
            ii = 0;
            DestroyInjector = false;
            if(sceneName == "Hub")
            {
                GetMissions();
                ReplaceMissions();
            }
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
                    //if (GameObject.Find("DropPodDoor")) GameObject.Find("DropPodDoor").SetActive(false);
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
                    Setuplvl();
                    //DestroyInjector = true;
                }
            }
        }

        private void Setuplvl()
        {
            var startlvl = GameObject.Find("StartButtonMission");
            var steplvl = GameObject.Find("StepButtonMission");
            var endlvl = GameObject.Find("EndButtonMission");

            var MissionManager = GameObject.Find("MissionManager");
            var EnemyManager = GameObject.Find("EnemyManager");

            var managersvnfbhnd = Resources.FindObjectsOfTypeAll<EnemyManager>();
            EnemyManager em = EnemyManager.AddComponent<EnemyManager>();
            if (managersvnfbhnd[1] != null) em = managersvnfbhnd[1].GetComponent<EnemyManager>();

            ClientInteractableObject cgi = startlvl.AddComponent<ClientInteractableObject>();
            NetworkObject nocgi = startlvl.AddComponent<NetworkObject>();
            MissionManager mm = MissionManager.AddComponent<MissionManager>();
            NetworkObject nomm = MissionManager.AddComponent<NetworkObject>();

            
            nomm.enabled=true;
            nomm.Spawn(false);
            nocgi.enabled=true;
            nocgi.Spawn(false);

            mm.enabled = true;
            em.enabled = true;

            cgi.Setup("Help", false, true);


            //mm.mission = GetMissions();
            //mm.missionContainer = CustomMissions.Container;
            cgi.Setup("", false, true);

            //mm.missionData;
        }
        private RegulatedRampageMission GetMissions()
        {
            var cc = GameObject.CreatePrimitive(PrimitiveType.Cube);
            
            var mission = new RegulatedRampageMission(ScriptableObject.CreateInstance(Il2CppType.Of<RegulatedRampageMission>()).Pointer);

            var test =cc.AddComponent<RegulatedRampageObjective>();
            var no =cc.AddComponent<NetworkObject>();
            no.enabled=true;
            no.Spawn(false);
            test.enabled=true;

            test.customWaveChance = 5;
            test.enemyIntensityIncreaseDuration= new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<float>(new float[]{700f,1200f,3000f});
            test.initialEnemyIntensity=99999999f;

            PlayerModifier<float> t = new PlayerModifier<float>();
            t.values = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<float>(new float[]{700f,1200f,3000f,9000f});
            test.killProgressMultiplier = t;
            test.title="THIGNY TITILE";
            test.intensityIncreaseIndex=1;
            test.waypointTarget=cc.transform;
            test.name = "test Ob";


            mission.rampageObjective=test;
            mission.killsNeededForOnePlayer=5;

            mission.CompatibleLevels = LevelFlags.AllRegions;
            mission.MissionColor = Color.cyan;
            mission.name = "test";
            mission._description = "yessss";
            mission._missionName = "Fuck";
            mission._missionTypeName = "what ?";
            mission.AutoStart = true;
            mission.ExtractAtEnd = true;
            mission.MissionType = MissionType.Purge;
            mission.StartFirstObjective = true;
            return mission;
        }
        private GameObject level;
        private bool DestroyInjector = false;
        private bool Injected = false;
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
                var wrldinf = UnityEngine.Object.FindObjectsOfType<MissionSelectButton>(true);
                foreach (MissionSelectButton item in wrldinf)
                {
                    
                    var misst = item.GetComponent<MissionSelectButton>().mission;
                    MissionData CustomMission = CustomMissions = new MissionData(misst.seed, misst.Mission, misst.Region, "Bridge", misst.Container);
                    item.GetComponent<MissionSelectButton>().mission = CustomMission;
                }
            }
        }

        private void ReplaceMissions()
        {
            var allglobal = Resources.FindObjectsOfTypeAll<Il2Cpp.Global>();
            var allMissions = Resources.FindObjectsOfTypeAll<Il2Cpp.Mission>();
            Il2Cpp.Global global = allglobal[0];
            global.Missions = (Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<Mission>)allMissions;
        }
    }
}
//#define DEBUG_ENABLED

using System;
using System.Collections.Generic;
using System.Linq;
using OWML.Common;
using OWML.ModHelper;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OuterWildsFlyover
{
    public class OuterWildsFlyover : ModBehaviour
    {
        public static IModHelper SharedModHelper;

        bool isPoisEnabled = false;
        FlyoverSaveFile save;
        FlyoverProfile saveProfile;

#if DEBUG_ENABLED
        Font hudFont;
#endif

        private void Start()
        {
            SharedModHelper = ModHelper;
            //Import AssetBundles
            AssetBundleItems.Init(ModHelper);

            //Add audio
            AudioLibrary audioLibrary = Locator.GetAudioManager()._libraryAsset;
            AudioLibrary.AudioEntry[] entries = new AudioLibrary.AudioEntry[audioLibrary.audioEntries.Length + 12];
            Array.Copy(audioLibrary.audioEntries, entries, audioLibrary.audioEntries.Length);
            //Starting at a really high enum value to prevent collisions. An enum is an int so it can go really high
            entries[audioLibrary.audioEntries.Length + 0] = new AudioLibrary.AudioEntry((AudioType)7987700, new AudioClip[] { AssetBundleItems.InfoSeeAudio });
            entries[audioLibrary.audioEntries.Length + 1] = new AudioLibrary.AudioEntry((AudioType)7987701, new AudioClip[] { AssetBundleItems.InfoDescAudio });
            entries[audioLibrary.audioEntries.Length + 2] = new AudioLibrary.AudioEntry((AudioType)7987702, new AudioClip[] { AssetBundleItems.InfoHitAudio });
            entries[audioLibrary.audioEntries.Length + 3] = new AudioLibrary.AudioEntry((AudioType)7987703, new AudioClip[] { AssetBundleItems.ResultsMusicAudio });
            entries[audioLibrary.audioEntries.Length + 4] = new AudioLibrary.AudioEntry((AudioType)7987704, new AudioClip[] { AssetBundleItems.ResultsCountAudio });
            entries[audioLibrary.audioEntries.Length + 5] = new AudioLibrary.AudioEntry((AudioType)7987705, new AudioClip[] { AssetBundleItems.ResultsCountLastAudio });
            entries[audioLibrary.audioEntries.Length + 6] = new AudioLibrary.AudioEntry((AudioType)7987706, new AudioClip[] { AssetBundleItems.ResultsItemAudio });
            entries[audioLibrary.audioEntries.Length + 7] = new AudioLibrary.AudioEntry((AudioType)7987707, new AudioClip[] { AssetBundleItems.ResultsLastItemAudio });
            entries[audioLibrary.audioEntries.Length + 8] = new AudioLibrary.AudioEntry((AudioType)7987708, new AudioClip[] { AssetBundleItems.ResultsItemClickAudio });
            entries[audioLibrary.audioEntries.Length + 9] = new AudioLibrary.AudioEntry((AudioType)7987709, new AudioClip[] { AssetBundleItems.ResultsTabClickAudio });
            entries[audioLibrary.audioEntries.Length + 10] = new AudioLibrary.AudioEntry((AudioType)7987710, new AudioClip[] { AssetBundleItems.ResultsHoverAudio });
            entries[audioLibrary.audioEntries.Length + 11] = new AudioLibrary.AudioEntry((AudioType)7987711, new AudioClip[] { AssetBundleItems.ResultsButtonClickAudio });
            audioLibrary.audioEntries = entries;

            //Load save
            save = ModHelper.Storage.Load<FlyoverSaveFile>("flyoversave.json");
            if (save == null) save = new FlyoverSaveFile();

#if DEBUG_ENABLED
            //DEBUG: Load font
            hudFont = Resources.Load<Font>(@"fonts/english - latin/SpaceMono-Regular_Dynamic");
#endif

            //Register event handlers
            PoiManager.OnNewPoiCollected += (s, e) =>
            {
                saveProfile.Saves[0].Pois = new List<FlyoverSavePoi>(PoiManager.GetCollectedPois().Select(p => new FlyoverSavePoi() { Id = p.Id, IsNew = p.InitialCollectState == PoiCollectState.Uncollected && p.CollectState == PoiCollectState.CollectedNow }));
                ModHelper.Storage.Save<FlyoverSaveFile>(save, "flyoversave.json");
            };
            ResultsManager.OnCompletedResults += (s, e) =>
            {
                saveProfile.Saves[0].Pois = new List<FlyoverSavePoi>(PoiManager.GetCollectedPois().Select(p => new FlyoverSavePoi() { Id = p.Id, IsNew = false }));
                ModHelper.Storage.Save<FlyoverSaveFile>(save, "flyoversave.json");
            };

            LoadManager.OnCompleteSceneLoad += (originalScene, loadScene) =>
            {
                isPoisEnabled = loadScene == OWScene.SolarSystem || loadScene == OWScene.EyeOfTheUniverse;

                if (isPoisEnabled)
                {
                    saveProfile = save.GetOrCreateProfile(StandaloneProfileManager.SharedInstance?.currentProfile?.profileName ?? "XboxGamepassDefaultProfile");

                    PoiManager.SetupInternalObjects();
                    PoiManager.PlacePoisInScene(loadScene, saveProfile.GetCollectedPois());

                    ResultsManager.SetupScene();

#if DEBUG_ENABLED
                    DebugInit(loadScene);
#endif
                }
            };

            LoadManager.OnStartSceneLoad += (originalScene, loadScene) =>
            {
                if (isPoisEnabled)
                {
                    PoiManager.DestroyScene();
                    ResultsManager.DestroyScene();
                }
            };
        }

        private void Update()
        {
            if (!isPoisEnabled) return;

            PoiManager.Update();
            ResultsManager.Update();

#if DEBUG_ENABLED
            DebugUpdate();
#endif
        }

#if DEBUG_ENABLED

        FragmentCollisionProxy fragmentCollision;
        Poi debugPoi;
        Vector3 debugPosition;
        KeyValuePair<string, Lazy<Component>>[] debugAvailableParents;
        int debugCurrentParent;
        private void DebugInit(OWScene scene)
        {
            fragmentCollision = scene == OWScene.SolarSystem ? Locator.GetAstroObject(AstroObject.Name.BrittleHollow).transform.Find("FragmentCollisionProxy").GetComponent<FragmentCollisionProxy>() : null;

            KeyValuePair<string, Lazy<Component>>[] solarSystemParents = new KeyValuePair<string, Lazy<Component>>[]
            {
                new KeyValuePair<string, Lazy<Component>>("Sun", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.Sun))),
                new KeyValuePair<string, Lazy<Component>>("Sun Station", new Lazy<Component>(() => Locator.GetMinorAstroObject("Sun Station"))),
                new KeyValuePair<string, Lazy<Component>>("Ember Twin", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.CaveTwin))),
                new KeyValuePair<string, Lazy<Component>>("Ash Twin", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.TowerTwin))),
                new KeyValuePair<string, Lazy<Component>>("Ash Twin - Ash Twin Project", new Lazy<Component>(() => UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().First(o => o.name == "TimeLoopRing_Body").transform)),
                new KeyValuePair<string, Lazy<Component>>("Timber Hearth", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.TimberHearth))),
                new KeyValuePair<string, Lazy<Component>>("Timber Hearth - Quantum Shard", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.TimberHearth).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector_QuantumGrove").gameObject.transform.Find("Interactables_QuantumGrove").Find("QuantumShard"))),
                new KeyValuePair<string, Lazy<Component>>("Hearthian Satellite", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.TimberHearth).GetSatellite())),
                new KeyValuePair<string, Lazy<Component>>("The Attlerock", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.TimberHearth).GetMoon())),
                new KeyValuePair<string, Lazy<Component>>("Brittle Hollow", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.BrittleHollow))),
                new KeyValuePair<string, Lazy<Component>>("Brittle Hollow - Crossroads", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.BrittleHollow).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector_Crossroads"))),
                new KeyValuePair<string, Lazy<Component>>("Brittle Hollow - Escape Pod", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.BrittleHollow).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector_EscapePodCrashSite")._subsectors.First(s => s.gameObject.name == "Sector_CrashFragment"))),
                new KeyValuePair<string, Lazy<Component>>("Brittle Hollow - Old Settlement", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.BrittleHollow).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector_OldSettlement")._subsectors.First(s => s.gameObject.name == "Fragment OldSettlement 0"))),
                new KeyValuePair<string, Lazy<Component>>("Brittle Hollow - Quantum Tower", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.BrittleHollow).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector_QuantumFragment"))),
                new KeyValuePair<string, Lazy<Component>>("Brittle Hollow - Quantum Shard", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.BrittleHollow).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector_QuantumFragment").gameObject.transform.Find("Interactables_QuantumFragment").Find("Surface_QRock_Shard").Find("QuantumShard"))),
                new KeyValuePair<string, Lazy<Component>>("Brittle Hollow - Gravity Cannon", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.BrittleHollow).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector_GravityCannon"))),
                new KeyValuePair<string, Lazy<Component>>("Brittle Hollow - Black Hole Forge", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.BrittleHollow).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector_NorthHemisphere")._subsectors.First(s => s.gameObject.name == "Sector_NorthPole")._subsectors.First(s => s.gameObject.name == "Sector_HangingCity").gameObject.transform.Find("Sector_HangingCity_BlackHoleForge").Find("BlackHoleForgePivot"))),
                new KeyValuePair<string, Lazy<Component>>("Brittle Hollow - Hidden Scroll", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.BrittleHollow).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector Northern Gravcanonia")._subsectors.First(s => s.gameObject.name == "Fragment 55_NorthernGravcanonia"))),
                new KeyValuePair<string, Lazy<Component>>("Hollow's Lantern", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.BrittleHollow).GetMoon())),
                new KeyValuePair<string, Lazy<Component>>("Giants Deep", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.GiantsDeep))),
                new KeyValuePair<string, Lazy<Component>>("Giants Deep - Gabbro's Island", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.GiantsDeep).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector_GabbroIsland"))),
                new KeyValuePair<string, Lazy<Component>>("Giants Deep - Statue Island", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.GiantsDeep).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector_StatueIsland"))),
                new KeyValuePair<string, Lazy<Component>>("Giants Deep - Construction Yard", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.GiantsDeep).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector_ConstructionYard"))),
                new KeyValuePair<string, Lazy<Component>>("Giants Deep - Quantum Tower", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.GiantsDeep).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector_QuantumIsland"))),
                new KeyValuePair<string, Lazy<Component>>("Giants Deep - Bramble Island", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.GiantsDeep).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector_BrambleIsland"))),
                new KeyValuePair<string, Lazy<Component>>("Giants Deep - Gabbro's Ship", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.GiantsDeep).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector_GabbroShip"))),
                new KeyValuePair<string, Lazy<Component>>("Orbital Probe Cannon", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.ProbeCannon))),
                new KeyValuePair<string, Lazy<Component>>("Dark Bramble", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.DarkBramble))),
                new KeyValuePair<string, Lazy<Component>>("Dark Bramble - Feldspar Node", new Lazy<Component>(() => Locator.GetMinorAstroObject("Pioneer Dimension"))),
                new KeyValuePair<string, Lazy<Component>>("Dark Bramble - Vessel Node", new Lazy<Component>(() => Locator.GetMinorAstroObject("Vessel Dimension"))),
                new KeyValuePair<string, Lazy<Component>>("Dark Bramble - Escape Pod Node", new Lazy<Component>(() => Locator.GetMinorAstroObject("Escape Pod Dimension"))),
                new KeyValuePair<string, Lazy<Component>>("Dark Bramble - Nest Node", new Lazy<Component>(() => Locator.GetMinorAstroObject("Angler Nest Dimension"))),
                new KeyValuePair<string, Lazy<Component>>("Dark Bramble - Recursion Node", new Lazy<Component>(() => Locator._minorAstroObjects.First(o => o.GetCustomName() == "Exit Only Dimension" && o.gameObject.name == "DB_ExitOnlyDimension_Body"))),
                new KeyValuePair<string, Lazy<Component>>("White Hole Station", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.WhiteHole).gameObject.transform.Find("Sector_WhiteHole").GetRequiredComponent<Sector>()._subsectors.First(s => s.gameObject.name == "Sector_WhiteholeStation"))),
                new KeyValuePair<string, Lazy<Component>>("The Interloper", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.Comet))),
                new KeyValuePair<string, Lazy<Component>>("The Interloper - Nomai Shuttle", new Lazy<Component>(() => Locator.GetNomaiShuttle(NomaiShuttleController.ShuttleID.HourglassShuttle))),
                new KeyValuePair<string, Lazy<Component>>("Quantum Moon - Planet", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.QuantumMoon).GetRootSector())),
                new KeyValuePair<string, Lazy<Component>>("Quantum Moon - Quantum Shrine", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.QuantumMoon).GetRootSector().gameObject.transform.Find("QuantumShrine"))),
                new KeyValuePair<string, Lazy<Component>>("Quantum Moon - Quantum Shuttle", new Lazy<Component>(() => Locator.GetNomaiShuttle(NomaiShuttleController.ShuttleID.BrittleHollowShuttle))),
                new KeyValuePair<string, Lazy<Component>>("Quantum Moon - The Eye", new Lazy<Component>(() => Locator.GetAstroObject(AstroObject.Name.QuantumMoon).GetRootSector().gameObject.transform.Find("State_EYE"))),
                new KeyValuePair<string, Lazy<Component>>("Backer Satellite", new Lazy<Component>(() => Locator.GetMinorAstroObject("Backer's Satellite"))),
            };

            KeyValuePair<string, Lazy<Component>>[] eyeParents = new KeyValuePair<string, Lazy<Component>>[]
            {
                new KeyValuePair<string, Lazy<Component>>("Eye - Surface", new Lazy<Component>(() => SectorManager.GetRegisteredSectors().First(s => s.gameObject.name == "Sector_EyeSurface"))),
                new KeyValuePair<string, Lazy<Component>>("Eye - Observatory", new Lazy<Component>(() => SectorManager.GetRegisteredSectors().First(s => s.gameObject.name == "Sector_Observatory"))),
                new KeyValuePair<string, Lazy<Component>>("Eye - Campfire", new Lazy<Component>(() => SectorManager.GetRegisteredSectors().First(s => s.gameObject.name == "Sector_Campfire"))),
            };

            debugAvailableParents = scene == OWScene.SolarSystem ? solarSystemParents : (scene == OWScene.EyeOfTheUniverse ? eyeParents : new KeyValuePair<string, Lazy<Component>>[0]);

            debugCurrentParent = 0;
            debugPoi = null;
            if (debugAvailableParents.Length > 0)
            {
                debugPosition = Vector3.zero;
                debugPoi = new Poi(debugPosition, debugAvailableParents[debugCurrentParent].Value.Value);
                debugPoi.Init(false, false);
                debugPoi.PlaceInScene();
                debugPoi.InfoAnimationEnabled = true;
            }
        }

        private void DebugUpdate()
        {
            if (!isPoisEnabled || debugPoi == null) return; 
            //X, Y, Z and up/down to adjust position
            //P and up/down to adjust parent
            //X, Y, or Z and R to set position to player
            //P and R to set parent to player

            debugPoi.RefreshEnabled(LoadManager.GetCurrentScene());
            debugPoi.Update();

            if ((Keyboard.current.xKey.isPressed || Keyboard.current.yKey.isPressed || Keyboard.current.zKey.isPressed) && (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.leftArrowKey.isPressed))
            {
                float modifier = Keyboard.current.rightArrowKey.isPressed ? 0.1f : -0.1f;
                if (Keyboard.current.xKey.isPressed) debugPosition.x += modifier;
                if (Keyboard.current.yKey.isPressed) debugPosition.y += modifier;
                if (Keyboard.current.zKey.isPressed) debugPosition.z += modifier;
                debugPosition.x = Mathf.Round(debugPosition.x * 10f) / 10f;
                debugPosition.y = Mathf.Round(debugPosition.y * 10f) / 10f;
                debugPosition.z = Mathf.Round(debugPosition.z * 10f) / 10f;
                debugPoi.DebugReposition(debugPosition);
            }
            if (Keyboard.current.pKey.isPressed && (Keyboard.current.rightArrowKey.wasPressedThisFrame || Keyboard.current.leftArrowKey.wasPressedThisFrame))
            {
                debugCurrentParent = (((debugCurrentParent + (Keyboard.current.rightArrowKey.isPressed ? 1 : -1)) % debugAvailableParents.Length) + debugAvailableParents.Length) % debugAvailableParents.Length;
                debugPoi.DebugReparent(debugAvailableParents[debugCurrentParent].Value.Value);
            }
            if ((Keyboard.current.xKey.isPressed || Keyboard.current.yKey.isPressed || Keyboard.current.zKey.isPressed) && Keyboard.current.rKey.wasPressedThisFrame)
            {
                debugPosition = debugAvailableParents[debugCurrentParent].Value.Value.transform.InverseTransformPoint(Locator.GetActiveCamera().transform.position);
                debugPosition.x = Mathf.Round(debugPosition.x * 10f) / 10f;
                debugPosition.y = Mathf.Round(debugPosition.y * 10f) / 10f;
                debugPosition.z = Mathf.Round(debugPosition.z * 10f) / 10f;
                debugPoi.DebugReposition(debugPosition);
            }
            if (Keyboard.current.pKey.isPressed && Keyboard.current.rKey.wasPressedThisFrame)
            {
                int newParentIndex = -1;
                for (int i = 0; i < debugAvailableParents.Length; i++)
                {
                    if (debugAvailableParents[i].Value.Value is AstroObject astroObject && astroObject.GetRootSector().GetOccupants().Any(o => o.GetOccupantType() == DynamicOccupant.Player))
                    {
                        newParentIndex = i;
                    }
                }
                if (newParentIndex != -1)
                {
                    debugCurrentParent = newParentIndex;
                    debugPoi.DebugReparent(debugAvailableParents[debugCurrentParent].Value.Value);
                }
            }
            if (Keyboard.current.xKey.isPressed && Keyboard.current.yKey.isPressed && Keyboard.current.zKey.isPressed && Keyboard.current.pKey.wasPressedThisFrame)
            {
                string[] sectorNames = SectorManager.GetRegisteredSectors().Where(s => s.GetOccupants().Any(o => o.GetOccupantType() == DynamicOccupant.Player)).Select(s => s.gameObject.name).ToArray();
                ModHelper.Console.WriteLine(sectorNames.Length == 0 ? "Player not in any sectors" : $"Player in sector(s): {string.Join(", ", sectorNames)}", MessageType.Message);

                OWCamera camera = Locator.GetActiveCamera();
                if (fragmentCollision != null && fragmentCollision._meshCollider.Raycast(new Ray(camera.transform.position, camera.transform.rotation.eulerAngles), out RaycastHit hitInfo, 50.0f))
                {
                    FragmentIntegrity collidedFragment = fragmentCollision.GetFragmentFromRaycastHit(hitInfo);
                    Transform currentObject = collidedFragment?.gameObject.transform;
                    if (currentObject != null)
                    {
                        string hitName = currentObject.gameObject.name;
                        List<string> hitSectorNames = new List<string>();
                        while (currentObject.parent != null)
                        {
                            Sector sectorComponent = currentObject.gameObject.GetComponent<Sector>();
                            if (sectorComponent != null)
                            {
                                hitSectorNames.Add(currentObject.gameObject.name);
                            }
                            currentObject = currentObject.parent;
                        }
                        ModHelper.Console.WriteLine(hitSectorNames.Count == 0 ? $"Raycast hit fragment {hitName}, which is in no sectors" : $"Raycast hit fragment {hitName}, which is in sector(s) {string.Join(", ", hitSectorNames)}");
                    }
                }
            }
        }

        private void OnGUI()
        {
            if (!isPoisEnabled || debugPoi == null) return;

            GUI.Label(
                new Rect(Screen.width - 300, Screen.height / 2, 300, 200),
                $"X: {debugPosition.x}\nY: {debugPosition.y}\nZ: {debugPosition.z}\nParent: {debugAvailableParents[debugCurrentParent].Key}",
                new GUIStyle() { font = hudFont, fontSize = 18, normal = new GUIStyleState() { textColor = Color.white }, alignment = TextAnchor.UpperLeft, wordWrap = true });
        }

#endif
    }
}

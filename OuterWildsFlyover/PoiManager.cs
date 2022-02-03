using System;
using System.Collections.Generic;
using System.Linq;
using OWML.Common;
using UnityEngine;
using UnityEngine.UI;

namespace OuterWildsFlyover
{
    internal static class PoiManager
    {
        private static OWScene currentScene;
        private static GameObject poiCanvas;
        private static CanvasGroup poiCanvasGroup;
        private static OWAudioSource descSound;
        private static readonly Dictionary<int, Poi> pois = new Dictionary<int, Poi>();
        private static int currentInfo;
        private static float lastTimeSeenPoi = 0.0f;
        private static float infoBoxFadeStart = float.MinValue;

        private static PlayerCameraEffectController camEffectController;
        private static bool isInRemoteCamera = false;

        public static int CurrentInfoId { get { return currentInfo >= 0 ? pois[currentInfo].Id : -1; } }

        public static event EventHandler OnNewPoiCollected;

        public static void SetupInternalObjects()
        {
            //Currently hardcoded, might make the POIs more configurable in the future
            Poi[] poiList = new Poi[]
            {
                new Poi("Sun Station", "Generates power by making the sun go supernova, which made its construction very controversial among the Nomai.", new Vector3(97f, -56f, 0f), OWScene.SolarSystem, new Vector2(-327f, -108f), () => Locator.GetMinorAstroObject("Sun Station"), null),
                new Poi("Sun Station Roof", "I was going to put this one on the window next to the sun, but that made it impossible to collect.", new Vector3(98f, 28f, 0f), OWScene.SolarSystem, new Vector2(-296f, -117f), () => Locator.GetMinorAstroObject("Sun Station"), null),
                new Poi("Hidden Quantum Cave", "This cave is completely sealed off from outside - the only way in is using the Cave Shard.", new Vector3(30f, 80f, -104f), OWScene.SolarSystem, new Vector2(-183f, 243f), () => Locator.GetAstroObject(AstroObject.Name.CaveTwin), null),
                new Poi("High Energy Lab", "The Nomai proved in this lab that the negative time interval during a warp could be increased by increasing the power.", new Vector3(106f, 66f, 94f), OWScene.SolarSystem, new Vector2(-162f, 154f), () => Locator.GetAstroObject(AstroObject.Name.CaveTwin), null),
                new Poi("High Energy Lab Bridge", "This bridge allows the cable to cross he canyon between the High Energy Lab and The Sunless City.", new Vector3(97.5f, 12.5f, 65f), OWScene.SolarSystem, new Vector2(-188f, 142f), () => Locator.GetAstroObject(AstroObject.Name.CaveTwin), null),
                new Poi("Stepping Stone Cave", "The Nomai children used this cave to get to the Anglefish Fossil through a secret hole.", new Vector3(-41.5f, -47.5f, -49f), OWScene.SolarSystem, new Vector2(-275f, 160f), () => Locator.GetAstroObject(AstroObject.Name.CaveTwin), null),
                new Poi("The Sunless City", "Built by the Nomai on Ember Twin, this city had to be constructed underground due to the heat of the sun.", new Vector3(7.5f, -107f, 25f), OWScene.SolarSystem, new Vector2(-258f, 152f), () => Locator.GetAstroObject(AstroObject.Name.CaveTwin), null),
                new Poi("Secret Entrance to The Sunless City", "If you can remember where the hole is, this path provides fast access to The Sunless City.", new Vector3(22f, -115f, -75f), OWScene.SolarSystem, new Vector2(-259f, 180f), () => Locator.GetAstroObject(AstroObject.Name.CaveTwin), null),
                new Poi("Eye Shrine", "A shrine set up by the Nomai dedicated to the Eye of the Universe. They were unsure what the Eye's intentions were.", new Vector3(56.5f, -124.5f, -15f), OWScene.SolarSystem, new Vector2(-250f, 130f), () => Locator.GetAstroObject(AstroObject.Name.CaveTwin), null),
                new Poi("Escape Pod 2", "Crash site of one of the three escape pods launched from the Vessel.", new Vector3(-93f, -99f, 54.5f), OWScene.SolarSystem, new Vector2(-235f, 106f), () => Locator.GetAstroObject(AstroObject.Name.CaveTwin), null),
                new Poi("Gravity Cannon", "Despite the cannon being on a hot planet, the shuttle is covered in ice.", new Vector3(13.5f, -86.5f, -81f), OWScene.SolarSystem, new Vector2(-267f, 201f), () => Locator.GetAstroObject(AstroObject.Name.CaveTwin), null),
                new Poi("Anglerfish Fossil", "As scary as it is, this fossil was a useful resource to the Nomai for finding the Anglerfish's weakness.", new Vector3(-71f, -85.5f, -17f), OWScene.SolarSystem, new Vector2(-287f, 175f), () => Locator.GetAstroObject(AstroObject.Name.CaveTwin), null),
                new Poi("Quantum Moon Locator", "Built by the Nomai to track the Quantum Moon's location. Unlike the Eye locator on the Attlerock, this one actually works.", new Vector3(0f, -186f, 0f), OWScene.SolarSystem, new Vector2(-295f, 137f), () => Locator.GetAstroObject(AstroObject.Name.CaveTwin), null),
                new Poi("Chert's Camp", "Chert has set up camp here to study the stars.", new Vector3(2f, 161f, 2f), OWScene.SolarSystem, new Vector2(-141f, 220f), () => Locator.GetAstroObject(AstroObject.Name.CaveTwin), null),
                new Poi("Lakebed Cave", "Deep down inside the planet, a Nomai went missing in this cave when their lantern went out.", new Vector3(-102f, 51.5f, -74f), OWScene.SolarSystem, new Vector2(-159f, 213f), () => Locator.GetAstroObject(AstroObject.Name.CaveTwin), null),
                new Poi("Sun Station Tower", "There's an even more dangerous way to reach the Sun Station that bypasses this tower entirely.", new Vector3(127.5f, 6f, -79f), OWScene.SolarSystem, new Vector2(-452f, 363f), () => Locator.GetAstroObject(AstroObject.Name.TowerTwin), null),
                new Poi("Ash Twin Project", "A huge Nomai construction, the ATP uses the power from a supernova to send memories 22 minutes into the past.", new Vector3(20f, 13.5f, 0f), OWScene.SolarSystem, new Vector2(-358f, 394f), () => UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().First(o => o.name == "TimeLoopRing_Body").transform, null),
                new Poi("You", "It's... you.", new Vector3(16f, -18f, -2f), OWScene.SolarSystem, new Vector2(-340f, 400f), () => UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().First(o => o.name == "TimeLoopRing_Body").transform, () => PlayerData.GetPersistentCondition("PLAYER_ENTERED_TIMELOOPCORE")),
                new Poi("Village", "The only Hearthian settlement in the Outer Wilds. Like any small village, it has a space program.", new Vector3(6.5f, -10f, 191.5f), OWScene.SolarSystem, new Vector2(-198f, -322f), () => Locator.GetAstroObject(AstroObject.Name.TimberHearth), null),
                new Poi("Observatory", "Contains a collection of interesting artifacts found by other travellers.", new Vector3(-66f, 6f, 218f), OWScene.SolarSystem, new Vector2(-205f, -298f), () => Locator.GetAstroObject(AstroObject.Name.TimberHearth), null),
                new Poi("Backer Graveyard", "Hidden on the edge of the village, this graveyard hold the names of 3 Fig backers who bought a particular backer tier.", new Vector3(-33f, -105f, 173f), OWScene.SolarSystem, new Vector2(-143f, -324f), () => Locator.GetAstroObject(AstroObject.Name.TimberHearth), null),
                new Poi("Zero-G Cave", "This cave gets its name for descending to the centre of the planet, which causes the loss of gravity.", new Vector3(27f, 11.5f, 12.6f), OWScene.SolarSystem, new Vector2(-170f, -379f), () => Locator.GetAstroObject(AstroObject.Name.TimberHearth), null),
                new Poi("Dark Bramble Seed", "Considering it only crashed here recently, this seed has grown at an alarming rate. Tektite is keen to destroy it quickly.", new Vector3(69f, 177f, 23f), OWScene.SolarSystem, new Vector2(-99f, -394f), () => Locator.GetAstroObject(AstroObject.Name.TimberHearth), null),
                new Poi("Quantum Grove", "Somehow that rock is causing everything in this grove to adopt a quantum behaviour.", new Vector3(0f, 10f, 0f), OWScene.SolarSystem, new Vector2(-153f, -429f), () => Locator.GetAstroObject(AstroObject.Name.TimberHearth).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector_QuantumGrove").gameObject.transform.Find("Interactables_QuantumGrove").Find("QuantumShard"), null),
                new Poi("Nomai Mines", "The ore here was used by the Nomai for the Ash Twin Project's protective shell due to its durability.", new Vector3(-71f, 128.5f, -167f), OWScene.SolarSystem, new Vector2(-237f, -436f), () => Locator.GetAstroObject(AstroObject.Name.TimberHearth), null),
                new Poi("First Encounter Mural", "The Nomai first discovered early Hearthian life here, which they described as being \"semi-aquatic and very hardy\".", new Vector3(-35.5f, -3.5f, -121.5f), OWScene.SolarSystem, new Vector2(-216f, -404f), () => Locator.GetAstroObject(AstroObject.Name.TimberHearth), null),
                new Poi("Hearthian Satellite", "Due to its low orbit and small mass, only a slight bump is enough to deorbit it.", new Vector3(0f, 4f, 0f), OWScene.SolarSystem, new Vector2(-39f, -383f), () => Locator.GetAstroObject(AstroObject.Name.TimberHearth).GetSatellite(), null),
                new Poi("Esker's Camp", "Travellers visited here more often when the ships were less sophisticated, but now Esker mostly just talks to ground control.", new Vector3(-27f, 46.5f, -49.5f), OWScene.SolarSystem, new Vector2(-320f, -425f), () => Locator.GetAstroObject(AstroObject.Name.TimberHearth).GetMoon(), null),
                new Poi("Eye Signal Locator", "The Nomai's first eye signal locator. It was unable to detect the Eye's signal, leading them to construct a larger locator.", new Vector3(0f, -78f, 0f), OWScene.SolarSystem, new Vector2(-315f, -452f), () => Locator.GetAstroObject(AstroObject.Name.TimberHearth).GetMoon(), null),
                new Poi("Lunar Lookout", "Used by Esker to keep tabs on the other travellers using his signalscope, due to its great reception.", new Vector3(0f, 81f, 0f), OWScene.SolarSystem, new Vector2(-334f, -405f), () => Locator.GetAstroObject(AstroObject.Name.TimberHearth).GetMoon(), null),
                new Poi("Large Impact Crater", "Chert believes that this crater was created by a piece from Dark Bramble when it imploded.", new Vector3(-6f, 3f, 33.5f), OWScene.SolarSystem, new Vector2(-339f, -464f), () => Locator.GetAstroObject(AstroObject.Name.TimberHearth).GetMoon(), null),
                new Poi("Crashed Alpha Ship", "The ship from the Alpha game somehow crashed here.", new Vector3(48.5f, -12.5f, -56.5f), OWScene.SolarSystem, new Vector2(-347f, -426f), () => Locator.GetAstroObject(AstroObject.Name.TimberHearth).GetMoon(), null),
                new Poi("Riebeck's Camp", "Timber Hearth's only archeologist Riebeck came here to explore the Nomai history and culture, but is terrified of space.", new Vector3(-14f, 160f, 0f), OWScene.SolarSystem, new Vector2(90f, 96f), () => Locator.GetAstroObject(AstroObject.Name.BrittleHollow).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector_Crossroads"), null),
                new Poi("Gravity Crystal Workshop", "The Nomai first designed and built gravity crystals here. Riebeck set up camp here, but has since moved.", new Vector3(-6f, 285f, 12f), OWScene.SolarSystem, new Vector2(91f, 77f), () => Locator.GetAstroObject(AstroObject.Name.BrittleHollow).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector_Crossroads"), null),
                new Poi("Southern Observatory", "The Nomai's second eye signal locator. Despite being more sensitive, it was still unable to detect the eye's signal.", new Vector3(-6f, -296f, -22f), OWScene.SolarSystem, new Vector2(102f, -2f), () => Locator.GetAstroObject(AstroObject.Name.BrittleHollow), null),
                new Poi("Southern Observatory Entrance", "The path is very broken, but since the front door is broken this is the only way in.", new Vector3(10f, -263f, 34f), OWScene.SolarSystem, new Vector2(98f, 24f), () => Locator.GetAstroObject(AstroObject.Name.BrittleHollow), null),
                new Poi("Escape Pod 1", "Crash site of one of the three escape pods launched from the Vessel.", new Vector3(-38f, 362f, -9f), OWScene.SolarSystem, new Vector2(32f, 123f), () => Locator.GetAstroObject(AstroObject.Name.BrittleHollow).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector_EscapePodCrashSite")._subsectors.First(s => s.gameObject.name == "Sector_CrashFragment"), null),
                new Poi("Old Settlement", "A settlement set up by the Nomai upon arrival. Concerns over the location's stability led them to move to the northern glacier.", new Vector3(0f, 244f, 0f), OWScene.SolarSystem, new Vector2(57f, 121f), () => Locator.GetAstroObject(AstroObject.Name.BrittleHollow).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector_OldSettlement")._subsectors.First(s => s.gameObject.name == "Fragment OldSettlement 0"), null),
                new Poi("Tower of Quantum Knowledge", "A tower constructed by the Nomai. Details the north pole rule and the significance of the Quantum Moon pilgrimage.", new Vector3(9f, 233f, -11f), OWScene.SolarSystem, new Vector2(15f, 88f), () => Locator.GetAstroObject(AstroObject.Name.BrittleHollow).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector_QuantumFragment"), null),
                new Poi("Quantum Shard", "The Nomai suspected that this shard is actually a piece from the Quantum Moon.", new Vector3(0.5f, 24.5f, 0f), OWScene.SolarSystem, new Vector2(-13f, 87f), () => Locator.GetAstroObject(AstroObject.Name.BrittleHollow).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector_QuantumFragment").gameObject.transform.Find("Interactables_QuantumFragment").Find("Surface_QRock_Shard").Find("QuantumShard"), null),
                new Poi("Gravity Cannon", "It's difficult, but if you time it right you can use the Nomai shuttle to go to another planet.", new Vector3(0f, 244f, 34f), OWScene.SolarSystem, new Vector2(180f, 106f), () => Locator.GetAstroObject(AstroObject.Name.BrittleHollow).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector_GravityCannon"), null),
                new Poi("The Hanging City", "Home to the inhabitants of Brittle Hollow, this ancient Nomai city is home to the Black Hole Forge.", new Vector3(5f, 181f, 35f), OWScene.SolarSystem, new Vector2(78f, 175f), () => Locator.GetAstroObject(AstroObject.Name.BrittleHollow), null),
                new Poi("Secret Entrance to The Hanging City", "Allows for much faster access to The Hanging City thanks to a convenient hole in the glacier.", new Vector3(-87f, 245f, 0f), OWScene.SolarSystem, new Vector2(47f, 184f), () => Locator.GetAstroObject(AstroObject.Name.BrittleHollow), null),
                new Poi("Eye Shrine", "This shrine to the Eye was created by the Nomai. They knew that the Eye's signal was older than the universe itself.", new Vector3(86f, 206.5f, 3f), OWScene.SolarSystem, new Vector2(103f, 167f), () => Locator.GetAstroObject(AstroObject.Name.BrittleHollow), null),
                new Poi("Northern Glacier", "Above The Hanging City, this glacier is the destination for the White Hole Station's warp.", new Vector3(-18f, 330f, -14f), OWScene.SolarSystem, new Vector2(92f, 200f), () => Locator.GetAstroObject(AstroObject.Name.BrittleHollow), null),
                new Poi("Black Hole Forge", "Poke successfully created the advanced warp core used in the Ash Twin Project here.", new Vector3(0f, 70f, 2f), OWScene.SolarSystem, new Vector2(80f, 155f), () => Locator.GetAstroObject(AstroObject.Name.BrittleHollow).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector_NorthHemisphere")._subsectors.First(s => s.gameObject.name == "Sector_NorthPole")._subsectors.First(s => s.gameObject.name == "Sector_HangingCity").gameObject.transform.Find("Sector_HangingCity_BlackHoleForge").Find("BlackHoleForgePivot"), null),
                new Poi("Hidden Scroll", "When inserted into a Nomai scroll socket, a smiley face and a QR code appears on the translator.", new Vector3(-252f, 75f, -171f), OWScene.SolarSystem, new Vector2(138f, 133f), () => Locator.GetAstroObject(AstroObject.Name.BrittleHollow).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector Northern Gravcanonia")._subsectors.First(s => s.gameObject.name == "Fragment 55_NorthernGravcanonia"), null),
                new Poi("Volcanic Testing Site", "Samples of ore from the mines inside Timber Hearth were tested here by The Nomai for durability.", new Vector3(28f, 98f, 29f), OWScene.SolarSystem, new Vector2(17f, 220f), () => Locator.GetAstroObject(AstroObject.Name.BrittleHollow).GetMoon(), null),
                new Poi("Terminator Nomai", "A half-submerged Nomai skeleton reaching out of the lava - references the ending of Terminator 2: Judgement Day.", new Vector3(84f, 27f, -16f), OWScene.SolarSystem, new Vector2(-10f, 233f), () => Locator.GetAstroObject(AstroObject.Name.BrittleHollow).GetMoon(), null),
                new Poi("Gabbro's Island", "Gabbro and you seem to be the only ones aware of the time loop. He seems fairly relaxed about it though...", new Vector3(-11f, 4f, 41f), OWScene.SolarSystem, new Vector2(180f, -203f), () => Locator.GetAstroObject(AstroObject.Name.GiantsDeep).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector_GabbroIsland"), null),
                new Poi("Statue Island", "The Nomai created the memory statues on this island. Unfortunately the front door to the workshop is broken.", new Vector3(0f, 36f, -25f), OWScene.SolarSystem, new Vector2(340f, -255f), () => Locator.GetAstroObject(AstroObject.Name.GiantsDeep).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector_StatueIsland"), null),
                new Poi("Statue Workshop", "Each memory statue pairs with a single person. It records that person's memories and sends them to the Ash Twin Project.", new Vector3(-6f, 5f, 28f), OWScene.SolarSystem, new Vector2(318f, -262f), () => Locator.GetAstroObject(AstroObject.Name.GiantsDeep).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector_StatueIsland"), null),
                new Poi("Construction Yard", "Each piece of the Orbital Probe Cannon was built here separately. They were assembled together while in orbit.", new Vector3(0f, 13f, 27f), OWScene.SolarSystem, new Vector2(260f, -399f), () => Locator.GetAstroObject(AstroObject.Name.GiantsDeep).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector_ConstructionYard"), null),
                new Poi("Tower of Quantum Trials", "A tower constructed by the Nomai. Details the rule of quantum imaging.", new Vector3(7f, 32.5f, 0f), OWScene.SolarSystem, new Vector2(272f, -144f), () => Locator.GetAstroObject(AstroObject.Name.GiantsDeep).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector_QuantumIsland"), null),
                new Poi("Bramble Island", "Feldspar camped here before leaving to Dark Bramble.", new Vector3(-50f, 24f, 25f), OWScene.SolarSystem, new Vector2(136f, -347f), () => Locator.GetAstroObject(AstroObject.Name.GiantsDeep).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector_BrambleIsland"), null),
                new Poi("Gabbro's Ship", "Hopefully Gabbro can find it when he needs it - it seems to have drifted quite far from his island.", new Vector3(0f, 6f, 8f), OWScene.SolarSystem, new Vector2(321f, -211f), () => Locator.GetAstroObject(AstroObject.Name.GiantsDeep).GetRootSector()._subsectors.First(s => s.gameObject.name == "Sector_GabbroShip"), null),
                new Poi("Control Module", "The only module that's fully intact. Communicates with the Ash Twin Project and controls the Orbital Probe Cannon.", new Vector3(5f, 66f, 0f), OWScene.SolarSystem, new Vector2(369f, -142f), () => Locator.GetAstroObject(AstroObject.Name.ProbeCannon), null),
                new Poi("Launch Module", "It's badly damaged, but luckily the window is broken. This module is responsible for launching the cannon.", new Vector3(-60f, -29f, 0f), OWScene.SolarSystem, new Vector2(393f, -113f), () => Locator.GetAstroObject(AstroObject.Name.ProbeCannon), null),
                new Poi("Probe Tracking Module", "Luckily the module is still working, otherwise it wouldn't have been able to receive the coordinates for the Eye.", new Vector3(-34f, -75.5f, -40f), OWScene.SolarSystem, new Vector2(235f, -293f), () => Locator.GetAstroObject(AstroObject.Name.GiantsDeep), null),
                new Poi("Old Orbital Probe Cannon Component", "The Nomai used the tornados to put the components into orbit. This piece sank instead due the tornado rotating the wrong way.", new Vector3(-26f, -3f, -100f), OWScene.SolarSystem, new Vector2(238f, -321f), () => Locator.GetAstroObject(AstroObject.Name.GiantsDeep), null),
                new Poi("Feldspar's Camp", "After crashing his ship evading an anglerfish, Feldspar has set up camp here. Somehow he finds this place \"peaceful\".", new Vector3(11f, -15f, -2f), OWScene.SolarSystem, new Vector2(439f, 315f), () => Locator.GetMinorAstroObject("Pioneer Dimension"), null),
                new Poi("Frozen Jellyfish", "According to Feldspar it doesn't make a good food source, but it's very good at insulating electricity.", new Vector3(-6f, -562f, -44f), OWScene.SolarSystem, new Vector2(457f, 256f), () => Locator.GetAstroObject(AstroObject.Name.DarkBramble), null),
                new Poi("The Vessel", "The Nomai tried to warp to the Eye, but their Vessel ended up warping inside of Dark Bramble instead.", new Vector3(130f, 11.5f, -14.5f), OWScene.SolarSystem, new Vector2(409f, 339f), () => Locator.GetMinorAstroObject("Vessel Dimension"), null),
                new Poi("The Vessel Basement", "Far too many people don't even realise that this section of the Vessel exists.", new Vector3(160f, 5.5f, -20f), OWScene.SolarSystem, new Vector2(397f, 330f), () => Locator.GetMinorAstroObject("Vessel Dimension"), null),
                new Poi("Escape Pod 3", "This escape pod crashed in the worst possible planet after being launched from the Vessel. ", new Vector3(-470.5f, 135f, 9f), OWScene.SolarSystem, new Vector2(431f, 367f), () => Locator.GetMinorAstroObject("Escape Pod Dimension"), null),
                new Poi("Nomai Grave", "As their air began to run out, the unlucky occupants of Escape Pod 3 ran into this seed while trying to return to The Vessel.", new Vector3(-203f, 426f, -10.5f), OWScene.SolarSystem, new Vector2(448f, 359f), () => Locator.GetMinorAstroObject("Escape Pod Dimension"), null),
                new Poi("Anglerfish Eggs", "I wouldn't like to be here when these eggs hatch…", new Vector3(0f, 0f, 0f), OWScene.SolarSystem, new Vector2(434f, 337f), () => Locator.GetMinorAstroObject("Angler Nest Dimension"), null),
                new Poi("Elsinore Seed", "This seed leads to a strange inaccessible room. It's a reference to a particular ending from the game Elsinore.", new Vector3(-100f, 475f, -60f), OWScene.SolarSystem, new Vector2(460f, 329f), () => Locator._minorAstroObjects.First(o => o.GetCustomName() == "Exit Only Dimension" && o.gameObject.name == "DB_ExitOnlyDimension_Body"), null),
                new Poi("White Hole Station", "The Nomai successfully recreated warp travel with this station. It was used to help build the towers on Ash Twin.", new Vector3(0f, 0f, -10f), OWScene.SolarSystem, new Vector2(539f, 55f), () => Locator.GetAstroObject(AstroObject.Name.WhiteHole).gameObject.transform.Find("Sector_WhiteHole").GetRequiredComponent<Sector>()._subsectors.First(s => s.gameObject.name == "Sector_WhiteholeStation"), null),
                new Poi("Beneath The Ice", "If it wasn't for the ghost matter, these icy tunnels would be quite fun to slide through.", new Vector3(-30f, -35f, 32f), OWScene.SolarSystem, new Vector2(8f, 433f), () => Locator.GetAstroObject(AstroObject.Name.Comet), null),
                new Poi("Nomai Shuttle", "Not quite sure how The Nomai managed to land this thing on The Interloper, but apparently they managed it.", new Vector3(0f, 16f, 0f), OWScene.SolarSystem, new Vector2(101f, 410f), () => Locator.GetNomaiShuttle(NomaiShuttleController.ShuttleID.HourglassShuttle), null),
                new Poi("Ruptured Core", "While it killed off The Nomai, The Hearthian species survived because ghost matter is ineffective in water.", new Vector3(0f, 0f, -5f), OWScene.SolarSystem, new Vector2(41f, 426f), () => Locator.GetAstroObject(AstroObject.Name.Comet), null),
                new Poi("Quantum Moon", "This moon orbits 6 different objects at once until it is observed by a conscious observer, when it collapses into one possibility.", new Vector3(0f, -75f, 0f), OWScene.SolarSystem, new Vector2(326f, -46f), () => Locator.GetAstroObject(AstroObject.Name.QuantumMoon).GetRootSector(), () => Locator.GetQuantumMoon().GetStateIndex() != 5),
                new Poi("Quantum Shrine", "Built by the Nomai, this shrine has an indicator on the wall that shows where the moon is located.", new Vector3(0f, 5f, 0f), OWScene.SolarSystem, new Vector2(330f, -21f), () => Locator.GetAstroObject(AstroObject.Name.QuantumMoon).GetRootSector().gameObject.transform.Find("QuantumShrine"), null),
                new Poi("Quantum Shuttle", "For some reason, this ⓘ point doesn't show up when the shuttle is recalled. Let's just say it's \"because quantum stuff\".", new Vector3(0f, 16f, 0f), OWScene.SolarSystem, new Vector2(306f, -46f), () => Locator.GetNomaiShuttle(NomaiShuttleController.ShuttleID.BrittleHollowShuttle), () => Locator.GetNomaiShuttle(NomaiShuttleController.ShuttleID.BrittleHollowShuttle).transform.IsChildOf(Locator.GetQuantumMoon().transform)),
                new Poi("Solanum", "Thanks to quantum entanglement, Solanum is dead in every location except the sixth location.", new Vector3(-5f, -72f, 12f), OWScene.SolarSystem, new Vector2(322f, -69f), () => Locator.GetAstroObject(AstroObject.Name.QuantumMoon).GetRootSector().gameObject.transform.Find("State_EYE"), () => Locator.GetQuantumMoon().GetStateIndex() == 5),
                new Poi("Fig Backer Satellite", "For $275, Fig backers could buy a special tier to get a video on this secret satellite. ", new Vector3(0f, 10f, 0f), OWScene.SolarSystem, new Vector2(550f, -317f), () => Locator.GetMinorAstroObject("Backer's Satellite"), null),
                new Poi("The Eye of the Universe", "Using technology built by the Nomai hundreds of thousands of years ago, a Hearthian was able to stand on the Eye.", new Vector3(0f, 204f, 25f), OWScene.EyeOfTheUniverse, new Vector2(506.5f, -472f), () => SectorManager.GetRegisteredSectors().First(s => s.gameObject.name == "Sector_EyeSurface"), () => Locator.GetEyeStateManager().GetState() == EyeState.WarpedToSurface),
                new Poi("Quantum Observatory", "Contains an overview of the discoveries made and events that led up to this moment.", new Vector3(7f, 2.5f, -10f), OWScene.EyeOfTheUniverse, new Vector2(506.5f, -438f), () => SectorManager.GetRegisteredSectors().First(s => s.gameObject.name == "Sector_Observatory"), () => Locator.GetEyeStateManager().GetState() == EyeState.Observatory),
                new Poi("Ancient Glade", "It is here where the old universe dies and a new universe is born. The code for this bit calls it the \"Cosmic Jam Session\".", new Vector3(0f, 4f, 0f), OWScene.EyeOfTheUniverse, new Vector2(506.5f, -456f), () => SectorManager.GetRegisteredSectors().First(s => s.gameObject.name == "Sector_Campfire"), () => Locator.GetEyeStateManager().GetState() == EyeState.InstrumentHunt && SectorManager.GetRegisteredSectors().First(s => s.gameObject.name == "Sector_Campfire").gameObject.GetRequiredComponent<QuantumCampsiteController>().AreAnyTravelersGathered() && !SectorManager.GetRegisteredSectors().First(s => s.gameObject.name == "Sector_Campfire").gameObject.GetRequiredComponent<QuantumCampsiteController>().AreAllTravelersGathered() && SectorManager.GetRegisteredSectors().First(s => s.gameObject.name == "Sector_Campfire").gameObject.transform.Find("InflationController").GetRequiredComponent<CosmicInflationController>()._state == CosmicInflationController.State.Inactive),
            };


            Poi.ResetIds();
            pois.Clear();
            foreach (Poi poi in poiList)
            {
                pois.Add(poi.Id, poi);
            }

            currentInfo = -1;
        }

        public static void PlacePoisInScene(OWScene scene, FlyoverSavePoi[] collectedPois)
        {
            currentScene = scene;

            camEffectController = GameObject.FindWithTag("MainCamera").GetRequiredComponent<PlayerCameraEffectController>();
            GlobalMessenger.AddListener("EnterNomaiRemoteCamera", EnableRemoteCamera);
            GlobalMessenger.AddListener("ExitNomaiRemoteCamera", DisableRemoteCamera);

            GameObject screenPromptCanvas = GameObject.FindWithTag("ScreenPromptUI");
            poiCanvas = GameObject.Instantiate(AssetBundleItems.PoiCanvasPrefab, screenPromptCanvas.transform);
            poiCanvasGroup = poiCanvas.GetComponent<CanvasGroup>();
            poiCanvasGroup.alpha = 0;
            poiCanvas.SetActive(false);
            descSound = poiCanvas.transform.Find("Audio").Find("Desc").gameObject.AddComponent<OWAudioSource>();
            descSound._audioLibraryClip = (AudioType)7987701;
            descSound._track = OWAudioMixer.TrackName.Environment_Unfiltered;
            descSound._clipSelectionOnPlay = OWAudioSource.ClipSelectionOnPlay.RANDOM;
            descSound._randomizePlayheadOnAwake = false;
            poiCanvas.SetActive(true);

            foreach (Tuple<int, bool, bool> poiInfo in collectedPois
                .Select(collectedPoi => new Tuple<int, bool, bool>(collectedPoi.Id, !collectedPoi.IsNew, collectedPoi.IsNew))
                .Concat(pois.Values
                    .Where(p => !collectedPois.Any(collectedPoi => collectedPoi.Id == p.Id))
                    .Select(unCollectedPoi => new Tuple<int, bool, bool>(unCollectedPoi.Id, false, false))))
            {
                Poi poi = pois[poiInfo.Item1];
                poi.Init(poiInfo.Item2, poiInfo.Item3);
                try
                {
                    if (poi.Scene == scene) poi.PlaceInScene();
                }
                catch (Poi.PoiSpawnException ex)
                {
                    OuterWildsFlyover.SharedModHelper.Console.WriteLine($"Failed to spawn i-point! Either another mod is causing this mod to break, or the game had an update which broke this mod.\nException details:\n{ex}", MessageType.Error);
                }
                poi.OnPoiCollected += (s, e) =>
                {
                    if (currentInfo >= 0)
                    {
                        SetInfoValue(pois[currentInfo]);
                    }
                };
                poi.OnNewPoiCollected += (s, e) => 
                {
                    OnNewPoiCollected?.Invoke(s, EventArgs.Empty);
                };
            }
        }

        public static void DestroyScene()
        {
            GlobalMessenger.RemoveListener("EnterNomaiRemoteCamera", EnableRemoteCamera);
            GlobalMessenger.RemoveListener("ExitNomaiRemoteCamera", DisableRemoteCamera);
        }

        private static void EnableRemoteCamera() { isInRemoteCamera = true; }
        private static void DisableRemoteCamera() { isInRemoteCamera = false; }

        public static Poi[] GetCollectedPois()
        {
            return pois.Values.Where(poi => poi.CollectState == PoiCollectState.CollectedBefore || poi.CollectState == PoiCollectState.CollectedNow).OrderBy(p => p.CollectOrder).ToArray();
        }

        public static void Update()
        {
            foreach (Poi poi in pois.Values)
            {
                poi.RefreshEnabled(currentScene);
            }

            UpdateInfo();

            foreach (Poi poi in pois.Values)
            {
                poi.Update();
            }
        }

        private static void UpdateInfo()
        {
            if (isInRemoteCamera || AreEyesClosed())
            {
                if (currentInfo >= 0)
                {
                    HideInfo();
                }
            }
            //Otherwise, check if we can show the info
            else
            {
                //Find the eligble Poi
                Poi eligblePoi = null;
                float poiDistance = 50.0f;
                OWCamera currentCamera = Locator.GetActiveCamera();
                foreach (Poi poi in pois.Values.Where(p => p.Enabled))
                {
                    Vector3 viewport = poi.GetViewportPoint();
                    float distance = poi.GetDistanceFromCamera();
                    if (viewport.x >= 0.0f && viewport.x <= 1.0f && viewport.y >= 0.0f && viewport.y <= 1.0f && viewport.z >= 0.0f && distance < poiDistance)
                    {
                        //Check for walls by raycasting (we're doing this now instead of earlier because it's a bit expensive)
                        Ray ray = currentCamera.ViewportPointToRay(viewport);
                        if (!Physics.Raycast(ray, distance, LayerMask.GetMask("Default")))
                        {
                            eligblePoi = poi;
                            poiDistance = distance;
                        }
                    }
                }

                if (eligblePoi != null)
                {
                    lastTimeSeenPoi = Time.time;
                }

                if (eligblePoi != null && eligblePoi.Id != currentInfo)
                {
                    ShowInfo(eligblePoi);
                }
                else if (eligblePoi == null && currentInfo >= 0 && Time.time - lastTimeSeenPoi > 10.0f)
                {
                    HideInfo();
                }
            }

            //Advance the fade animation
            DoInfoFadeAnimation();
        }

        private static bool AreEyesClosed()
        {
            return camEffectController._isOpeningEyes || camEffectController._isClosingEyes || camEffectController._isDying;
        }

        private static void ShowInfo(Poi poi)
        {
            SetInfoValue(poi);
            infoBoxFadeStart = Time.time;

            descSound.PlayDelayed(1.0f);

            poi.InfoAnimationEnabled = true;

            foreach (Poi currentPoi in pois.Values)
            {
                if (poi.Id != currentPoi.Id) currentPoi.InfoAnimationEnabled = false;
            }

            currentInfo = poi.Id;
        }

        private static void SetInfoValue(Poi poi)
        {
            GameObject titlePanel = poiCanvas.transform.Find("PoiTitlePanel").gameObject;
            GameObject descriptionPanel = poiCanvas.transform.Find("PoiDescriptionPanel").gameObject;
            Sprite newSprite = null;
            switch (poi.CollectState)
            {
                case PoiCollectState.Uncollected:
                    newSprite = AssetBundleItems.NewSprite;
                    break;
                case PoiCollectState.CollectedBefore:
                    newSprite = AssetBundleItems.CheckedSprite;
                    break;
                case PoiCollectState.CollectedNow:
                    newSprite = AssetBundleItems.GreySprite;
                    break;
            }
            Image titleImage = titlePanel.transform.Find("PoiTitleImage").GetComponent<Image>();
            titleImage.sprite = newSprite;
            Text titleText = titlePanel.transform.Find("PoiTitle").GetComponent<Text>();
            titleText.text = poi.Name;
            Image titleBg = titlePanel.transform.Find("PoiTitleBackground").GetComponent<Image>();
            titleBg.rectTransform.sizeDelta = new Vector2((titleText.preferredWidth / 2) + 33 + 33, titleBg.rectTransform.sizeDelta.y);
            Text descriptionText = descriptionPanel.transform.Find("PoiDescription").GetComponent<Text>();
            descriptionText.text = poi.Description;
        }

        private static void HideInfo()
        {
            infoBoxFadeStart = Time.time;

            foreach (Poi poi in pois.Values)
            {
                poi.InfoAnimationEnabled = false;
            }

            currentInfo = -1;
        }

        private static void DoInfoFadeAnimation()
        {
            const float duration = 0.25f;
            float timeSinceFade = Time.time - infoBoxFadeStart;
            if (timeSinceFade <= duration)
            {
                if (currentInfo >= 0)
                    poiCanvasGroup.alpha = Mathf.Lerp(0.0f, 1.0f, timeSinceFade / duration);
                else
                    poiCanvasGroup.alpha = Mathf.Lerp(1.0f, 0.0f, timeSinceFade / duration);
            }
            else
            {
                poiCanvasGroup.alpha = currentInfo >= 0 ? 1.0f : 0.0f;
            }
        }
    }
}

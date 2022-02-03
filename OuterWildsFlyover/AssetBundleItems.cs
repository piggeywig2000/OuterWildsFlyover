using OWML.Common;
using UnityEngine;

namespace OuterWildsFlyover
{
    internal static class AssetBundleItems
    {
        public static GameObject PoiObjectPrefab { get; private set; }
        public static GameObject PoiCanvasPrefab { get; private set; }
        public static Material NewMaterial { get; private set; }
        public static Material CheckedMaterial { get; private set; }
        public static Material GreyMaterial { get; private set; }
        public static Sprite NewSprite { get; private set; }
        public static Sprite CheckedSprite { get; private set; }
        public static Sprite GreySprite { get; private set; }
        public static GameObject PoiResultsPrefab { get; private set; }
        public static Sprite ResultsItemEmpty { get; private set; }
        public static Sprite ResultsItemPoi { get; private set; }
        public static GameObject MapNormalPrefab { get; private set; }
        public static GameObject MapNewPrefab { get; private set; }
        public static AudioClip InfoSeeAudio { get; private set; }
        public static AudioClip InfoDescAudio { get; private set; }
        public static AudioClip InfoHitAudio { get; private set; }
        public static AudioClip ResultsMusicAudio { get; private set; }
        public static AudioClip ResultsCountAudio { get; private set; }
        public static AudioClip ResultsCountLastAudio { get; private set; }
        public static AudioClip ResultsItemAudio { get; private set; }
        public static AudioClip ResultsLastItemAudio { get; private set; }
        public static AudioClip ResultsItemClickAudio { get; private set; }
        public static AudioClip ResultsTabClickAudio { get; private set; }
        public static AudioClip ResultsHoverAudio { get; private set; }
        public static AudioClip ResultsButtonClickAudio { get; private set; }

        public static void Init(IModHelper modHelper)
        {
            AssetBundle poiBundle = modHelper.Assets.LoadBundle("poi");

            PoiObjectPrefab = poiBundle.LoadAsset<GameObject>("Assets/Prefabs/Poi.prefab");
            NewMaterial = poiBundle.LoadAsset<Material>("Assets/Materials/New.mat");
            CheckedMaterial = poiBundle.LoadAsset<Material>("Assets/Materials/Checked.mat");
            GreyMaterial = poiBundle.LoadAsset<Material>("Assets/Materials/Grey.mat");
            PoiObjectPrefab.SetActive(false);

            PoiCanvasPrefab = poiBundle.LoadAsset<GameObject>("Assets/Prefabs/PoiCanvas.prefab");
            NewSprite = poiBundle.LoadAsset<Sprite>("Assets/Textures/WS2_pln_info_core_sprite.png");
            CheckedSprite = poiBundle.LoadAsset<Sprite>("Assets/Textures/WS2_pln_info_core_check_sprite.png");
            GreySprite = poiBundle.LoadAsset<Sprite>("Assets/Textures/WS2_pln_info_core_check_grey_sprite.png");
            PoiCanvasPrefab.SetActive(false);

            PoiResultsPrefab = poiBundle.LoadAsset<GameObject>("Assets/Prefabs/PoiResults.prefab");
            ResultsItemEmpty = poiBundle.LoadAsset<Sprite>("Assets/Textures/ResultItemEmpty.png");
            ResultsItemPoi = poiBundle.LoadAsset<Sprite>("Assets/Textures/ResultItemPoi.png");
            PoiResultsPrefab.SetActive(false);

            MapNormalPrefab = poiBundle.LoadAsset<GameObject>("Assets/Prefabs/MapNormal.prefab");
            MapNewPrefab = poiBundle.LoadAsset<GameObject>("Assets/Prefabs/MapNew.prefab");
            MapNormalPrefab.SetActive(false);
            MapNewPrefab.SetActive(false);

            InfoSeeAudio = poiBundle.LoadAsset<AudioClip>("Assets/Sounds/infosee.wav");
            InfoDescAudio = poiBundle.LoadAsset<AudioClip>("Assets/Sounds/infodesc.wav");
            InfoHitAudio = poiBundle.LoadAsset<AudioClip>("Assets/Sounds/infohit.wav");
            ResultsMusicAudio = poiBundle.LoadAsset<AudioClip>("Assets/Sounds/resultsmusic.wav");
            ResultsCountAudio = poiBundle.LoadAsset<AudioClip>("Assets/Sounds/resultscount.wav");
            ResultsCountLastAudio = poiBundle.LoadAsset<AudioClip>("Assets/Sounds/resultscountlast.wav");
            ResultsItemAudio = poiBundle.LoadAsset<AudioClip>("Assets/Sounds/resultsitem.wav");
            ResultsLastItemAudio = poiBundle.LoadAsset<AudioClip>("Assets/Sounds/resultsitemlast.wav");
            ResultsItemClickAudio = poiBundle.LoadAsset<AudioClip>("Assets/Sounds/resultsitemclick.wav");
            ResultsTabClickAudio = poiBundle.LoadAsset<AudioClip>("Assets/Sounds/resultstabclick.wav");
            ResultsHoverAudio = poiBundle.LoadAsset<AudioClip>("Assets/Sounds/resultshover.wav");
            ResultsButtonClickAudio = poiBundle.LoadAsset<AudioClip>("Assets/Sounds/resultsbuttonclick.wav");
        }
    }
}

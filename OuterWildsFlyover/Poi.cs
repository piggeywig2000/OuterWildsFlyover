using System;
using System.Collections.Generic;
using UnityEngine;

namespace OuterWildsFlyover
{
    internal class Poi
    {
        const float FADE_IN_START = 100.0f;
        const float FADE_IN_LENGTH = 25.0f;

        private readonly byte[] whiteRingAnim = { 0, 11, 22, 35, 48, 61, 75, 89, 103, 118, 132, 145, 159, 172, 184, 196, 207, 217, 227, 235, 242, 247, 251, 254, 255, 254, 252, 248, 243, 237, 230, 223, 214, 204, 194, 183, 171, 159, 146, 134, 121, 108, 95, 82, 69, 56, 44, 32, 21, 10 };
        private readonly byte[] blueRing1Anim = { 0, 0, 0, 0, 0, 0, 2, 10, 23, 39, 59, 80, 103, 127, 151, 174, 195, 215, 231, 244, 252, 255, 254, 251, 247, 241, 234, 226, 217, 207, 196, 184, 172, 160, 147, 134, 120, 107, 94, 82, 70, 58, 47, 37, 28, 20, 13, 7, 3, 0 };
        private readonly byte[] blueRing2Anim = { 0, 3, 12, 26, 44, 66, 89, 114, 140, 165, 188, 210, 228, 242, 251, 255, 253, 250, 244, 237, 228, 217, 206, 193, 179, 165, 150, 135, 119, 104, 89, 75, 61, 48, 37, 26, 17, 10, 4, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        private static int nextId = 0;
        private static int nextCollectOrder = 0;
        private static int nextCycleCollectOrder = 0;

        private Func<Component> getParentTransform;
        private Func<bool> getIsEnabled;
        private GameObject gameObject = null;
        private OWAudioSource seeSound;
        private OWAudioSource hitSound;
        private MeshRenderer centerRenderer;
        private MeshRenderer whiteringLargeRenderer;
        private MeshRenderer whiteringRenderer;
        private MeshRenderer bluering1Renderer;
        private MeshRenderer bluering2Renderer;

        private Queue<OWAudioSource> soundsToPlay = new Queue<OWAudioSource>();

        public int Id { get; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        private PoiCollectState _collectState = PoiCollectState.Uncollected;
        public PoiCollectState CollectState
        {
            get { return _collectState; }
            set
            {
                if ((value == PoiCollectState.CollectedBefore || value == PoiCollectState.CollectedNow) && CollectState == PoiCollectState.Uncollected) CollectOrder = nextCollectOrder++;
                if (value == PoiCollectState.CollectedNow && CollectState != PoiCollectState.CollectedNow) CycleCollectOrder = nextCycleCollectOrder++;
                _collectState = value;
                if (IsPlaced) UpdateCenterCollectTexture();
            }
        }
        public PoiCollectState InitialCollectState { get; private set; }
        public int CollectOrder { get; private set; }
        public int CycleCollectOrder { get; private set; }
        public Vector3 Position { get; private set; }
        public OWScene Scene { get; private set; }
        public Vector2 MapPosition { get; private set; }
        public bool Enabled { get; private set; }
        public bool IsPlaced { get; private set; }

        private bool isDebug = false;

        private float animationStartTime = 0.0f;
        private bool infoAnimationEnabled = false;
        public bool InfoAnimationEnabled
        {
            get { return infoAnimationEnabled; }
            set
            {
                if (value && !infoAnimationEnabled)
                {
                    animationStartTime = Time.time;
                    if (IsPlaced && !isDebug) soundsToPlay.Enqueue(seeSound);
                }
                infoAnimationEnabled = value;
            }
        }

        public event EventHandler OnNewPoiCollected;
        public event EventHandler OnPoiCollected;

        public class PoiSpawnException : Exception
        {
            public int Id { get; private set; }

            public PoiSpawnException(int id, Exception innerException) : base($"Failed to place Poi {id}", innerException)
            {
                Id = id;
            }
        }

        public Poi(string name, string description, Vector3 position, OWScene scene, Vector2 mapPosition, Func<Component> getParentTransform, Func<bool> getIsEnabled)
        {
            Id = nextId++;
            Name = name;
            Description = description;
            CollectOrder = int.MaxValue;
            CycleCollectOrder = int.MaxValue;
            Position = position;
            Scene = scene;
            MapPosition = mapPosition;
            Enabled = false;
            IsPlaced = false;
            this.getParentTransform = getParentTransform;
            this.getIsEnabled = getIsEnabled;
        }

        public Poi(Vector3 position, Component parent)
        {
            isDebug = true;
            Id = -1;
            Name = "Debug";
            Description = "Debug";
            CollectOrder = int.MaxValue;
            CycleCollectOrder = int.MaxValue;
            Position = position;
            Scene = LoadManager.GetCurrentScene();
            MapPosition = Vector2.zero;
            Enabled = false;
            IsPlaced = false;
            getParentTransform = () => parent;
            getIsEnabled = () => true;
        }

        public static void ResetIds()
        {
            nextId = 0;
            nextCollectOrder = 0;
            nextCycleCollectOrder = 0;
        }

        public void Init(bool isCollectedBefore, bool isCollectedNow)
        {
            CollectState = isCollectedNow ? PoiCollectState.CollectedNow : (isCollectedBefore ? PoiCollectState.CollectedBefore : PoiCollectState.Uncollected);
            InitialCollectState = isCollectedBefore ? CollectState : PoiCollectState.Uncollected;
        }

        public void PlaceInScene()
        {
            Transform parentTransform = null;
            try
            {
                parentTransform = getParentTransform().transform;
                if (parentTransform == null) throw new Exception("getParentTransform() did not throw an exception, but its transform is null");
            }
            catch (Exception ex)
            {
                throw new PoiSpawnException(Id, ex);
            }
            gameObject = GameObject.Instantiate(AssetBundleItems.PoiObjectPrefab, parentTransform);
            gameObject.transform.localPosition = Position;
            gameObject.SetActive(false);
            seeSound = gameObject.transform.Find("Audio").Find("See").gameObject.AddComponent<OWAudioSource>();
            hitSound = gameObject.transform.Find("Audio").Find("Hit").gameObject.AddComponent<OWAudioSource>();
            seeSound._audioLibraryClip = (AudioType)7987700;
            hitSound._audioLibraryClip = (AudioType)7987702;
            seeSound._track = OWAudioMixer.TrackName.Environment_Unfiltered;
            hitSound._track = OWAudioMixer.TrackName.Environment_Unfiltered;
            seeSound._clipSelectionOnPlay = OWAudioSource.ClipSelectionOnPlay.RANDOM;
            hitSound._clipSelectionOnPlay = OWAudioSource.ClipSelectionOnPlay.RANDOM;
            seeSound._randomizePlayheadOnAwake = false;
            hitSound._randomizePlayheadOnAwake = false;

            centerRenderer = gameObject.transform.Find("center").GetComponent<MeshRenderer>();
            whiteringLargeRenderer = gameObject.transform.Find("whitering-large").GetComponent<MeshRenderer>();
            whiteringRenderer = gameObject.transform.Find("whitering").GetComponent<MeshRenderer>();
            bluering1Renderer = gameObject.transform.Find("bluering-1").GetComponent<MeshRenderer>();
            bluering2Renderer = gameObject.transform.Find("bluering-2").GetComponent<MeshRenderer>();

            IsPlaced = true;
            UpdateCenterCollectTexture();
        }

        public Vector3 GetViewportPoint()
        {
            OWCamera currentCamera = Locator.GetActiveCamera();
            return currentCamera.WorldToViewportPoint(gameObject.transform.position);
        }

        public float GetDistanceFromCamera()
        {
            OWCamera currentCamera = Locator.GetActiveCamera();
            return Vector3.Distance(gameObject.transform.position, currentCamera.transform.position);
        }

        public float GetDistanceFromPlayer()
        {
            Transform playerTransform = Locator.GetPlayerTransform();
            return Vector3.Distance(gameObject.transform.position, playerTransform.position);
        }

        private void UpdateCenterCollectTexture()
        {
            Material newMaterial = null;
            switch (CollectState)
            {
                case PoiCollectState.Uncollected:
                    newMaterial = AssetBundleItems.NewMaterial;
                    break;
                case PoiCollectState.CollectedBefore:
                    newMaterial = AssetBundleItems.CheckedMaterial;
                    break;
                case PoiCollectState.CollectedNow:
                    newMaterial = AssetBundleItems.GreyMaterial;
                    break;
            }
            centerRenderer.material = newMaterial;
        }

        public void RefreshEnabled(OWScene currentScene)
        {
            Enabled = IsPlaced && currentScene == Scene && !Locator.GetDeathManager().IsPlayerDead() && (getIsEnabled == null || getIsEnabled());
        }

        public void Update()
        {
            if (Enabled)
            {
                CheckForCollect();
                LookAtPlayer();
                float alpha = SetDistanceFade();
                DoRingAnimation(alpha);
                PlaySounds();
            }
            else if (IsPlaced)
            {
                gameObject.SetActive(false);
            }
        }

        private void CheckForCollect()
        {
            if (isDebug) return;
            //If we're really close to it, collect it
            if (_collectState != PoiCollectState.CollectedNow && GetDistanceFromPlayer() <= 2.0f)
            {
                //Trigger event if this is previously uncollected
                PoiCollectState oldCollectState = CollectState;
                CollectState = PoiCollectState.CollectedNow;

                soundsToPlay.Enqueue(hitSound);

                OnPoiCollected?.Invoke(this, EventArgs.Empty);
                if (oldCollectState == PoiCollectState.Uncollected)
                {
                    OnNewPoiCollected?.Invoke(this, EventArgs.Empty);
                }
                NotificationManager.SharedInstance.PostNotification(new NotificationData(NotificationTarget.All, "i-point collected"));
            }
        }

        private void LookAtPlayer()
        {
            //We actually look 2 units behind the player to prevent it whipping round when the player tries to go through it
            OWCamera currentCamera = Locator.GetActiveCamera();
            gameObject.transform.LookAt(currentCamera.transform.position + currentCamera.transform.TransformDirection(Vector3.back * 2.0f), currentCamera.transform.TransformDirection(Vector3.up));
        }

        private float SetDistanceFade()
        {
            float distance = GetDistanceFromCamera();
            float alpha = Mathf.Min(Mathf.Lerp(1.0f, 0.0f, (distance - FADE_IN_START + FADE_IN_LENGTH) / FADE_IN_LENGTH), DampenAlpha(Mathf.Lerp(0.0f, 1.0f, (distance - 2.0f) / 4.0f)));
            gameObject.SetActive(alpha > 0.0f);
            centerRenderer.material.SetAlpha(alpha);
            return alpha;
        }

        private void DoRingAnimation(float parentAlpha)
        {
            if (!infoAnimationEnabled)
            {
                gameObject.transform.Find("whitering-large").gameObject.SetActive(false);
                gameObject.transform.Find("whitering").gameObject.SetActive(false);
                gameObject.transform.Find("bluering-1").gameObject.SetActive(false);
                gameObject.transform.Find("bluering-2").gameObject.SetActive(false);
            }
            else
            {
                gameObject.transform.Find("whitering").gameObject.SetActive(true);
                gameObject.transform.Find("bluering-1").gameObject.SetActive(true);
                gameObject.transform.Find("bluering-2").gameObject.SetActive(true);

                const float animationLength = 2.0f;
                float timeSinceStart = Time.time - animationStartTime;
                float t = (timeSinceStart % animationLength) / animationLength;

                //Large white ring
                if (timeSinceStart / animationLength <= 0.5f)
                {
                    gameObject.transform.Find("whitering-large").transform.localScale = Vector3.Lerp(Vector3.zero, new Vector3(200.0f, 200.0f, 100.0f), t * 2.0f);
                    whiteringLargeRenderer.material.SetAlpha(Mathf.Min(parentAlpha, InterpolateAnim(t * 2.0f, whiteRingAnim, true)));
                    whiteringLargeRenderer.material.SetFloat("_Cutoff", 1.0f - InterpolateAnim(t * 2.0f, whiteRingAnim, false));
                    gameObject.transform.Find("whitering-large").gameObject.SetActive(true);
                }
                else
                {
                    gameObject.transform.Find("whitering-large").gameObject.SetActive(false);
                }

                //White ring
                gameObject.transform.Find("whitering").transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * 100.0f, t);
                whiteringRenderer.material.SetAlpha(Mathf.Min(parentAlpha, InterpolateAnim(t, whiteRingAnim, true)));
                whiteringRenderer.material.SetFloat("_Cutoff", 1.0f - InterpolateAnim(t, whiteRingAnim, false));

                float blueRingRot = 360.0f * ((timeSinceStart % (animationLength * 5.0f)) / (animationLength * 5.0f));
                //Blue ring 1
                gameObject.transform.Find("bluering-1").transform.localRotation = Quaternion.Euler(0.0f, 0.0f, blueRingRot);
                bluering1Renderer.material.SetAlpha(Mathf.Min(parentAlpha, InterpolateAnim(t, blueRing1Anim, true) / 8.0f));
                bluering1Renderer.material.SetFloat("_Cutoff", 1.0f - InterpolateAnim(t, blueRing1Anim, false));

                //Blue ring 2
                gameObject.transform.Find("bluering-2").transform.localRotation = Quaternion.Euler(0.0f, 0.0f, blueRingRot);
                gameObject.transform.Find("bluering-2").transform.localScale = Vector3.Lerp(Vector3.one * 70.0f, Vector3.one * 100.0f, t);
                bluering2Renderer.material.SetAlpha(Mathf.Min(parentAlpha, InterpolateAnim(t, blueRing2Anim, true) / 24.0f));
                bluering2Renderer.material.SetFloat("_Cutoff", 1.0f - InterpolateAnim(t, blueRing2Anim, false));
            }
        }

        private float InterpolateAnim(float t, byte[] animPoints, bool dampen)
        {
            float pointDeltaTime = 1.0f / ((float)whiteRingAnim.Length);
            float prev = (float)(animPoints[Mathf.FloorToInt(t / pointDeltaTime)]) / 255.0f;
            float next = (float)(animPoints[Mathf.CeilToInt(t / pointDeltaTime) % whiteRingAnim.Length]) / 255.0f;
            float lerpT = t % pointDeltaTime;
            float result = prev + ((next - prev) * lerpT);
            return dampen ? DampenAlpha(result) : result;
        }

        private float DampenAlpha(float input)
        {
            return 1.0f / ((-10.0f * input) + 11f) - (0.09f);
        }

        private void PlaySounds()
        {
            while (soundsToPlay.Count > 0)
            {
                soundsToPlay.Dequeue().Play();
            }
        }

        public void DebugReposition(Vector3 position)
        {
            if (!isDebug) throw new InvalidOperationException("Poi is not debug");
            Position = position;
            gameObject.transform.localPosition = Position;
        }

        public void DebugReparent(Component newParent)
        {
            if (!isDebug) throw new InvalidOperationException("Poi is not debug");
            getParentTransform = () => newParent;
            gameObject.transform.SetParent(getParentTransform().transform, false);
        }
    }

    internal enum PoiCollectState
    {
        Uncollected,
        CollectedBefore,
        CollectedNow
    }
}

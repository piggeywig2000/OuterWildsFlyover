using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace OuterWildsFlyover
{
    internal static class ResultsManager
    {
        private static GameObject gameObject;
        private static Camera uiCamera;
        private static OWCamera camera;
        private static GameObject particleCamera;
        private static Image blackFade;
        private static Transform rootCanvas;

        private static RectTransform baseCanvasTransform;
        private static CanvasGroup baseCanvasGroup;
        private static GameObject mapEyeContainer;
        private static Transform mapItemsContainer;
        private static GameObject scoreContainer;
        private static RectTransform scoreContainerTransform;
        private static Text scoreText;
        private static CanvasGroup selectedItemGroup;
        private static Text selectedItemText;
        private static GameObject pagesContainer;
        private static Image pagesBackground;
        private static Image[] pagesPanels;
        private static RectTransform[] tabTransforms;
        private static Image[] tabShadows;
        private static GameObject itemsContainer;
        private static RectTransform[] itemTransforms;
        private static Image[] itemImages;
        private static RectTransform frameTransform;
        private static RectTransform buttonTransform;
        private static CanvasGroup buttonCanvasGroup;
        private static Image buttonNormalBackground;
        private static Image buttonFocusBackground;
        private static Text buttonText;

        private static RectTransform messageCanvasTransform;
        private static CanvasGroup messageCanvasGroup;
        private static RectTransform messageButtonTransform;
        private static Image messageButtonImage;

        private static OWAudioSource musicSound;
        private static OWAudioSource effectsSound;

        private enum ResultsState
        {
            Inactive,
            FadeIn,
            OpeningScore,
            CountingScore,
            ButtonAppearScore,
            Score,
            ButtonDisappearScore,
            ClosingScore,
            OpeningList,
            AddingList,
            ButtonAppearList,
            List,
            ButtonDisappearList,
            ClosingList,
            OpeningMessage,
            Message,
            ClosingMessage,
            FadeOut,
            Loading
        };
        private static ResultsState currentState = ResultsState.Inactive;
        private static float timeCurrentState = 0.0f;

        private static Poi[] collectedPois;
        private static GameObject[] mapItemHalos;

        public static event EventHandler OnCompletedResults;

        public static void SetupScene()
        {
            //Remove the Flashback's event listener and add my own
            GlobalMessenger.RemoveListener("TriggerFlashback", UnityEngine.SceneManagement.SceneManager.GetActiveScene()
                .GetRootGameObjects().First(o => o.name == "FlashbackCamera").GetRequiredComponent<Flashback>().OnTriggerFlashback);
            GlobalMessenger.AddListener("TriggerFlashback", OnTriggerFlashback);

            //Root objects and cameras
            gameObject = GameObject.Instantiate(AssetBundleItems.PoiResultsPrefab);
            gameObject.transform.localPosition = new Vector3(0, 0, 0);
            gameObject.SetActive(false);
            Transform cameraObject = gameObject.transform.Find("ResultsCamera");
            uiCamera = cameraObject.GetRequiredComponent<Camera>();
            camera = cameraObject.gameObject.AddComponent<OWCamera>();
            camera.useFarCamera = false;
            camera.renderSkybox = true;
            cameraObject.gameObject.SetActive(false);
            particleCamera = cameraObject.Find("ResultsParticleCamera").gameObject;
            particleCamera.SetActive(false);
            rootCanvas = gameObject.transform.Find("ResultsCanvas");
            blackFade = rootCanvas.Find("ResultsCameraFade").gameObject.GetRequiredComponent<Image>();

            //Base canvas
            Transform baseCanvas = rootCanvas.Find("ResultsBaseCanvas");
            baseCanvasTransform = baseCanvas.GetRequiredComponent<RectTransform>();
            baseCanvasGroup = baseCanvas.GetRequiredComponent<CanvasGroup>();
            Transform mapContainer = baseCanvas.Find("ResultsMapContainer");
            mapEyeContainer = mapContainer.Find("ResultsMapEye").gameObject;
            mapItemsContainer = mapContainer.Find("ResultsMapItems");
            scoreContainer = baseCanvas.Find("ResultsScore").gameObject;
            Transform scoreTextContainerTransform = scoreContainer.transform.Find("ResultsScoreTextContainer");
            scoreContainerTransform = scoreTextContainerTransform.GetRequiredComponent<RectTransform>();
            scoreText = scoreTextContainerTransform.Find("ResultsScoreText").GetRequiredComponent<Text>();
            selectedItemGroup = baseCanvas.Find("ResultsSelectedItem").GetRequiredComponent<CanvasGroup>();
            selectedItemText = selectedItemGroup.transform.Find("ResultsSelectedItemTextContainer").Find("ResultsSelectedItemText").GetRequiredComponent<Text>();
            pagesContainer = baseCanvas.Find("ResultsPage").gameObject;
            pagesBackground = pagesContainer.transform.Find("ResultsPageBackground").GetRequiredComponent<Image>();
            pagesPanels = new Image[]
            {
                pagesContainer.transform.Find("ResultsPagePanel0").GetRequiredComponent<Image>(),
                pagesContainer.transform.Find("ResultsPagePanel1").GetRequiredComponent<Image>(),
                pagesContainer.transform.Find("ResultsPagePanel2").GetRequiredComponent<Image>(),
                pagesContainer.transform.Find("ResultsPagePanel3").GetRequiredComponent<Image>()
            };
            Transform[] tabs = new Transform[]
            {
                pagesContainer.transform.Find("ResultsTab0"),
                pagesContainer.transform.Find("ResultsTab1")

            };
            tabTransforms = tabs.Select(t => t.GetRequiredComponent<RectTransform>()).ToArray();
            tabShadows = tabs.Select(t => t.Find("ResultsTabShadow").GetRequiredComponent<Image>()).ToArray();
            itemsContainer = baseCanvas.Find("ResultsItems").gameObject;
            itemTransforms = itemsContainer.GetComponentsInChildren<RectTransform>(false).Where(t => t.gameObject.name != "ResultsItems").ToArray();
            itemImages = itemsContainer.GetComponentsInChildren<Image>(false).Where(t => t.gameObject.name != "ResultsItems").ToArray();
            frameTransform = itemsContainer.transform.Find("Poi0").Find("PoiFrame").GetRequiredComponent<RectTransform>();
            Transform button = baseCanvas.Find("ResultsButton");
            buttonTransform = button.GetRequiredComponent<RectTransform>();
            buttonCanvasGroup = button.GetRequiredComponent<CanvasGroup>();
            buttonNormalBackground = button.Find("ResultsButtonBackground").GetRequiredComponent<Image>();
            buttonFocusBackground = button.Find("ResultsButtonBackgroundHover").GetRequiredComponent<Image>();
            buttonText = button.Find("ResultsButtonTextContainer").Find("ResultsButtonText").GetRequiredComponent<Text>();

            //Message canvas
            Transform messageCanvas = rootCanvas.Find("ResultsMessageCanvas");
            messageCanvasTransform = messageCanvas.GetRequiredComponent<RectTransform>();
            messageCanvasGroup = messageCanvas.GetRequiredComponent<CanvasGroup>();
            GameObject messageButton = messageCanvas.Find("ResultsMessageABtn").gameObject;
            messageButtonTransform = messageButton.GetRequiredComponent<RectTransform>();
            messageButtonImage = messageButton.GetRequiredComponent<Image>();

            //Audio
            musicSound = gameObject.transform.Find("Audio").Find("Music").gameObject.AddComponent<OWAudioSource>();
            effectsSound = gameObject.transform.Find("Audio").Find("Effects").gameObject.AddComponent<OWAudioSource>();
            musicSound._audioLibraryClip = (AudioType)7987703;
            effectsSound._audioLibraryClip = (AudioType)7987704;
            musicSound._track = OWAudioMixer.TrackName.Death;
            effectsSound._track = OWAudioMixer.TrackName.Death;
            musicSound._clipSelectionOnPlay = OWAudioSource.ClipSelectionOnPlay.RANDOM;
            effectsSound._clipSelectionOnPlay = OWAudioSource.ClipSelectionOnPlay.RANDOM;
            musicSound._randomizePlayheadOnAwake = false;
            effectsSound._randomizePlayheadOnAwake = false;

            gameObject.SetActive(true);
        }

        public static void DestroyScene()
        {
            GlobalMessenger.RemoveListener("TriggerFlashback", OnTriggerFlashback);
        }

        private static void OnTriggerFlashback()
        {
            //Deactivate universe
            if (PlayerData.GetPersistentCondition("GAME_OVER_LAST_SAVE"))
            {
                PlayerData.SetPersistentCondition("GAME_OVER_LAST_SAVE", false);
            }
            CenterOfTheUniverse.DeactivateUniverse();
            Locator.GetActiveCamera().enabled = false;

            //Setup scene objects
            ApplyAlphaModRecursive(rootCanvas);
            collectedPois = PoiManager.GetCollectedPois();
            mapEyeContainer.SetActive(collectedPois.Where(p => p.Scene == OWScene.EyeOfTheUniverse).Any(p => p.CollectState == PoiCollectState.CollectedBefore || p.CollectState == PoiCollectState.CollectedNow));
            SetCameraAngle();

            //Switch camera
            camera.gameObject.SetActive(true);
            GlobalMessenger<OWCamera>.FireEvent("SwitchActiveCamera", camera);

            //Enable updates
            currentState = ResultsState.Inactive;
            NextState();

            //Start loading
            GlobalMessenger.FireEvent("FlashbackStart");
            LoadManager.ReloadSceneAsync(false, false);
        }

        private static void ApplyAlphaModRecursive(Transform rootGameObject)
        {
            if (rootGameObject.TryGetComponent<Image>(out Image image))
            {
                image.color = new Color(image.color.r, image.color.g, image.color.b, ModAlpha(image.color.a));
            }
            if (rootGameObject.TryGetComponent<CanvasGroup>(out CanvasGroup group))
            {
                group.alpha = ModAlpha(group.alpha);
            }

            foreach (Transform child in rootGameObject)
            {
                ApplyAlphaModRecursive(child);
            }
        }

        private static float ModAlpha(float alpha)
        {
            return (0.25f * Mathf.Pow(alpha, 3)) + (0.75f * Mathf.Pow(alpha, 2));
        }

        static bool hasKilledPlayer = true;
        public static void Update()
        {
            if (!hasKilledPlayer)
            {
                hasKilledPlayer = true;
                Locator.GetDeathManager().KillPlayer(DeathType.Meditation);
                return;
            }
            if (currentState == ResultsState.Inactive) return;
            timeCurrentState += Time.deltaTime;

            CheckForMusicLoop();

            WaitForLoading();
            SetCameraAngle();
            HandleCameraFadeAnimation();
            HandleCanvasOpen();
            HandleCountScore();
            HandleAddList();
            HandleButtonAppear();
            HandleTabShadow();
            HandleTabMovement();
            HandleItemHover();
            HandleSelectedItemFade();
            HandleButtonHoverAnimation();
            HandleButtonDisappear();
            HandleCanvasClose();
            HandleMessageOpen();
            HandleMessageClose();
            HandleMessageAAnimation();
        }

        private static void NextState()
        {
            currentState = (ResultsState)((int)currentState + 1);
            timeCurrentState = 0.0f;

            if (currentState == ResultsState.FadeIn)
            {
                blackFade.color = new Color(0, 0, 0, 1);
                OWInput.ChangeInputMode(InputMode.None);

                baseCanvasGroup.alpha = 0;
                messageCanvasGroup.alpha = 0;

                musicSound.timeSamples = 0;
                musicSound.Play();
            }
            else if (currentState == ResultsState.OpeningScore || currentState == ResultsState.OpeningList)
            {
                ClearMap();
                particleCamera.SetActive(true);
                scoreContainer.SetActive(currentState == ResultsState.OpeningScore);
                pagesContainer.SetActive(currentState == ResultsState.OpeningList);
                itemsContainer.SetActive(currentState == ResultsState.OpeningList);
                selectedItemGroup.alpha = 0.0f;
                buttonText.text = currentState == ResultsState.OpeningScore ? "Next" : "Close";
                buttonCanvasGroup.alpha = 0.0f;
                buttonNormalBackground.color = new Color(1, 1, 1, 1);
                buttonFocusBackground.color = new Color(1, 1, 1, 0);

                if (currentState == ResultsState.OpeningList)
                {
                    ChangePage((byte)Math.Min(collectedPois.Count(p => p.InitialCollectState == PoiCollectState.CollectedBefore) / 40, 1), true);
                }
            }
            else if (currentState == ResultsState.AddingList)
            {
                for (int i = 0; i < collectedPois.Length; i++)
                {
                    if (collectedPois[i].InitialCollectState == PoiCollectState.CollectedBefore)
                    {
                        SpawnOnMap(i);
                    }
                }
            }
            else if (currentState == ResultsState.Score || currentState == ResultsState.List)
            {
                OWInput.ChangeInputMode(InputMode.Menu);
            }
            else if (currentState == ResultsState.ButtonDisappearScore || currentState == ResultsState.ButtonDisappearList)
            {
                buttonNormalBackground.color = new Color(1, 1, 1, 0);
                buttonFocusBackground.color = new Color(1, 1, 1, 1);
                OWInput.ChangeInputMode(InputMode.None);
            }
            else if (currentState == ResultsState.ClosingScore || currentState == ResultsState.ClosingList)
            {
                particleCamera.SetActive(false);
            }
            else if (currentState == ResultsState.OpeningMessage)
            {
                //We shouldn't show the message if they don't have less than 80, OR if they don't have any newly collected points and the enter key isn't pressed
                if (collectedPois.Length < 80 || (!collectedPois.Any(p => p.InitialCollectState == PoiCollectState.Uncollected) && !Keyboard.current.enterKey.isPressed))
                {
                    currentState = ResultsState.ClosingMessage;
                    NextState();
                }
            }
            else if (currentState == ResultsState.Message)
            {
                OWInput.ChangeInputMode(InputMode.Menu);
            }
            else if (currentState == ResultsState.ClosingMessage)
            {
                OWInput.ChangeInputMode(InputMode.None);
            }
            else if (currentState == ResultsState.FadeOut)
            {
                blackFade.color = new Color(0, 0, 0, 0);

                //Fade audio
                musicSound.FadeOut(1.0f/3.0f);
            }
            else if (currentState == ResultsState.Loading)
            {
                OnCompletedResults?.Invoke(null, EventArgs.Empty);

                TimeLoop.RestartTimeLoop();
                if (LoadManager.IsAsyncLoadComplete())
                {
                    currentState = ResultsState.Inactive;
                    LoadManager.EnableAsyncLoadTransition();
                }
                else
                {
                    SpinnerUI.Show();
                }
            }
        }

        private static void WaitForLoading()
        {
            if (currentState != ResultsState.Loading) return;

            if (LoadManager.IsAsyncLoadComplete())
            {
                currentState = ResultsState.Inactive;
                LoadManager.EnableAsyncLoadTransition();
                SpinnerUI.Hide();
            }
        }

        private static void SetCameraAngle()
        {
            float t = Time.time;
            float x = Mathf.Sin((t * 2.5f * Mathf.PI) / 90.0f) * 45.0f;
            float y = Mathf.Cos((t * 6.0f * Mathf.PI) / 90.0f) * 90.0f;
            float z = Mathf.Sin((t * 6.0f * Mathf.PI) / 90.0f) * 30.0f;
            gameObject.transform.localEulerAngles = new Vector3(x, y, z);
        }

        private static void CheckForMusicLoop()
        {
            if (currentState == ResultsState.Inactive || currentState == ResultsState.FadeOut || currentState == ResultsState.Loading) return;

            //We restart when we've got 1 second to go, to give us a bit of a buffer in case of powerpoint fps
            if (musicSound.timeSamples >= 1468320 - 32000)
            {
                musicSound.timeSamples = musicSound.timeSamples - 1181600;
            }
        }

        private static void HandleCameraFadeAnimation()
        {
            //1 second fade in
            if (currentState == ResultsState.FadeIn)
            {
                blackFade.color = new Color(0, 0, 0, ModAlpha(Mathf.Lerp(1, 0, timeCurrentState)));
                if (timeCurrentState >= 1.0f) NextState();
            }
            //0.333 second fade out
            if (currentState == ResultsState.FadeOut)
            {
                blackFade.color = new Color(0, 0, 0, ModAlpha(Mathf.Lerp(0, 1, timeCurrentState * 3.0f)));
                if (timeCurrentState >= 1.0f/3.0f) NextState();
            }
        }

        private static void HandleCanvasOpen()
        {
            if (currentState != ResultsState.OpeningScore && currentState != ResultsState.OpeningList) return;
            float width;
            float height;

            if (timeCurrentState <= 0.2)
            {
                width = ((-59.5f) * Mathf.Pow(timeCurrentState - 0.1421f, 2)) + 1.2f;
                height = 0.01f;
            }
            else if (timeCurrentState < 0.3)
            {
                width = 1.0f;
                height = 0.01f;
            }
            else
            {
                width = 1.0f;
                height = ((-26.45f) * Mathf.Pow(timeCurrentState - 0.513f, 2)) + 1.1f;
            }

            if (timeCurrentState >= 0.6f)
            {
                width = 1.0f;
                height = 1.0f;
                NextState();
            }

            baseCanvasTransform.localScale = new Vector2(width, height);
            baseCanvasGroup.alpha = 1.0f;
        }

        private static void HandleCountScore()
        {
            if (currentState != ResultsState.CountingScore) return;

            const float NORMAL_SCALE = 1.0f;
            const float MAX_SCALE = 1.125f;
            int maxCount = collectedPois.Count(p => p.CollectState == PoiCollectState.CollectedNow);
            int currentCount = Mathf.FloorToInt(timeCurrentState * 4.0f);

            float scale = NORMAL_SCALE;

            if (currentCount > maxCount + 1)
            {
                NextState();
            }
            else if (currentCount > 0)
            {
                //If we need to change it, change it and play the sound and show the particles
                string newScore = currentCount.ToString();
                if (currentCount <= maxCount && newScore != scoreText.text)
                {
                    scoreText.text = newScore;

                    Poi currentPoi = collectedPois.OrderBy(p => p.CycleCollectOrder).ToArray()[currentCount - 1];
                    SpawnOnMap(currentPoi.CollectOrder);

                    effectsSound.PlayOneShot(currentCount < maxCount ? (AudioType)7987704 : (AudioType)7987705);
                }

                if (currentCount <= maxCount)
                {
                    float animTime = ((timeCurrentState * 4.0f) % 1.0f) * 0.25f;
                    if (animTime < 1.0f / 12.0f)
                        scale = Mathf.Lerp(NORMAL_SCALE, MAX_SCALE, animTime * 12.0f);
                    else
                        scale = Mathf.Lerp(MAX_SCALE, NORMAL_SCALE, (animTime * 12.0f) - 1.0f);
                }
            }

            scoreContainerTransform.localScale = new Vector2(scale, scale);
        }

        private static int lastListPlayNum = 0;
        private static void HandleAddList()
        {
            if (currentState != ResultsState.AddingList) return;

            int maxCount = collectedPois.Length;
            int initialCount = collectedPois.Count(p => p.InitialCollectState == PoiCollectState.CollectedBefore);
            int currentCount = initialCount + Mathf.FloorToInt(timeCurrentState * 4.0f);
            if (maxCount > 40) maxCount += 1;
            if (initialCount > 40)
            {
                initialCount += 1;
                currentCount += 1;
            }
            int currentPoiIndex = currentCount == 0 || currentCount == 41 ? -1 : (currentCount <= 40 ? currentCount - 1 : currentCount - 2);

            if (currentCount > maxCount)
            {
                //Time to move on
                if (currentCount > maxCount + 1)
                    NextState();
            }
            else if (initialCount <= 40 && currentCount == 41)
            {
                //Change page
                if (currentPage == 0)
                    ChangePage(1, true);
            }
            else if (currentCount > initialCount)
            {
                //Iterate over all items setting their image. Remember that second page has indexes +1d
                //int numShownIcons = currentCount <= 40 ? currentCount : currentCount - 41;
                int numShownIcons = currentPoiIndex < 0 ? 0 : (currentPoiIndex % 40) + 1;
                float animTime = ((timeCurrentState * 4.0f) % 1.0f) * 0.25f;

                //Play sound and show the particles if we haven't already
                if (numShownIcons > 0 && lastListPlayNum != numShownIcons)
                {
                    SpawnOnMap(currentPoiIndex);
                    effectsSound.PlayOneShot(currentCount < maxCount ? (AudioType)7987706 : (AudioType)7987707);
                    lastListPlayNum = numShownIcons;
                }

                FillPageWithNItems(numShownIcons);
                for (int i = 0; i < 40; i++)
                {
                    float scale = 1.0f;
                    bool isEmpty = i >= numShownIcons;
                    if (!isEmpty && i == numShownIcons - 1)
                    {
                        scale = Mathf.Lerp(1.5f, 1.0f, animTime * 12.0f);
                    }
                    itemTransforms[i].localScale = new Vector3(scale, scale);
                }
            }
        }

        private static void HandleButtonAppear()
        {
            if (currentState != ResultsState.ButtonAppearScore && currentState != ResultsState.ButtonAppearList) return;

            float width = timeCurrentState <= 0.05f ? (40 * Mathf.Pow(timeCurrentState - 0.05f, 2)) + 1.0f : 1.0f;
            float height = (5420 * Mathf.Pow(timeCurrentState - 0.08f, 3)) + (410 * Mathf.Pow(timeCurrentState - 0.08f, 2)) + 0.85f;
            float alpha = ModAlpha(Mathf.Lerp(0, 1, timeCurrentState * 10));

            if (timeCurrentState >= 0.1f)
            {
                width = 1.0f;
                height = 1.0f;
                alpha = 1.0f;
                NextState();
            }

            buttonTransform.localScale = new Vector2(width, height);
            buttonCanvasGroup.alpha = alpha;
        }

        private static Tuple<bool, float, float>[] itemAnimationStates;
        private static void HandleItemHover()
        {
            if (currentState != ResultsState.OpeningList && currentState != ResultsState.ButtonAppearList && currentState != ResultsState.List && currentState != ResultsState.ButtonDisappearList && currentState != ResultsState.ClosingList) return;

            bool isNewHoverLocked = false;
            for (int i = 0; i < itemAnimationStates.Length; i++)
            {
                bool isHovering = RectTransformUtility.RectangleContainsScreenPoint(itemTransforms[i], Mouse.current.position.ReadValue(), uiCamera);
                (bool wasHovering, _, float clickStartTime) = itemAnimationStates[i];
                if (isHovering && wasHovering && Time.time - clickStartTime > 1.0f/15.0f)
                {
                    isNewHoverLocked = true;
                }
            }

            bool wasItemClicked = false;
            for (int i = 0; i < itemAnimationStates.Length; i++)
            {
                RectTransform itemTransform = itemTransforms[i];
                (bool wasHovering, float animStartTime, float clickStartTime) = itemAnimationStates[i];
                bool isHovering = currentState == ResultsState.List && RectTransformUtility.RectangleContainsScreenPoint(itemTransforms[i], Mouse.current.position.ReadValue(), uiCamera) && (!isNewHoverLocked || wasHovering);
                if (isHovering != wasHovering)
                    animStartTime = Time.time; //Start animation
                if (isHovering && !wasHovering)
                {
                    itemTransforms[i].SetAsLastSibling(); //Bring to last sibling
                }
                float size = Mathf.Lerp(isHovering ? 1.0f : 1.5f, isHovering ? 1.5f : 1.0f, (Time.time - animStartTime) * 20.0f);

                //Check for clicking
                if (currentState == ResultsState.List && isHovering && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    clickStartTime = Time.time;
                    SelectItem((currentPage * 40) + i);
                    effectsSound.PlayOneShot((AudioType)7987708);
                }

                //Update click animation
                if (Time.time - clickStartTime <= 1.0f/15.0f)
                {
                    isHovering = false;
                    animStartTime = -10f;
                    size = Time.time - clickStartTime <= 1.0f / 30.0f ? Mathf.Lerp(1.5f, 0.75f, (Time.time - clickStartTime) * 30.0f) : Mathf.Lerp(0.75f, 1f, ((Time.time - clickStartTime) * 30.0f) - 1.0f);
                    wasItemClicked = true;
                }

                //Play hover SFX
                if (isHovering && !wasHovering)
                {
                    effectsSound.PlayOneShot((AudioType)7987710);
                }

                wasHovering = isHovering;
                itemAnimationStates[i] = new Tuple<bool, float, float>(wasHovering, animStartTime, clickStartTime);
                itemTransform.localScale = new Vector2(size, size);
            }

            if (Mouse.current.leftButton.wasPressedThisFrame && !wasItemClicked)
            {
                DeselectItem();
            }
        }

        private static float timeOnSelectedItemChange = -10f;
        private static void HandleSelectedItemFade()
        {
            if (currentState != ResultsState.List) return;

            selectedItemGroup.alpha = ModAlpha(Mathf.Lerp(frameTransform.gameObject.activeSelf ? 0 : 1, frameTransform.gameObject.activeSelf ? 1 : 0, (Time.time - timeOnSelectedItemChange) * 10.0f));
        }

        private static bool wasButtonHoveringBefore = false;
        private static float timeOnButtonHover = -10.0f;
        private static void HandleButtonHoverAnimation()
        {
            if (currentState != ResultsState.Score && currentState != ResultsState.List) return;

            bool isHovering = timeCurrentState > 0.0f && RectTransformUtility.RectangleContainsScreenPoint(buttonTransform, Mouse.current.position.ReadValue(), uiCamera);
            if (isHovering != wasButtonHoveringBefore)
                timeOnButtonHover = Time.time; //Start animation
            if (isHovering && !wasButtonHoveringBefore)
                effectsSound.PlayOneShot((AudioType)7987710); //Play hover SFX
            float t = Mathf.Lerp(isHovering ? 0 : 1, isHovering ? 1 : 0, (Time.time - timeOnButtonHover) * 20);

            if (isHovering && Mouse.current.leftButton.wasPressedThisFrame)
            {
                t = 1.0f;
                isHovering = false;
                timeOnButtonHover = -10.0f;
                effectsSound.PlayOneShot((AudioType)7987711);
                NextState();
            }
            float size = 1.0f + (t * 0.05f);

            wasButtonHoveringBefore = isHovering;
            buttonTransform.localScale = new Vector2(size, size);
            buttonNormalBackground.color = new Color(1, 1, 1, 1.0f - ModAlpha(t));
            buttonFocusBackground.color = new Color(1, 1, 1, ModAlpha(t));
        }

        private static void HandleButtonDisappear()
        {
            if (currentState != ResultsState.ButtonDisappearScore && currentState != ResultsState.ButtonDisappearList) return;

            float size = timeCurrentState <= 0.1f/3.0f ? Mathf.Lerp(1.05f, 0.925f, timeCurrentState * 30.0f) : Mathf.Lerp(0.925f, 1.0f, (timeCurrentState - (0.1f/3.0f)) * 60.0f);

            if (timeCurrentState >= 0.05f)
            {
                size = 1.0f;
                if (timeCurrentState >= 0.15f)
                    NextState();
            }

            buttonTransform.localScale = new Vector2(size, size);
        }

        private static void HandleCanvasClose()
        {
            if (currentState != ResultsState.ClosingScore && currentState != ResultsState.ClosingList) return;

            float alpha = ModAlpha(Mathf.Lerp(1, 0, timeCurrentState * (20.0f / 3.0f)));
            float size = Mathf.Lerp(1, 0.75f, timeCurrentState * (20.0f/3.0f));

            if (timeCurrentState >= 0.15f)
            {
                alpha = 0;
                size = 1;
                NextState();
            }

            baseCanvasTransform.localScale = new Vector2(size, size);
            baseCanvasGroup.alpha = alpha;
        }

        private static void HandleMessageOpen()
        {
            if (currentState != ResultsState.OpeningMessage) return;

            //0.2s waiting, 0.3s transition, 2.35s waiting, 0.15s button transition
            float alpha = ModAlpha(Mathf.Lerp(0, 1, (timeCurrentState - 0.2f) * (10.0f / 3.0f)));
            float buttonTransition = Mathf.Lerp(0, 1, (timeCurrentState - 2.85f) * (20.0f / 3.0f));

            if (timeCurrentState >= 3)
            {
                alpha = 1;
                buttonTransition = 1;
                NextState();
            }

            messageCanvasGroup.alpha = alpha;
            messageButtonTransform.localScale = new Vector2(buttonTransition * 1.5f, buttonTransition * 1.5f);
            messageButtonImage.color = new Color(1, 1, 1, ModAlpha(buttonTransition * 0.9294118f));
        }

        private static void HandleMessageAAnimation()
        {
            if (currentState != ResultsState.Message) return;

            float t = (timeCurrentState * 2.0f) % 1;
            float size = -Mathf.Pow(Mathf.Abs(t - 0.5f) - 0.5f, 2) + 1.5f;

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                size = 1.5f;
                effectsSound.PlayOneShot((AudioType)7987711);
                NextState();
            }

            messageButtonTransform.localScale = new Vector2(size, size);
        }

        private static void HandleMessageClose()
        {
            if (currentState != ResultsState.ClosingMessage) return;

            float size = timeCurrentState < 0.05f ? Mathf.Lerp(1.5f, 1, timeCurrentState * 20) : Mathf.Lerp(1, 1.5f, (timeCurrentState - 0.05f) * 10);
            Color color = timeCurrentState < 0.15f ?
                Color.Lerp(new Color(1, 1, 1, ModAlpha(0.9294118f)), new Color(1, 0.9f, 0.333f, ModAlpha(0.9294118f)), timeCurrentState * (20.0f / 3.0f)) :
                Color.Lerp(new Color(1, 0.9f, 0.333f, ModAlpha(0.9294118f)), new Color(1, 1, 1, ModAlpha(0.9294118f)), (timeCurrentState - 0.15f) * (60.0f / 11.0f));

            if (timeCurrentState >= 1.0f/3.0f)
            {
                size = 1.5f;
                color = new Color(1, 1, 1, ModAlpha(0.9294118f));
                NextState();
            }

            messageButtonTransform.localScale = new Vector2(size, size);
            messageButtonImage.color = color;
        }

        private static void SpawnOnMap(int collectedPoisIndex)
        {
            Poi poi = collectedPois[collectedPoisIndex];
            bool isNew = poi.InitialCollectState == PoiCollectState.Uncollected;

            //Instantiate object
            GameObject newMapItem = GameObject.Instantiate(isNew ? AssetBundleItems.MapNewPrefab : AssetBundleItems.MapNormalPrefab, mapItemsContainer);
            newMapItem.transform.localPosition = poi.MapPosition;
            mapItemHalos[collectedPoisIndex] = newMapItem.transform.Find("SelectedHalo").gameObject;

            //Disable effects if we're just fading it in
            if (currentState == ResultsState.AddingList && !isNew)
            {
                newMapItem.transform.Find("Sparkle").gameObject.SetActive(false);
                newMapItem.transform.Find("Ring").gameObject.SetActive(false);
            }

            newMapItem.SetActive(true);
        }

        private static void ClearMap()
        {
            mapItemsContainer.DestroyAllChildren();
            mapItemHalos = new GameObject[collectedPois.Length];
        }

        private static byte currentPage = 0;
        private static float timeOnPageChange = -10f;
        private static void ChangePage(byte pageNum, bool useInitialCollectStates)
        {
            currentPage = pageNum;
            timeOnPageChange = Time.time;

            //Change background alpha
            float alpha = ModAlpha(pageNum == 0 ? 26.0f/51.0f : 32.0f/51.0f);
            pagesBackground.color = new Color(pagesBackground.color.r, pagesBackground.color.g, pagesBackground.color.b, alpha);
            foreach (Image panel in pagesPanels)
            {
                panel.color = new Color(panel.color.r, panel.color.g, panel.color.b, alpha);
            }

            DeselectItem();

            //Set images and reset animation state
            FillPageWithNItems(Math.Max((useInitialCollectStates ? collectedPois.Count(p => p.InitialCollectState == PoiCollectState.CollectedBefore) : collectedPois.Length) - (currentPage * 40), 0));
            for (int i = 0; i < 40; i++) itemTransforms[i].localScale = Vector2.one;

            itemAnimationStates = new Tuple<bool, float, float>[GetNumberOfPoisOnPage(pageNum)];
            for (int i = 0; i < itemAnimationStates.Length; i++) itemAnimationStates[i] = new Tuple<bool, float, float>(false, -10f, -10f);
        }

        private static void FillPageWithNItems(int numItems)
        {
            for (int i = 0; i < 40; i++)
            {
                bool isEmpty = i >= numItems;
                itemImages[i].sprite = isEmpty ? AssetBundleItems.ResultsItemEmpty : AssetBundleItems.ResultsItemPoi;
                itemImages[i].color = new Color(1, 1, 1, ModAlpha(isEmpty ? (currentPage == 0 ? 0.55f : 2.0f / 3.0f) : 1));
            }
        }

        private static int GetNumberOfPoisOnPage(byte pageNum)
        {
            return Math.Max(0, Math.Min(collectedPois.Length - (40 * pageNum), 40));
        }

        private static Poi selectedPoi = null;
        private static void SelectItem(int poiIndex)
        {
            //If another item already selected then don't do animation
            timeOnSelectedItemChange = selectedPoi != null ? -10f : Time.time;
            selectedPoi = collectedPois[poiIndex];
            selectedItemText.text = selectedPoi.Name;
            frameTransform.SetParent(itemTransforms[poiIndex % 40], false);
            frameTransform.gameObject.SetActive(true);

            //Update selected halo animation
            for (int i = 0; i < collectedPois.Length; i++)
            {
                if (mapItemHalos[i] != null)
                    mapItemHalos[i].SetActive(i == poiIndex);
            }
        }

        private static void DeselectItem()
        {
            //If another item already selected then don't do animation
            timeOnSelectedItemChange = selectedPoi != null ? Time.time : -10f;
            frameTransform.gameObject.SetActive(false);
            selectedPoi = null;

            //Update selected halo animation
            for (int i = 0; i < collectedPois.Length; i++)
            {
                if (mapItemHalos[i] != null)
                    mapItemHalos[i].SetActive(false);
            }
        }

        private static void HandleTabShadow()
        {
            if (currentState != ResultsState.OpeningList && currentState != ResultsState.AddingList && currentState != ResultsState.ButtonAppearList && currentState != ResultsState.List && currentState != ResultsState.ButtonDisappearList && currentState != ResultsState.ClosingList) return;

            Image currentTab = tabShadows[currentPage];
            currentTab.color = new Color(currentTab.color.r, currentTab.color.g, currentTab.color.b, Mathf.Lerp(0.8f, 0, (Time.time - timeOnPageChange) * 10.0f));

            foreach (Image tab in tabShadows)
            {
                if (tab == currentTab) continue;

                tab.color = new Color(tab.color.r, tab.color.g, tab.color.b, Mathf.Max(Mathf.Lerp(0, 0.8f, (Time.time - timeOnPageChange) * 10.0f), tab.color.a));
            }
        }

        private static Tuple<bool, float>[] tabMovmentStates = new Tuple<bool, float>[]
        {
            new Tuple<bool, float>(false, -10f),
            new Tuple<bool, float>(false, -10f)
        };
        private static void HandleTabMovement()
        {
            if (currentState != ResultsState.OpeningList && currentState != ResultsState.ButtonAppearList && currentState != ResultsState.List && currentState != ResultsState.ButtonDisappearList && currentState != ResultsState.ClosingList) return;

            for (byte i = 0; i < tabTransforms.Length; i++)
            {
                RectTransform tabTransform = tabTransforms[i];
                (bool wasHovering, float animStartTime) = tabMovmentStates[i];

                bool isHovering = currentPage != i && RectTransformUtility.RectangleContainsScreenPoint(tabTransform, Mouse.current.position.ReadValue(), uiCamera);
                if (isHovering != wasHovering)
                    animStartTime = Time.time; //Start animation
                if (isHovering && !wasHovering)
                    effectsSound.PlayOneShot((AudioType)7987710); //Play hover SFX
                float width = Mathf.Lerp(isHovering ? 172 : 224, isHovering ? 224 : 172, (Time.time - animStartTime) * 10.0f);

                if (currentState == ResultsState.List && isHovering && Mouse.current.leftButton.wasPressedThisFrame)
                {
                    ChangePage(i, false);
                    effectsSound.PlayOneShot((AudioType)7987709);
                }

                wasHovering = isHovering;
                tabMovmentStates[i] = new Tuple<bool, float>(wasHovering, animStartTime);
                tabTransform.sizeDelta = new Vector2(width, 396);
            }
        }
    }
}

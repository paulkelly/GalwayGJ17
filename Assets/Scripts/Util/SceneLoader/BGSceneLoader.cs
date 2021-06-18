using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

namespace Billygoat
{
    public class BGSceneLoader : MonoBehaviour
    {
        public GameObject TransitionPrefab;

        public static bool LoadInProgress { get; private set; }
        public static bool LoadQueued { get; private set; }

        private string _currentLevel;
        private static SceneTransition _sceneTransition;

        private static bool _finishedInitOperation = false;

        private static bool _applicationQuitting = false;

        private static bool _waitingForSceneLoad;

        public delegate void SceneLoadStarted(string level);
        public delegate void SceneLoadComplete(string level);

        public delegate void TransitionAnimationFinished();

        public static event SceneLoadStarted OnSceneLoadStarted = (level) => { };
        public static event SceneLoadComplete OnSceneLoadComplete = (level) => { };
        public static event TransitionAnimationFinished OnTransitionAnimationFinished;

        protected static BGSceneLoader _instance;
        public static BGSceneLoader Instance
        {
            get
            {
                if (_applicationQuitting)
                {
                    return null;
                }

                if (_instance == null)
                {
                    CreateInstance();
                }

                return _instance;
            }

            private set { _instance = value; }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void OnGameStart()
        {
            CreateInstance();
        }

        private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
        {
            _waitingForSceneLoad = false;
        }

        private static void CreateInstance()
        {
            if (_instance == null)
            {
#if DEBUG
                BGSceneLoader obj = FindObjectOfType<BGSceneLoader>();
                if (obj != null)
                {
                    Debug.LogError("Singleton " + obj.name + " already exists.");
                    _instance = obj;
                    return;
                }
#endif
                GameObject singleton = new GameObject();
                _instance = singleton.AddComponent<BGSceneLoader>();
                singleton.name = typeof(BGSceneLoader).ToString();

                DontDestroyOnLoad(singleton);
                _instance.Init();
            }
        }


        public void Init()
        {
            CreateSceneTransitionInstance();

            _currentLevel = SceneManager.GetActiveScene().name;
        }

        private void Start()
        {
            StartCoroutine(EndSceneTransition());
            _finishedInitOperation = true;
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        }

        private void OnDestroy()
        {
            _applicationQuitting = true;
            SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        }

        public static void Quit()
        {
            Instance.StartCoroutine(QuitGame());
        }

        private void CreateSceneTransitionInstance()
        {
            if (TransitionPrefab == null)
            {
                TransitionPrefab = Resources.Load<GameObject>("SceneTransitions");
            }

            if (TransitionPrefab != null)
            {
                GameObject instance = GameObject.Instantiate(TransitionPrefab);
                instance.transform.SetParent(transform, false);
                instance.name = "SceneTransitions";
                _sceneTransition = instance.GetComponent<SceneTransition>();
            }
        }

        public static void LoadLevel(string level, bool queue = false)
        {
            if (LoadInProgress)
            {
#if DEBUG
                if (LoadQueued)
                {
                    Debug.LogError("Attempted to load level '" + level +
                                   "' with scene already loading and another load queued. IGNORING THIS REQUEST");
                }
                else
                {
                    Debug.LogWarning("Attempted to load level '" + level +
                                     "' with scene already loading - going to finish current scene switch");
                }
#endif
                if (queue && !LoadQueued)
                {
                    Instance.StartCoroutine(QueueLoad(level));
                }
                return;
            }

            Instance.StartCoroutine(LoadCoroutine(level));
        }

        private static IEnumerator QueueLoad(string level)
        {
            LoadQueued = true;

            while (LoadInProgress)
            {
                yield return null;
            }

            LoadQueued = false;

            LoadLevel(level, false);
        }

        private static bool _waitingForTransitionAnimation;
        private static bool _waitingForAnimation = false;

        private static IEnumerator LoadCoroutine(string level)
        {
            LoadInProgress = true;
            OnSceneLoadStarted?.Invoke(level);
            //PauseScreen.TogglePause();
            Instance._currentLevel = level;

            yield return Instance.StartCoroutine(StartSceneTransition());

            if (!_finishedInitOperation)
            {
                yield return null;
            }
            _waitingForSceneLoad = true;
            PhotonNetwork.LoadLevel(level);
            while(_waitingForSceneLoad)
            {
                yield return null;
            }

            yield return new WaitForSecondsRealtime(0.4f);

            //AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(level);
            //asyncOperation.allowSceneActivation = false;

            //while (asyncOperation.progress < 0.89f)
            //{
            //    yield return null;
            //}

            //yield return new WaitForSecondsRealtime(0.4f);

            //asyncOperation.allowSceneActivation = true;

            //while (!asyncOperation.isDone)
            //{
            //    yield return null;
            //}

            if (!LoadQueued)
            {
                yield return new WaitForSecondsRealtime(0.1f);
            }
            
            LoadInProgress = false;
            OnSceneLoadComplete.Invoke(level);

            // wait for photon to join room
            while (NetworkManager.IsBusy)
            {
                yield return null;
            }
            
            yield return Instance.StartCoroutine(EndSceneTransition());
        }

        private static IEnumerator QuitGame()
        {
            LoadInProgress = true;
            //PauseScreen.TogglePause();
            OnSceneLoadStarted?.Invoke("Quit");
            yield return Instance.StartCoroutine(StartSceneTransition());
            Application.Quit();
        }


        private static IEnumerator StartSceneTransition()
        {
            _waitingForAnimation = true;
            _sceneTransition.TransitionOut(SceneTransitionFinished);

            float completion = 0;
            while (completion < 1)
            {
                completion += Time.unscaledDeltaTime;
                //SFXManager.SetGameSFXVolume(Mathf.Lerp(0, -80, completion));
                yield return null;
            }

            while (_waitingForAnimation)
            {
                yield return null;
            }
        }

        private static IEnumerator EndSceneTransition()
        {
            _waitingForTransitionAnimation = true;
            _waitingForAnimation = true;

            _sceneTransition.TransitionIn(SceneTransitionFinished);
            _sceneTransition.TransitionAnimationFinished(SceneTransitionAnimationFinished);

            while (_waitingForTransitionAnimation)
            {
                yield return null;
            }

            OnTransitionAnimationFinished?.Invoke();

            System.GC.Collect();

            float completion = 0;
            while (completion < 1)
            {
                completion += Time.unscaledDeltaTime * 15;
                //SFXManager.SetGameSFXVolume(Mathf.Lerp(-80, 0, completion));
                yield return null;
            }
            //SFXManager.SetGameSFXVolume(0);

            while (_waitingForAnimation)
            {
                yield return null;
            }
        }

        private static void SceneTransitionAnimationFinished()
        {
            _waitingForTransitionAnimation = false;
        }

        private static void SceneTransitionFinished()
        {
            _waitingForAnimation = false;
        }

        public static void ReloadCurrentLevel()
        {
            LoadLevel(Instance._currentLevel);
        }
    }
}
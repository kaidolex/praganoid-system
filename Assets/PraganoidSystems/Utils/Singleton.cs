using UnityEngine;

namespace PraganoidSystems.Utils
{
    /// <summary>
    /// Generic singleton base class for MonoBehaviour components.
    /// Inherit from this class to make your MonoBehaviour a singleton.
    /// </summary>
    /// <typeparam name="T">The type of the singleton class</typeparam>
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        // Singleton instance
        private static T _instance;
        private static readonly object _lock = new object();
        private static bool _applicationIsQuitting = false;

        /// <summary>
        /// The singleton instance. Creates the instance if it doesn't exist.
        /// </summary>
        public static T Instance
        {
            get
            {
                if (_applicationIsQuitting)
                {
                    Debug.LogWarning($"[{typeof(T).Name}] Instance already destroyed on application quit. Won't create again - returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindFirstObjectByType<T>();

                        if (_instance == null)
                        {
                            // Create a new GameObject to attach the singleton to
                            GameObject singletonObject = new GameObject();
                            _instance = singletonObject.AddComponent<T>();
                            singletonObject.name = typeof(T).ToString();

                            // Make sure the singleton persists between scene loads
                            DontDestroyOnLoad(singletonObject);
                        }
                    }

                    return _instance;
                }
            }
        }

        /// <summary>
        /// Called when the MonoBehaviour is created. Ensures only one instance exists.
        /// </summary>
        protected virtual void Awake()
        {
            // Ensure only one instance exists
            if (_instance == null)
            {
                _instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Debug.LogWarning($"[{typeof(T).Name}] Another instance of {typeof(T).Name} already exists! Destroying this instance.");
                Destroy(gameObject);
                return;
            }
        }

        /// <summary>
        /// Called when the MonoBehaviour is destroyed.
        /// </summary>
        protected virtual void OnDestroy()
        {
            _applicationIsQuitting = true;
        }

        /// <summary>
        /// Called when the application is quitting.
        /// </summary>
        protected virtual void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }

        /// <summary>
        /// Override this method to add initialization logic that should only run for the singleton instance.
        /// </summary>
        protected virtual void Start()
        {
            // Only initialize if this is the singleton instance
            if (_instance != this) return;
            
            // Override this method in derived classes to add initialization logic
        }
    }
}


using Unity.Collections;
using UnityEngine;

namespace PraganoidSystems.Events
{
    public class EventDebugger : MonoBehaviour
    {
        [SerializeField] VoidGameEvent OnPlayerDied;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnPlayerDied.Trigger(default);
            }
        }

        public void Test()
        {
            Debug.Log("Player died, sad. Very sad");
        }
    }
}


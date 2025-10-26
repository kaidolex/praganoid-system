using NUnit.Framework.Internal;
using UnityEngine;

namespace PraganoidSystems.Events
{
    public class UIDebug : MonoBehaviour
    {
        [SerializeField] VoidGameEvent OnPlayerDied;

        public void Test()
        {
            Debug.Log("Player Died, sad");
            Debug.Log(transform.position);
            gameObject.SetActive(false);
        }
    }
}


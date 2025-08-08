using UnityEngine;

namespace PraganoidSystems.Utils
{
    public static class GameLogger 
    {
        private static bool isEnabled = true;
        
        public static void Log( params object[] args) 
        {
            if (!isEnabled) return;

            Debug.Log(string.Join(" ", args));
        }
    }
}


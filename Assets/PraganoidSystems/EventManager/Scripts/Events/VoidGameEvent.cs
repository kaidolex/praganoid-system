using UnityEngine;


namespace PraganoidSystems.Events
{
    public struct NoParam { }

    [CreateAssetMenu(menuName = "Praganoid Systems/Event/Void Game Event")]
    public class VoidGameEvent : BaseGameEvent<NoParam> { }
}


// Man, I wish we had interfaces :(

using UdonSharp;
using UnityEngine;

namespace KitKat.JetSim.FloatingOrigin.Runtime
{
    public abstract class FO_Listener : UdonSharpBehaviour
    {
        [SerializeField, HideInInspector] private FO_Manager manager;

        public abstract void _Notify(Vector3 newOriginOffset);


        protected virtual void OnEnable()
        {
            manager._Subscribe(this);
            _Notify(manager.anchor.position);
        }
        protected virtual void OnDisable() => manager._Unsubscribe(this);
    }
}
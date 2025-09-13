
using System;
using UnityEngine;
using JetBrains.Annotations;

namespace KitKat.JetSim.FloatingOrigin.Runtime
{
    public partial class FO_Manager
    {
        /// <summary>
        /// Checks the player's distance from 0,0,0 and moves the world and player if the distance is larger than a set threshold.
        /// </summary>
        [PublicAPI]
        public void DistanceCheck() => DistanceCheck_Internal();

        [PublicAPI]
        public void Subscribe(FO_Listener listener) => _listeners = _listeners.AddUnique(listener);
        [PublicAPI]
        public void Unsubscribe(FO_Listener listener) => _listeners = _listeners.Remove(listener);

        [PublicAPI]
        public void RegisterDynamicObject(Transform obj) => _dynamicObjects = _dynamicObjects.AddUnique(obj);
        [PublicAPI]
        public void UnregisterDynamicObject(Transform obj) => _dynamicObjects = _dynamicObjects.Remove(obj);

        #region DEPRECATED

        private const string _PREFIXED_UNDERSCORE_INFO = "Prefixed underscores have been deprecated in v4.0.0, and will be removed in v5.0.0. Use the non-prefixed version instead.";
        
        [Obsolete(_PREFIXED_UNDERSCORE_INFO)]
        public void _Subscribe(FO_Listener listener) => Subscribe(listener);
        [Obsolete(_PREFIXED_UNDERSCORE_INFO)]
        public void _Unsubscribe(FO_Listener listener) => Unsubscribe(listener);

        [Obsolete(_PREFIXED_UNDERSCORE_INFO)]
        public void _RegisterDynamicObject(Transform obj) => RegisterDynamicObject(obj);
        [Obsolete(_PREFIXED_UNDERSCORE_INFO)]
        public void _UnregisterDynamicObject(Transform obj) => UnregisterDynamicObject(obj);

        #endregion // DEPRECATED
    }
}

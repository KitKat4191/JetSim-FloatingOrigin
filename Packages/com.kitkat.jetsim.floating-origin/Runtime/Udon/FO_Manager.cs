
//#define DO_LOGGING
#pragma warning disable IDE1006 // Naming Styles

using UnityEngine;
using UdonSharp;
using VRC.SDKBase;
using VRRefAssist;

namespace KitKat.JetSim.FloatingOrigin.Runtime
{
    [Singleton]
    [AddComponentMenu("")] // Hides this script from the add component menu to reduce clutter.
    [HelpURL("https://github.com/KitKat4191")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class FO_Manager : UdonSharpBehaviour
    {
        #region PUBLIC FIELDS

        [Header("Settings")]
        [Tooltip("The minimum distance (meters) from the origin that is required before moving the world.")]
        public float DistanceMoveThreshold = 100;
        [Tooltip("How often the player's distance from the origin is measured.")]
        public float SecondsPerDistanceCheck = 3;

        [Space]
        public Transform anchor;

        /// <summary>
        /// If the local local player is in a station other than the FO_PlayerStation.
        /// </summary>
        [HideInInspector] public bool InExternalStation;

        #endregion // PUBLIC FIELDS

        #region PRIVATE FIELDS

        private int _VRCShaderPropertyID;

        private VRCStation _playerStation;
        private VRCPlayerApi _localPlayer;
        private Transform[] _rootObjects;

        private bool _distanceCheckLoopStarted;

        private FO_Listener[] _listeners = System.Array.Empty<FO_Listener>();

        private Transform[] _dynamicObjects = System.Array.Empty<Transform>();

        #endregion // PRIVATE FIELDS

        #region FIELD VALIDATION

        private void OnValidate()
        {
            DistanceMoveThreshold = Mathf.Clamp(DistanceMoveThreshold, 0f, float.MaxValue);
            SecondsPerDistanceCheck = Mathf.Clamp(SecondsPerDistanceCheck, 0f, float.MaxValue);
        }

        #endregion // FIELD VALIDATION

        ////////////////

        #region UNITY

        private void Start()
        {
            _VRCShaderPropertyID = VRCShader.PropertyToID("_Udon_FO_WorldOffset");
            VRCShader.SetGlobalVector(_VRCShaderPropertyID, anchor.position);

            _localPlayer = Networking.LocalPlayer;

            _rootObjects = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
                _rootObjects[i] = transform.GetChild(i);
        }

        #endregion // UNITY

        #region API

        /// <summary>
        /// The station currently assigned to the local player.
        /// </summary>
        private FO_PlayerStation _localPlayerStation;

        /// <summary>
        /// This is called from the FO_StationNotifier to hand off sync responsibility to the station the local player entered.
        /// </summary>
        internal void _ExternalStationEntered()
        {
#if DO_LOGGING
            FO_Debug.Log("_ExternalStationEntered called");
#endif
            InExternalStation = true;

            if (_localPlayerStation)
                _localPlayerStation._OnEnterExternalStation();
        }
        /// <summary>
        /// This is called from the FO_StationNotifier to give sync responsibility back to the local player's FO_PlayerStation.
        /// </summary>
        internal void _ExternalStationExited()
        {
#if DO_LOGGING
            FO_Debug.Log("_ExternalStationExited called");
#endif
            InExternalStation = false;

            if (_localPlayerStation)
                _localPlayerStation._OnExitExternalStation();
        }
        
        internal void _RegisterPlayerStation(FO_PlayerStation playerStation)
        {
            if (!playerStation) return;

            _localPlayerStation = playerStation;
            _playerStation = _localPlayerStation.GetComponent<VRCStation>();

            StartDistanceCheckLoop();
        }

        public void _Subscribe(FO_Listener listener) => _listeners = _listeners.AddUnique(listener);
        public void _Unsubscribe(FO_Listener listener) => _listeners = _listeners.Remove(listener);

        public void _RegisterDynamicObject(Transform obj) => _dynamicObjects = _dynamicObjects.AddUnique(obj);
        public void _UnregisterDynamicObject(Transform obj) => _dynamicObjects = _dynamicObjects.Remove(obj);

        #endregion // API

        #region VRC OVERRIDES

        public override void OnPlayerRespawn(VRCPlayerApi _)
        {
            // When the player respawns we want to re-align the coordinate system so world space objects such as
            // prints, cameras, portals, etc. appear in the correct positions.
            TranslateWorld(-anchor.position);
            
#if DO_LOGGING
            FO_Debug.Log($"Translate World called from OnPlayerRespawn.");
#endif
        }

        #endregion // VRC OVERRIDES
        
        #region INTERNAL

        /// <summary>
        /// Checks the player's distance from 0,0,0 and moves the world and player if the distance is larger than a set threshold.
        /// </summary>
        private void DistanceCheck()
        {
            Vector3 playerPos = _localPlayer.GetPosition();
            if (playerPos.magnitude < DistanceMoveThreshold) return;

            TranslateWorld(-playerPos);
            
#if DO_LOGGING
            FO_Debug.Log($"Translate World called from DistanceCheck.");
#endif
        }
        
        private void StartDistanceCheckLoop()
        {
            if (_distanceCheckLoopStarted) return;
            _distanceCheckLoopStarted = true;
            SendCustomEventDelayedFrames(nameof(_DistanceCheckLoop), 1);
        }

        /// <summary>
        /// The internal loop used to determine when to move the origin.
        /// </summary>
        /// <remarks>
        /// Public due to SendCustomEventDelayedSeconds. Don't call this.
        /// </remarks>
        public void _DistanceCheckLoop()
        {
            DistanceCheck();
            SendCustomEventDelayedSeconds(nameof(_DistanceCheckLoop), SecondsPerDistanceCheck);
        }

        private void TranslateWorld(Vector3 delta)
        {
            // Move the world
            transform.Translate(delta);

            // Move the player
            if (!InExternalStation)
            {
                Vector3 playerVelocity = _localPlayer.GetVelocity();
                _localPlayer.TeleportTo(_localPlayer.GetPosition() + delta);
                //_playerStation.UseStation(_localPlayer); TODO is this really required?
                _localPlayer.SetVelocity(playerVelocity);
            }

            // Reparent children to avoid large local positions on colliders and stuff near the player.
            transform.DetachChildren();
            transform.position = Vector3.zero;
            
            foreach (Transform t in _rootObjects)
                t.SetParent(transform);

            foreach (Transform t in _dynamicObjects)
                t.SetParent(transform);

            Vector3 anchorPos = anchor.position;
            NotifyListeners(anchorPos);
            VRCShader.SetGlobalVector(_VRCShaderPropertyID, anchorPos);

#if DO_LOGGING
            FO_Debug.LogSuccess($"Moved origin {delta.magnitude}m");
#endif
        }
        
        private void NotifyListeners(Vector3 newOriginOffset)
        {
            foreach (FO_Listener listener in _listeners)
            {
                if (!listener) continue;
                listener._Notify(newOriginOffset);
            }
        }

        #endregion // INTERNAL
    }
}

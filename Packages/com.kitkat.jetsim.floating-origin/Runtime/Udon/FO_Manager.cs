
#pragma warning disable IDE1006 // Naming Styles

using UnityEngine;
using UdonSharp;
using VRC.SDKBase;
using VRRefAssist;

namespace KitKat.JetSim.FloatingOrigin.Runtime
{
    [Singleton]
    [AddComponentMenu("")] // Hides this script from the add component menu to reduce clutter.
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public partial class FO_Manager : UdonSharpBehaviour
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

        private int _vrcShaderPropertyID;

        /// <summary>
        /// The station currently assigned to the local player.
        /// </summary>
        private FO_PlayerStation _localPlayerStation;
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
            _vrcShaderPropertyID = VRCShader.PropertyToID("_Udon_FO_WorldOffset");
            VRCShader.SetGlobalVector(_vrcShaderPropertyID, anchor.position);

            _localPlayer = Networking.LocalPlayer;

            _rootObjects = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
                _rootObjects[i] = transform.GetChild(i);
        }

        #endregion // UNITY
        
        #region VRC OVERRIDES

        public override void OnPlayerRespawn(VRCPlayerApi _)
        {
            // When the player respawns we want to re-align the coordinate system so world space objects such as
            // prints, cameras, portals, etc. appear in the correct positions.
            TranslateWorld(-anchor.position);
            
#if JS_FLOATING_ORIGIN_ENABLE_LOGGING
            FO_Debug.Log($"Translate World called from OnPlayerRespawn.");
#endif
        }

        #endregion // VRC OVERRIDES
        
        #region INTERNAL

        /// <summary>
        /// This is called from the FO_StationNotifier to hand off sync responsibility to the station the local player entered.
        /// </summary>
        internal void ExternalStationEntered()
        {
#if JS_FLOATING_ORIGIN_ENABLE_LOGGING
            FO_Debug.Log("External station entered.");
#endif
            InExternalStation = true;

            if (_localPlayerStation)
                _localPlayerStation._OnEnterExternalStation();
        }
        /// <summary>
        /// This is called from the FO_StationNotifier to give sync responsibility back to the local player's FO_PlayerStation.
        /// </summary>
        internal void ExternalStationExited()
        {
#if JS_FLOATING_ORIGIN_ENABLE_LOGGING
            FO_Debug.Log("External station exited.");
#endif
            InExternalStation = false;

            if (_localPlayerStation)
                _localPlayerStation._OnExitExternalStation();
        }
        
        internal void RegisterPlayerStation(FO_PlayerStation playerStation)
        {
            if (!playerStation) return;

            _localPlayerStation = playerStation;
            _playerStation = playerStation.GetComponent<VRCStation>();

            StartDistanceCheckLoop();
        }

        private void DistanceCheck_Internal()
        {
            Vector3 playerPos = _localPlayer.GetPosition();
            if (playerPos.magnitude < DistanceMoveThreshold) return;

            TranslateWorld(-playerPos);

#if JS_FLOATING_ORIGIN_ENABLE_LOGGING
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

        private void TranslateWorld(Vector3 delta) // TODO: use double precision
        {
            // Move the world
            transform.Translate(delta);

            // Move the player
            if (!InExternalStation)
            {
                Vector3 playerVelocity = _localPlayer.GetVelocity();
                _localPlayer.TeleportTo(_localPlayer.GetPosition() + delta);
                _playerStation.UseStation(_localPlayer);
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
            VRCShader.SetGlobalVector(_vrcShaderPropertyID, anchorPos);

#if JS_FLOATING_ORIGIN_ENABLE_LOGGING
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

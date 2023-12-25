
#define DO_LOGGING

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

        public float RotationSmoothing = 0.1f;
        public float PositionSmoothing = 0.07f;

        /// <summary>
        /// The minimum distance from 0,0,0 required before moving the origin.
        /// </summary>
        public float DistanceMoveThreshold = 100;
        public float SecondsBetweenDistanceCheck = 3;

        [Space]
        public Transform anchor;

        /// <summary>
        /// If the local local player is in a station other than the FO_PlayerStation.
        /// </summary>
        [HideInInspector] public bool InExternalStation;

        #endregion // PUBLIC FIELDS

        #region PRIVATE FIELDS

        private VRCStation _playerStation;
        private VRCPlayerApi _localPlayer;
        private Transform[] _rootObjects;

        #endregion // PRIVATE FIELDS

        #region FIELD VALIDATION
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        private void OnValidate()
        {
            RotationSmoothing = Mathf.Clamp(RotationSmoothing, 0f, 1f);
            PositionSmoothing = Mathf.Clamp(PositionSmoothing, 0f, 1f);

            DistanceMoveThreshold = Mathf.Clamp(DistanceMoveThreshold, 0f, float.MaxValue);
            SecondsBetweenDistanceCheck = Mathf.Clamp(SecondsBetweenDistanceCheck, 0f, float.MaxValue);
        }
#endif
        #endregion // FIELD VALIDATION

        ////////////////

        #region UNITY

        private void Start()
        {
            _localPlayer = Networking.LocalPlayer;

            _rootObjects = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                _rootObjects[i] = transform.GetChild(i);
            }
        }

        #endregion // UNITY

        #region API

        /// <summary>
        /// The station currently assigned to the local player.
        /// </summary>
        public FO_PlayerStation LocalPlayerStation { get; private set; }

        /// <summary>
        /// This is called from the FO_StationNotifier to hand off sync responsibility to the station the local player entered.
        /// </summary>
        public void _ExternalStationEntered()
        {
#if DO_LOGGING
            _print("_ExternalStationEntered called");
#endif
            InExternalStation = true;

            if (LocalPlayerStation)
                LocalPlayerStation._OnEnterExternalStation();
        }
        /// <summary>
        /// This is called from the FO_StationNotifier to give sync responsibility back to the local player's FO_PlayerStation.
        /// </summary>
        public void _ExternalStationExited()
        {
#if DO_LOGGING
            _print("_ExternalStationExited called");
#endif
            InExternalStation = false;

            if (LocalPlayerStation)
                LocalPlayerStation._OnExitExternalStation();
        }

        /// <summary>
        /// Called by FO_PlayerStation when assigned to the local player.
        /// </summary>
        public void _RegisterPlayerStation(FO_PlayerStation playerStation)
        {
            if (!playerStation) return;
            LocalPlayerStation = playerStation;
            _playerStation = LocalPlayerStation.GetComponent<VRCStation>();
            playerStation._SetInterpolationSettings(PositionSmoothing, RotationSmoothing);

            SendCustomEventDelayedSeconds(nameof(_DistanceCheckLoop), SecondsBetweenDistanceCheck);
        }

        /// <summary>
        /// Checks the player's distance from 0,0,0 and moves the world parent and player if the distance is larger than a set threshold.
        /// </summary>
        public void _DistanceCheck()
        {
            VRCPlayerApi.TrackingData _trackingData = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Origin);

            Vector3 playerPos = _trackingData.position;
            if (playerPos.magnitude < DistanceMoveThreshold) return;

            // Move world and reparent children
            transform.Translate(-playerPos);

            if (!InExternalStation)
            {
                Vector3 playerVelocity = _localPlayer.GetVelocity();
                _localPlayer.TeleportTo(Vector3.zero, _trackingData.rotation, VRC_SceneDescriptor.SpawnOrientation.AlignRoomWithSpawnPoint);
                _playerStation.UseStation(_localPlayer);
                _localPlayer.SetVelocity(playerVelocity);
            }

            transform.DetachChildren();
            transform.position = Vector3.zero;
            for (int i = 0; i < _rootObjects.Length; i++)
            {
                _rootObjects[i].SetParent(transform);
            }
#if DO_LOGGING
            _printSuccess($"Moved origin {playerPos.magnitude}m");
#endif
        }

        #endregion // API

        #region INTERNAL

        /// <summary>
        /// The internal loop used to determine when to move the origin.
        /// </summary>
        /// <remarks>
        /// Public due to SendCustomEventDelayedSeconds. Don't call this.
        /// </remarks>
        public void _DistanceCheckLoop()
        {
            _DistanceCheck();
            SendCustomEventDelayedSeconds(nameof(_DistanceCheckLoop), SecondsBetweenDistanceCheck);
        }

        #endregion // INTERNAL

        #region LOGGING
#if DO_LOGGING
        private const string _LOG_IDENTIFIER = "[<color=purple>FloatingOrigin</color>]";
        protected void _print(string message)
        {
            Debug.Log($"{_LOG_IDENTIFIER} <color=white>[{name}]</color> <color=lightblue>{message}</color>", this);
        }

        protected void _printSuccess(string message)
        {
            Debug.Log($"{_LOG_IDENTIFIER} <color=white>[{name}]</color> <color=lime>{message}</color>", this);
        }

        protected void _printWarning(string message)
        {
            Debug.LogWarning($"{_LOG_IDENTIFIER} <color=white>[{name}]</color> <color=orange>{message}</color>", this);
        }

        protected void _printError(string message)
        {
            Debug.LogError($"{_LOG_IDENTIFIER} <color=white>[{name}]</color> <color=red>{message}</color>", this);
        }
#endif
        #endregion // LOGGING
    }
}
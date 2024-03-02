
#define DO_LOGGING

using UnityEngine;
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon.Common;
using VRRefAssist;
using Cyan.PlayerObjectPool;

namespace KitKat.JetSim.FloatingOrigin.Runtime
{
    [AddComponentMenu("")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class FO_PlayerStation : CyanPlayerObjectPoolObject
    {
        #region VRRefAssist

        [SerializeField, HideInInspector] private FO_Manager FO_Manager;
        [SerializeField, HideInInspector, GetComponent] private VRCStation station;

        #endregion // VRRefAssist

        #region PRIVATE FIELDS

        private Transform _anchor;

        private bool _localPlayerIsOwner;

        
        /// <summary>
        /// If the local local player is in the FO_PlayerStation.
        /// </summary>
        private bool _localPlayerSeated;

        private float _timeSinceLastSerialization;
        private const float _SYNC_RATE = 0.25f;

        #endregion // PRIVATE FIELDS

        #region SYNCED FIELDS

        /// <summary>
        /// Controls interpolation on remote.
        /// </summary>
        /// <remarks>
        /// True = no interpolation this serialization.
        /// Gets reset to false each network tick.
        /// This is True by default so we don't interpolate when a player is assigned.
        /// </remarks>
        [UdonSynced] private bool _flagDiscontinuity = true;

        /// <remarks>
        /// UdonSynced
        /// </remarks>
        [UdonSynced] private short _playerRotation_Y = 0;
        private Quaternion _playerRotation = Quaternion.identity;
        private Quaternion _smoothPlayerRotation = Quaternion.identity;

        /// <remarks>
        /// UdonSynced
        /// </remarks>
        [UdonSynced] private Vector3 _playerPosition = Vector3.zero;
        private Vector3 _smoothPlayerPosition = Vector3.zero;

        #endregion // SYNCED FIELDS

        ////////////////

        #region UNITY

        private void Start()
        {
            _anchor = FO_Manager.anchor;
        }

        private void Update()
        {
            if (_localPlayerIsOwner) HandleSerialization(); else HandleInterpolation();
        }

        #endregion // UNITY

        private void HandleSerialization()
        {
            _timeSinceLastSerialization += Time.deltaTime;
            if (_timeSinceLastSerialization < _SYNC_RATE) return;
            _timeSinceLastSerialization = 0;
            RequestSerialization();
        }

        private void HandleInterpolation()
        {
            
        }

        #region NETWORKING OVERRIDES

        public override void OnPreSerialization()
        {
            if (!VRC.SDKBase.Utilities.IsValid(Owner)) return;
            _playerPosition = Owner.GetPosition() - _anchor.position;
            _playerRotation_Y = System.Convert.ToInt16(Owner.GetTrackingData(VRCPlayerApi.TrackingDataType.AvatarRoot).rotation.eulerAngles.y);
        }
        public override void OnDeserialization(DeserializationResult result)
        {
            _playerRotation = Quaternion.Euler(0, _playerRotation_Y, 0);

            if (!_flagDiscontinuity) return;
#if DO_LOGGING
            _print($"Discontinuity triggered.");
#endif

            // Skip interpolation

            _smoothPlayerPosition = _playerPosition;
            _smoothPlayerRotation = _playerRotation;
        }
        public override void OnPostSerialization(SerializationResult result)
        {
            _flagDiscontinuity = false;
        }

        #endregion // NETWORKING OVERRIDES

        #region PLAYER OBJECT POOL OVERRIDES

        public override void _OnCleanup() => _flagDiscontinuity = true;
        public override void _OnOwnerSet()
        {
            if (!VRC.SDKBase.Utilities.IsValid(Owner)) return;

            _localPlayerIsOwner = Owner.isLocal;

#if DO_LOGGING
            _print(_localPlayerIsOwner ? "_OnOwnerSet for local player" : "_OnOwnerSet for remote player");
#endif

            station.PlayerMobility = _localPlayerIsOwner ? VRCStation.Mobility.Mobile : VRCStation.Mobility.Immobilize;

            if (!_localPlayerIsOwner) return;

            FO_Manager._RegisterPlayerStation(this);

            _ForcePlayerInStationLoop();

            RequestSerialization();
        }

        #endregion // PLAYER OBJECT POOL OVERRIDES

        #region FLOATING ORIGIN EVENTS

        public void _OnEnterExternalStation()
        {
            #if DO_LOGGING
            _print("_OnEnterExternalStation");
            #endif

            gameObject.SetActive(false);
        }
        public void _OnExitExternalStation()
        {
            #if DO_LOGGING
            _print("_OnExitExternalStation");
            #endif

            gameObject.SetActive(true);

            _flagDiscontinuity = true;

            SendCustomEventDelayedFrames(nameof(_ForcePlayerInStationLoop), 1);
        }

        #endregion // FLOATING ORIGIN EVENTS

        #region VRC OVERRIDES

        public override void OnStationEntered(VRCPlayerApi player)
        {
            if (!_localPlayerIsOwner) return;
            
            #if DO_LOGGING
            _print("Entered FO_PlayerStation");
            #endif

            _localPlayerSeated = true;
            _flagDiscontinuity = true;
        }
        public override void OnStationExited(VRCPlayerApi player)
        {
            if (!_localPlayerIsOwner) return;
            if (!VRC.SDKBase.Utilities.IsValid(Owner)) return;

            #if DO_LOGGING
            _print("Exited FO_PlayerStation");
            _print("Distance check called from OnStationExited");
            #endif

            _localPlayerSeated = false;
            _flagDiscontinuity = true;

            SendCustomEventDelayedFrames(nameof(_ForcePlayerInStationLoop), 1);

            FO_Manager.SendCustomEventDelayedFrames(nameof(Runtime.FO_Manager._DistanceCheck), 1);
        }

        public override void OnPlayerRespawn(VRCPlayerApi player)
        {
            if (!_localPlayerIsOwner) return;
            if (!VRC.SDKBase.Utilities.IsValid(Owner)) return;

            #if DO_LOGGING
            _print("Local Player Respawned.");
            #endif

            _flagDiscontinuity = true;

            Owner.UseAttachedStation();
        }

        #endregion // VRC OVERRIDES

        #region FORCE PLAYER IN

        public void _ForcePlayerInStationLoop()
        {
            if (_localPlayerSeated) return;
            if (FO_Manager.InExternalStation) return;
            SendCustomEventDelayedSeconds(nameof(_ForcePlayerInStationLoop), 0.5f, VRC.Udon.Common.Enums.EventTiming.LateUpdate);

            ForcePlayerBackIn();
        }

        private void ForcePlayerBackIn()
        {
            if (FO_Manager.InExternalStation)
            {
                #if DO_LOGGING
                _printWarning("_ForcePlayerBackIn aborted : _localPlayer is in an external station.");
                #endif

                return;
            }

            if (_localPlayerSeated)
            {
                #if DO_LOGGING
                _printWarning("_ForcePlayerBackIn aborted : _localPlayer is already in the FO_PlayerStation.");
                #endif

                return;
            }

            Owner.UseAttachedStation();
        }

        #endregion // FORCE PLAYER IN

        #region LOGGING
#if DO_LOGGING
        private const string _LOG_IDENTIFIER = "[<color=purple>FloatingOrigin</color>]";
        protected void _print(string message)
        {
            Debug.Log($"{_LOG_IDENTIFIER} <color=lightblue>{message}</color>", this);
        }

        protected void _printSuccess(string message)
        {
            Debug.Log($"{_LOG_IDENTIFIER} <color=lime>{message}</color>", this);
        }

        protected void _printWarning(string message)
        {
            Debug.LogWarning($"{_LOG_IDENTIFIER} <color=orange>{message}</color>", this);
        }

        protected void _printError(string message)
        {
            Debug.LogError($"{_LOG_IDENTIFIER} <color=red>{message}</color>", this);
        }
#endif
        #endregion // LOGGING
    }
}
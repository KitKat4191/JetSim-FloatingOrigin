
//#define DO_LOGGING

using UnityEngine;
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon.Common;
using VRRefAssist;

namespace KitKat.JetSim.FloatingOrigin.Runtime
{
    [AddComponentMenu("")] // Hides this script from the add component menu to reduce clutter.
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class FO_PlayerStation : UdonSharpBehaviour
    {
        #region VRRefAssist

        [SerializeField, HideInInspector] private FO_Manager manager;
        [SerializeField, HideInInspector, GetComponent] private VRCStation station;

        #endregion // VRRefAssist

        #region PRIVATE FIELDS

        private Transform _anchor;

        private VRCPlayerApi _owner;
        private bool _localPlayerIsOwner;
        private bool _isOwnerInVR;

        
        /// <summary>
        /// If the local local player is in the FO_PlayerStation.
        /// </summary>
        private bool _localPlayerSeated;

        private float _timeSinceLastSerialization;
        private const float _SYNC_RATE = 0.25f;

        #endregion // PRIVATE FIELDS

        #region SYNCED FIELDS

        /// <remarks>UdonSynced</remarks>
        [UdonSynced] private Vector3 _playerPosition = Vector3.zero;

        /// <remarks>UdonSynced</remarks>
        [UdonSynced] private short _playerRotation_Y = 0;

        /// <summary>
        /// Controls interpolation on remote.
        /// </summary>
        /// <remarks>
        /// True = no interpolation this serialization.
        /// Gets reset to false each network tick.
        /// This is True by default so we don't interpolate when a player is assigned.
        /// </remarks>
        [UdonSynced] private bool _flagDiscontinuity = true;

        #endregion // SYNCED FIELDS

        ////////////////

        #region UNITY

        private void Start()
        {
            transform.parent = null;
            
            _owner = Networking.GetOwner(gameObject);
            
            _localPlayerIsOwner = _owner.isLocal;
            _isOwnerInVR = _owner.IsUserInVR();
            
            _anchor = manager.anchor;

#if DO_LOGGING
            _print(_localPlayerIsOwner ? "_OnOwnerSet for local player" : "_OnOwnerSet for remote player");
#endif

            station.PlayerMobility = _localPlayerIsOwner ? VRCStation.Mobility.Mobile : VRCStation.Mobility.ImmobilizeForVehicle;

            if (!_localPlayerIsOwner) return;

            manager._RegisterPlayerStation(this);

            _ForcePlayerInStationLoop();

            RequestSerialization();
        }

        private void Update()
        {
            if (_localPlayerIsOwner) HandleSerialization(); else HandleInterpolation();
        }

        #endregion // UNITY

        #region SYNC & INTERPOLATION

        private void HandleSerialization()
        {
            _timeSinceLastSerialization += Time.deltaTime;
            if (_timeSinceLastSerialization < _SYNC_RATE) return;
            _timeSinceLastSerialization = 0;
            RequestSerialization();
        }

        private void HandleInterpolation()
        {
            float simulationTime;

            if (_isOwnerInVR)
            {
                _smoothNetworkTime = Mathf.SmoothDamp(_smoothNetworkTime, _networkTime, ref _networkVelocity, 0.2f);
                simulationTime = Time.realtimeSinceStartup - _smoothNetworkTime - 0.5f;
            }
            else simulationTime = Networking.SimulationTime(_owner);

            GetIndexLeftAndRightOf(time: simulationTime, out int left, out int right);

            float interpolation = _discontinuityFlagBuffer[right] ? 1 : Mathf.InverseLerp(_timestampBuffer[left], _timestampBuffer[right], simulationTime);

            Vector3 position = Vector3.Lerp(_positionBuffer[left], _positionBuffer[right], interpolation);
            Quaternion rotation = Quaternion.Slerp(_rotationBuffer[left], _rotationBuffer[right], interpolation);

            transform.SetPositionAndRotation(
                position + _anchor.position,
                rotation
            );
        }

        #endregion // SYNC & INTERPOLATION

        #region NETWORKING OVERRIDES

        public override void OnPreSerialization()
        {
            if (!VRC.SDKBase.Utilities.IsValid(_owner)) return;

            _playerPosition = _owner.GetPosition() - _anchor.position;
            _playerRotation_Y = System.Convert.ToInt16(_owner.GetTrackingData(VRCPlayerApi.TrackingDataType.AvatarRoot).rotation.eulerAngles.y);
        }
        public override void OnDeserialization(DeserializationResult result)
        {
            if (_isOwnerInVR) _networkTime = Time.realtimeSinceStartup - result.sendTime;

            Quaternion playerRotation = Quaternion.Euler(0, _playerRotation_Y, 0);

            Capture(
                position: _playerPosition,
                rotation: playerRotation,
                discontinuity: _flagDiscontinuity,
                timestamp: result.sendTime
            );


#if DO_LOGGING
            if (_flagDiscontinuity) _print($"Discontinuity triggered.");
#endif
        }
        public override void OnPostSerialization(SerializationResult result)
        {
            _flagDiscontinuity = false;
        }

        #endregion // NETWORKING OVERRIDES

        #region PLAYOUT DELAY BUFFER

        private const int _BUFFER_SIZE = 10;

        /// <summary>
        /// This points to the last slot that was written to.
        /// </summary>
        private int _ringBufferWritePointer;

        private readonly Vector3[] _positionBuffer = new Vector3[_BUFFER_SIZE];
        private readonly Quaternion[] _rotationBuffer = new Quaternion[_BUFFER_SIZE];

        private readonly bool[] _discontinuityFlagBuffer = new bool[_BUFFER_SIZE];
        private readonly float[] _timestampBuffer = new float[_BUFFER_SIZE];

        private void Capture(Vector3 position, Quaternion rotation, bool discontinuity, float timestamp)
        {
            _ringBufferWritePointer++;
            if (_ringBufferWritePointer == _BUFFER_SIZE)
                _ringBufferWritePointer = 0;

            _positionBuffer[_ringBufferWritePointer] = position;
            _rotationBuffer[_ringBufferWritePointer] = rotation;

            _discontinuityFlagBuffer[_ringBufferWritePointer] = discontinuity;
            _timestampBuffer[_ringBufferWritePointer] = timestamp;
        }

        private void GetIndexLeftAndRightOf(float time, out int left, out int right)
        {
            int newer = _ringBufferWritePointer;
            int older = newer - 1;
            if (older < 0)
                older = _BUFFER_SIZE - 1;

            left = older;
            right = newer;

            // Traverse the buffer from the newest element to the oldest.
            for (int i = 0; i < _BUFFER_SIZE; i++)
            {
                if (_timestampBuffer[older] <= time && time <= _timestampBuffer[newer])
                {
                    right = newer;
                    left = older;
                    break;
                }

                newer = older;
                older--;
                if (older < 0)
                    older = _BUFFER_SIZE - 1;
            }
        }

        private float _networkTime;
        private float _smoothNetworkTime;
        private float _networkVelocity;

        #endregion // PLAYOUT DELAY BUFFER
        
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
            if (!VRC.SDKBase.Utilities.IsValid(_owner)) return;

            #if DO_LOGGING
            _print("Exited FO_PlayerStation");
            _print("Distance check called from OnStationExited");
            #endif

            _localPlayerSeated = false;
            _flagDiscontinuity = true;

            SendCustomEventDelayedFrames(nameof(_ForcePlayerInStationLoop), 1);

            manager.SendCustomEventDelayedFrames(nameof(Runtime.FO_Manager._DistanceCheck), 1);
        }

        public override void OnPlayerRespawn(VRCPlayerApi player)
        {
            if (!_localPlayerIsOwner) return;
            if (!VRC.SDKBase.Utilities.IsValid(_owner)) return;

            #if DO_LOGGING
            _print("Local Player Respawned.");
            #endif

            _flagDiscontinuity = true;

            _owner.UseAttachedStation();
        }

        #endregion // VRC OVERRIDES

        #region FORCE PLAYER IN

        public void _ForcePlayerInStationLoop()
        {
            if (_localPlayerSeated) return;
            if (manager.InExternalStation) return;
            SendCustomEventDelayedSeconds(nameof(_ForcePlayerInStationLoop), 0.5f, VRC.Udon.Common.Enums.EventTiming.LateUpdate);

            ForcePlayerBackIn();
        }

        private void ForcePlayerBackIn()
        {
            #if UNITY_EDITOR
            // This is a temporary workaround to VRChat breaking multiple scripts per GameObject as of SDK v3.7.4
            // TODO: Has this been fixed yet? https://feedback.vrchat.com/udon/p/only-one-client-sim-udon-helper-gets-added-as-of-sdk-v374
            return;
            #endif
            
            if (manager.InExternalStation)
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

            _owner.UseAttachedStation();
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

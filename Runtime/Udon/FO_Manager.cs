
#define DO_LOGGING
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

        private FO_Listener[] _listeners = new FO_Listener[0];

        private Transform[] _dynamicObjects = new Transform[0];

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

            StartDistanceCheckLoop();
        }

        /// <summary>
        /// Checks the player's distance from 0,0,0 and moves the world parent and player if the distance is larger than a set threshold.
        /// </summary>
        public void _DistanceCheck()
        {
            VRCPlayerApi.TrackingData _trackingData = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Origin);

            Vector3 playerPos = _trackingData.position;
            if (playerPos.magnitude < DistanceMoveThreshold) return;

            // Move the world
            transform.Translate(-playerPos);

            // Move the player
            if (!InExternalStation)
            {
                Vector3 playerVelocity = _localPlayer.GetVelocity();
                _localPlayer.TeleportTo(Vector3.zero, _trackingData.rotation, VRC_SceneDescriptor.SpawnOrientation.AlignRoomWithSpawnPoint);
                _playerStation.UseStation(_localPlayer);
                _localPlayer.SetVelocity(playerVelocity);
            }

            // Reparent children to avoid large local positions on colliders and stuff near the player.
            transform.DetachChildren();
            transform.position = Vector3.zero;
            for (int i = 0; i < _rootObjects.Length; i++)
                _rootObjects[i].SetParent(transform);

            for (int i = 0; i < _dynamicObjects.Length; i++)
                _dynamicObjects[i].SetParent(transform);

            Vector3 anchorPos = anchor.position;
            NotifyListeners(anchorPos);
            VRCShader.SetGlobalVector(_VRCShaderPropertyID, anchorPos);

#if DO_LOGGING
            _printSuccess($"Moved origin {playerPos.magnitude}m");
#endif
        }

        public void _Subscribe(FO_Listener listener) => _listeners = AddUnique(_listeners, listener);
        public void _Unsubscribe(FO_Listener listener) => _listeners = Remove(_listeners, listener);

        public void _RegisterDynamicObject(Transform obj) => _dynamicObjects = AddUnique(_dynamicObjects, obj);
        public void _UnregisterDynamicObject(Transform obj) => _dynamicObjects = Remove(_dynamicObjects, obj);

        #endregion // API

        #region INTERNAL

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
            _DistanceCheck();
            SendCustomEventDelayedSeconds(nameof(_DistanceCheckLoop), SecondsPerDistanceCheck);
        }

        private void NotifyListeners(Vector3 newOriginOffset)
        {
            foreach (FO_Listener listener in _listeners)
            {
                if (!listener) continue;
                listener._Notify(newOriginOffset);
            }
        }

        #region ARRAY STUFF

        private static T[] AddUnique<T>(T[] array, T item)
        {
            if (item == null) return array;
            int index = System.Array.IndexOf(array, item);
            if (index > -1) return array; // The item is already in the array.

            var temp = new T[array.Length + 1];

            array.CopyTo(temp, 0);

            array = temp;
            array[array.Length - 1] = item;

            return array;
        }

        private static T[] Remove<T>(T[] array, T item)
        {
            int listenerIndex = System.Array.IndexOf(array, item);
            if (listenerIndex == -1) return array; // The item isn't in the array so we don't need to remove it.

            if (array.Length <= 1) return new T[0];

            array[listenerIndex] = default;
            // Put the last item in place of the empty slot to fill the gap.
            int newLength = array.Length - 1;
            Swap(array, listenerIndex, newLength);

            // Shrink the array by 1
            var newArray = new T[newLength];

            System.Array.Copy(array, newArray, newLength);
            return newArray;
        }

        private static void Swap<T>(T[] array, int A, int B)
        {
            T temp = array[A];
            array[A] = array[B];
            array[B] = temp;
        }

        #endregion // ARRAY STUFF

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
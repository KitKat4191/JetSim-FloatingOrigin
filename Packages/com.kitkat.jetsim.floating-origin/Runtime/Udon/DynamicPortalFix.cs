
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace KitKat.JetSim.FloatingOrigin.Runtime
{
    [AddComponentMenu("")]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class DynamicPortalFix : UdonSharpBehaviour
    {
        [UdonSynced] private Vector3 _portalPosition;


        [SerializeField, HideInInspector] private FO_Manager FO_Manager;
        private Transform _anchor;

        private GameObject _portal;
        private Transform _portalTransform;
        
        private TextMeshProUGUI _portalTMPro;
        private string _portalData;

        private VRCPlayerApi _localPlayer;


        private void Start()
        {
            _localPlayer = Networking.LocalPlayer;

            _anchor = FO_Manager.transform.GetChild(0);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other) return;
            if (!other.gameObject) return;
            if (other.name != "PortalInternal(Clone)") return;

            if (_portal == other.gameObject) return; // It's the same portal lmao

            //Debug.Log("Detected Dynamic Portal!");

            _portal = other.gameObject;
            _portal.SetActive(false);

            _portalTransform = other.transform;
            _portalTransform.SetParent(_anchor);

            _delayCounter = 0;

            _portalTMPro = other.GetComponentInChildren<TextMeshProUGUI>(true);
            if (!_portalTMPro) { Debug.LogError("Couldn't find NameTag's TextMeshProUGUI component!"); return; }

            SendCustomEventDelayedSeconds(nameof(_DelayedTextFetch), 0.1f);
        }

        private int _delayCounter;
        public void _DelayedTextFetch()
        {
            _delayCounter++;
            _portalData = _portalTMPro.text;
            
            // Check if the placeholder format text has been replaced with the real content.
            if (!_portalData.StartsWith("[world]")) { FetchData(); return; }
            
            if (_delayCounter >= 100) { Debug.LogError("Portal Data Fetch Timed Out"); return; }
            SendCustomEventDelayedSeconds(nameof(_DelayedTextFetch), 0.1f);
        }

        private void FetchData()
        {
            var content = _portalData.Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (content.Length < 3)
            {
                Debug.LogError($"Content length mismatch! Length was {content.Length}.");
                Debug.LogError($"Dumping Portal Data: {string.Join("\n", _portalData)}");
                return;
            }

            string ownerName = content[1];

            //Debug.Log($"Found Portal Owner Name: {ownerName} : after {_delayCounter} attempts.");
            //Debug.Log($"Did we place the portal?: {ownerName == Networking.LocalPlayer.displayName}");

            // Check if we placed the portal.
            if (ownerName != _localPlayer.displayName) return;

            Networking.SetOwner(_localPlayer, gameObject);
            _portalPosition = _portalTransform.position - _anchor.position;
            RequestSerialization();
            _portal.SetActive(true);
        }

        public override void OnDeserialization()
        {
            if (!_portalTransform) return;
            _portalTransform.position = _portalPosition + _anchor.position;
            _portal.SetActive(true);
        }
    }
}
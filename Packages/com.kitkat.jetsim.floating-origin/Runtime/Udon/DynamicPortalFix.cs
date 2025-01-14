
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
        
        private TextMeshProUGUI[] _portalTMPro;
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

            _portalTMPro = _portal.GetComponentsInChildren<TextMeshProUGUI>(true);
            if (_portalTMPro == null || _portalTMPro.Length == 0) { Debug.LogError("Couldn't find any TextMeshProUGUI components on the portal!", this); return; }
            if (_portalTMPro.Length < 2) { Debug.LogError("There were too few TMPro UGUI components on the portal. VRChat probably changed this again :("); }
            
            SendCustomEventDelayedSeconds(nameof(_DelayedTextFetch), 0.1f);
        }

        private int _delayCounter;
        public void _DelayedTextFetch()
        {
            _delayCounter++;
            _portalData = _portalTMPro[1].text; // 0: [world] 1: [owner] 2: [access] 3: [group] 4: [ageGate] 5: [0/10] 6: [countdown timer (30s)]
            
            // Check if the placeholder text has been replaced with the real content.
            if (!_portalData.StartsWith("[owner]")) { FetchData(); return; }
            
            if (_delayCounter >= 100) { Debug.LogError("Portal Data Fetch Timed Out"); return; }
            SendCustomEventDelayedSeconds(nameof(_DelayedTextFetch), 0.1f);
        }

        private void FetchData()
        {
            string ownerName = _portalData;

            Debug.Log($"Found Portal Owner Name: {ownerName} : after {_delayCounter} attempts.");

            // Check if we placed the portal.
            if (ownerName != _localPlayer.displayName) return;

            Debug.Log("The local player placed this portal! Syncing data...");
            
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
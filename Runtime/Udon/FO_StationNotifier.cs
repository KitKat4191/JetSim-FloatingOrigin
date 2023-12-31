
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace KitKat.JetSim.FloatingOrigin.Runtime
{
    // To ensure no sync mode conflicts occur, FO_BuildValidator.cs will automatically set the sync mode on this script to be the same as the other scripts.
    [AddComponentMenu("")] // Hides this script from the add component menu to reduce clutter.
    [DisallowMultipleComponent] // Prevents multiple instances of this script from being added to the same object.
    public class FO_StationNotifier : UdonSharpBehaviour
    {
        // Automatically set by VRRefAssist before entering play mode or building the project.
        [SerializeField, HideInInspector] private FO_Manager floatingOriginManager_SINGLETON;


        public override void OnStationEntered(VRCPlayerApi player)
        {
            if (!VRC.SDKBase.Utilities.IsValid(player)) return;
            if (!player.isLocal) return;
            floatingOriginManager_SINGLETON._ExternalStationEntered();
        }
        public override void OnStationExited(VRCPlayerApi player)
        {
            if (!VRC.SDKBase.Utilities.IsValid(player)) return;
            if (!player.isLocal) return;
            floatingOriginManager_SINGLETON._ExternalStationExited();
        }
    }
}
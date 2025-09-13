
using UnityEngine;
using VRC.SDKBase;

namespace KitKat.JetSim.FloatingOrigin.Runtime
{
    public static class FO_Utils
    {
        /// <summary>
        /// Teleports the <paramref name="player"/> to <paramref name="targetPos"/> without changing their rotation.
        /// </summary>
        /// <param name="player">The player to teleport, this has to be the local player.</param>
        /// <param name="targetPos">The position to teleport the player to.</param>
        /// <param name="lerpOnRemote">Whether to interpolate the player's movement for remote players.</param>
        public static void TeleportTo(this VRCPlayerApi player, Vector3 targetPos, bool lerpOnRemote = false)
        {
            VRCPlayerApi.TrackingData origin = player.GetTrackingData(VRCPlayerApi.TrackingDataType.Origin);

            Vector3 offset = player.GetPosition() - origin.position;
            Vector3 teleportPos = targetPos - offset;

            player.TeleportTo(teleportPos, origin.rotation,
                VRC_SceneDescriptor.SpawnOrientation.AlignRoomWithSpawnPoint, lerpOnRemote);
        }
    }
}

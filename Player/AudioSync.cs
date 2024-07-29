using Unity.Netcode;
using UnityEngine;

namespace AC {
    [RequireComponent(typeof(AudioSource))]
    public class AudioSync : NetworkBehaviour
    {
        public AudioSource audioSource;
        public AudioClip[] clips;

        public void PlaySound(int id) {
            if (id >= 0 && id < clips.Length) {
                PlaySoundServerRpc(id);
            }
        }

        public void ChangePitch(float pitch) {
            ChangePitchServerRpc(pitch);
        }

        [ServerRpc(RequireOwnership = false)]
        private void PlaySoundServerRpc(int id) {
            PlaySoundClientRpc(id);
        }

        [ClientRpc]
        private void PlaySoundClientRpc(int id) {
            audioSource.PlayOneShot(clips[id]);
        }

        [ServerRpc(RequireOwnership = false)]
        private void ChangePitchServerRpc(float pitch) {
            ChangePitchClientRpc(pitch);
        }

        [ClientRpc]
        private void ChangePitchClientRpc(float pitch) {
            audioSource.pitch = pitch;
        }
    }
}

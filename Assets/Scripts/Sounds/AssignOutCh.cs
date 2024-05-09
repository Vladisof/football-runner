using UnityEngine;
using UnityEngine.Audio;
namespace Sounds
{
    public class AssignOutCh : MonoBehaviour
    {
        public string mixerGroup;

        private void Awake()
        {
            AudioSource source = GetComponent<AudioSource>();

            if (source == null)
            {
                Destroy(this);
                return;
            }

            AudioMixerGroup[] groups = SoundPlayer.instance.mixer.FindMatchingGroups(mixerGroup);

            if(groups.Length == 0)
            {}

            foreach (AudioMixerGroup t in groups)
            {
                if (t.name != mixerGroup)
                {
                    continue;
                }

                source.outputAudioMixerGroup = t;
                break;
            }
        }
    }
}

using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    AudioMixer audioMixer;

    private void Start()
    {
        audioMixer = GetComponent<AudioMixer>();
    }
    public void AdjustMasterVolume(float value)
    {

    }
}

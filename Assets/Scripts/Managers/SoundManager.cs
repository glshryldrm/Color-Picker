using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SoundManager
{
    public static void PlaySound(GameAssets.SoundType soundType)
    {
        GameObject soundGameObject = new GameObject("Sound");
        AudioSource audioSource = soundGameObject.AddComponent<AudioSource>();
        audioSource.PlayOneShot(GetAudioClip(soundType));
        GameObject.Destroy(soundGameObject, 3f);

    }

    private static AudioClip GetAudioClip(GameAssets.SoundType soundType)
    {
        foreach (Sound audio in GameAssets.Instance.sounds)
        {
            if (audio.soundType == soundType && audio != null)
            {
                return audio.audio;
            }
        }
        Debug.LogError("Sound" + soundType + "not found!");
        return null;
    }
}

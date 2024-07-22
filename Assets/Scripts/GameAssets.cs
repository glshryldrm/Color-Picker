using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameAssets : MonoBehaviour
{
    private static GameAssets instance;
    public GameObject gridPrefab;
    public GameObject pawnPrefab;
    public GameObject particlePrefab;
    public enum SoundType
    {
        click,
        bubble
    }
    public List<Sound> sounds = new List<Sound>();
    public static GameAssets Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (Instantiate(Resources.Load("GameAssets")) as GameObject).GetComponent<GameAssets>();
            }

            return instance;
        }
    }
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
    
}
[System.Serializable]
public class Sound
{
    public GameAssets.SoundType soundType;
    public AudioClip audio;

    Sound(GameAssets.SoundType soundType, AudioClip audio)
    {
        soundType = this.soundType;
        audio = this.audio;
    }
}
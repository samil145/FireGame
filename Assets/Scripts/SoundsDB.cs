using System.Collections.Generic;
using UnityEngine;
using Utils;

public class SoundsDB : MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField] private List<Pair<string, AudioClip>> m_Sounds;
    [SerializeField] private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
    private static SoundsDB instance;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static SoundsDB Instance => instance;

    public void OnBeforeSerialize()
    {
        m_Sounds.Clear();
        foreach (var item in audioClips)
        {
            m_Sounds.Add(new Pair<string, AudioClip>(item.Key, item.Value));
        }
    }

    public void OnAfterDeserialize()
    {
        if (audioClips.Count != 0 && audioClips.Count < m_Sounds.Count)
        {
            m_Sounds[m_Sounds.Count - 1] = new Pair<string, AudioClip>("NewAudio", null);
        }
        audioClips.Clear();
        foreach (var item in m_Sounds)
        {
            audioClips.Add(item.Key, item.Value);
        }
    }

    public Dictionary<string, AudioClip> AudioClips => audioClips;
}

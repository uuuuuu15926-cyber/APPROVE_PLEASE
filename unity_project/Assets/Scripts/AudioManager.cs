using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class SoundEntry
    {
        public string key;       // "PaperShuffle", "StampHeavy", "BuzzerError", "ClockTick"
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
    }

    [SerializeField] private SoundEntry[] sounds;
    private AudioSource _source;
    private Dictionary<string, SoundEntry> _map = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _source = GetComponent<AudioSource>();
        if (_source == null) _source = gameObject.AddComponent<AudioSource>();
        _source.playOnAwake = false;

        foreach (var s in sounds)
            _map[s.key] = s;
    }

    public void Play(string key)
    {
        if (!_map.TryGetValue(key, out var entry)) return;
        _source.PlayOneShot(entry.clip, entry.volume);
    }
}
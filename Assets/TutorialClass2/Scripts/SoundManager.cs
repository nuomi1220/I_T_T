using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance = null;

    public Sound[] sounds;

    private Dictionary<string, AudioSource> audioSourcesDic;
    // 每个 owner（玩家）对应的 soundName -> AudioSource
    private Dictionary<GameObject, Dictionary<string, AudioSource>> ownerAudioSources = new Dictionary<GameObject, Dictionary<string, AudioSource>>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        audioSourcesDic = new Dictionary<string, AudioSource>();

        for (int i = 0; i < sounds.Length; i++)
        {
            GameObject soundGameObject = new GameObject("Sound_" + i + "_" + sounds[i].name);
            soundGameObject.transform.SetParent(this.transform);
            sounds[i].SetSource(soundGameObject.AddComponent<AudioSource>());
            audioSourcesDic.Add(sounds[i].name, soundGameObject.GetComponent<AudioSource>());
        }

        // 订阅死亡事件
    }

    // 现有全局播放（保持兼容）
    public void Play(string soundName, float volume = 1, bool loop = false)
    {
        if (!audioSourcesDic.ContainsKey(soundName)) return;
        AudioSource source = audioSourcesDic[soundName];
        if (!source.isPlaying)
        {
            source.volume = volume;
            source.loop = loop;
            source.Play();
        }
    }

    public void Pause(string soundName)
    {
        if (!audioSourcesDic.ContainsKey(soundName)) return;
        AudioSource source = audioSourcesDic[soundName];
        source.Pause();
    }

    public void UnPause(string soundName)
    {
        if (!audioSourcesDic.ContainsKey(soundName)) return;
        AudioSource source = audioSourcesDic[soundName];
        source.UnPause();
    }

    public void SetVolume(string soundName, float volume)
    {
        if (!audioSourcesDic.ContainsKey(soundName)) return;
        AudioSource source = audioSourcesDic[soundName];
        source.volume = volume;
    }

    public void Stop(string soundName)
    {
        if (!audioSourcesDic.ContainsKey(soundName)) return;
        AudioSource source = audioSourcesDic[soundName];
        source.Stop();
    }

    public void StopAll()
    {
        foreach (var audioSource in audioSourcesDic.Values)
        {
            audioSource.Stop();
        }

        // 停止并销毁 owner 的临时音源
        foreach (var kv in ownerAudioSources)
        {
            foreach (var s in kv.Value.Values)
            {
                if (s != null)
                {
                    // 这些 AudioSource 都挂在我们创建的子对象上，销毁子对象安全
                    Destroy(s.gameObject);
                }
            }
        }
        ownerAudioSources.Clear();
    }

    public void PlayerDeathSound()
    {
        SoundManager.Instance.Play("PlayerDeath");
    }

    // ---- 新增 API：在指定 GameObject 上播放/停止声音（适合脚步声等每人独立的循环声） ----

    // 在 owner 上播放 soundName（会在 owner 上创建或复用 AudioSource）
    public AudioSource PlayOn(GameObject owner, string soundName, float volume = 1f, bool loop = false, bool spatial = true)
    {
        if (owner == null || string.IsNullOrEmpty(soundName)) return null;

        var sound = FindSoundByName(soundName);
        if (sound == null || sound.clip == null) return null;

        if (!ownerAudioSources.TryGetValue(owner, out var dict))
        {
            dict = new Dictionary<string, AudioSource>();
            ownerAudioSources[owner] = dict;
        }

        if (dict.TryGetValue(soundName, out var existing) && existing != null)
        {
            existing.volume = volume;
            existing.loop = loop;
            if (!existing.isPlaying) existing.Play();
            return existing;
        }

        // 始终在 owner 下创建一个子对象来承载 AudioSource，避免影响 owner 本体
        GameObject go = new GameObject($"Audio_{soundName}");
        go.transform.SetParent(owner.transform);
        go.transform.localPosition = Vector3.zero;
        var src = go.AddComponent<AudioSource>();

        src.clip = sound.clip;
        src.volume = volume;
        src.loop = loop;
        src.playOnAwake = false;
        src.spatialBlend = spatial ? 1f : 0f;
        src.Play();

        dict[soundName] = src;
        return src;
    }

    // 停止并销毁 owner 上该名称的音源（如果存在）
    public void StopOn(GameObject owner, string soundName)
    {
        if (owner == null || string.IsNullOrEmpty(soundName)) return;

        if (ownerAudioSources.TryGetValue(owner, out var dict))
        {
            if (dict.TryGetValue(soundName, out var src) && src != null)
            {
                src.Stop();
                Destroy(src.gameObject);
            }
            dict.Remove(soundName);
            if (dict.Count == 0) ownerAudioSources.Remove(owner);
        }
    }

    // 为 owner 一次性播放（不循环），使用 PlayClipAtPoint（会自动销毁）
    public void PlayOneShotAt(GameObject owner, string soundName, float volume = 1f)
    {
        if (owner == null || string.IsNullOrEmpty(soundName)) return;
        var sound = FindSoundByName(soundName);
        if (sound == null || sound.clip == null) return;
        AudioSource.PlayClipAtPoint(sound.clip, owner.transform.position, volume);
    }

    // 查找 Sound 配置
    private Sound FindSoundByName(string soundName)
    {
        if (sounds == null) return null;
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i] != null && sounds[i].name == soundName) return sounds[i];
        }
        return null;
    }
}

[System.Serializable]
public class Sound
{
    public AudioClip clip;
    public string name;

    public float volume = 1;
    public bool loop = false;

    public Sound(string name, AudioClip clip)
    {
        this.name = name;
        this.clip = clip;
    }

    public void SetSource(AudioSource source)
    {
        source.clip = clip;
        source.volume = volume;
        source.loop = loop;
    }

    

}
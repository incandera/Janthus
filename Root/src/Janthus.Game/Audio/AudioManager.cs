using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace Janthus.Game.Audio;

public class AudioManager : IDisposable
{
    public float MasterVolume { get; set; } = 1.0f;
    public float MusicVolume { get; set; } = 0.7f;
    public float SfxVolume { get; set; } = 1.0f;
    public float AmbientVolume { get; set; } = 0.6f;

    private readonly Dictionary<SoundId, SoundEffect[]> _effects = new();
    private readonly Dictionary<MusicId, Song> _music = new();
    private readonly Random _rng = new();

    private SoundEffectInstance _ambientLoop;
    private MusicId _currentMusic = MusicId.None;

    // SoundId → (subfolder, base filename) mapping
    private static readonly Dictionary<SoundId, (string folder, string baseName)> SoundPaths = new()
    {
        [SoundId.MeleeHit] = ("combat", "melee_hit"),
        [SoundId.MeleeMiss] = ("combat", "melee_miss"),
        [SoundId.SpellCast] = ("combat", "spell_cast"),
        [SoundId.SpellFizzle] = ("combat", "spell_fizzle"),
        [SoundId.CombatStart] = ("combat", "combat_start"),
        [SoundId.Death] = ("combat", "death"),
        [SoundId.FootstepGrass] = ("footsteps", "grass"),
        [SoundId.FootstepStone] = ("footsteps", "stone"),
        [SoundId.FootstepDirt] = ("footsteps", "dirt"),
        [SoundId.FootstepSand] = ("footsteps", "sand"),
        [SoundId.FootstepWater] = ("footsteps", "water"),
        [SoundId.UISelect] = ("ui", "select"),
        [SoundId.UINavigate] = ("ui", "navigate"),
        [SoundId.UIOpen] = ("ui", "open"),
        [SoundId.UIClose] = ("ui", "close"),
        [SoundId.ItemPickup] = ("events", "item_pickup"),
        [SoundId.GoldReceive] = ("events", "gold_receive"),
        [SoundId.QuestAccepted] = ("events", "quest_accepted"),
        [SoundId.QuestCompleted] = ("events", "quest_completed"),
        [SoundId.FollowerJoined] = ("events", "follower_joined"),
        [SoundId.LevelUp] = ("events", "level_up"),
    };

    private static readonly Dictionary<MusicId, string> MusicPaths = new()
    {
        [MusicId.Menu] = "menu",
        [MusicId.Exploration] = "exploration",
        [MusicId.Combat] = "combat",
    };

    private static readonly Dictionary<string, SoundId> TileToFootstep = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Grass"] = SoundId.FootstepGrass,
        ["Stone"] = SoundId.FootstepStone,
        ["Dirt"] = SoundId.FootstepDirt,
        ["Sand"] = SoundId.FootstepSand,
        ["Water"] = SoundId.FootstepWater,
    };

    public void LoadAll(string contentRoot)
    {
        var audioRoot = Path.Combine(contentRoot, "audio");
        if (!Directory.Exists(audioRoot)) return;

        // Load sound effects
        foreach (var kvp in SoundPaths)
        {
            var folder = Path.Combine(audioRoot, kvp.Value.folder);
            if (!Directory.Exists(folder)) continue;

            var variants = new List<SoundEffect>();

            // Try numbered variants: baseName_01.wav, _02.wav, _03.wav
            for (int i = 1; i <= 10; i++)
            {
                var variantPath = Path.Combine(folder, $"{kvp.Value.baseName}_{i:D2}.wav");
                var effect = LoadWav(variantPath);
                if (effect != null)
                    variants.Add(effect);
            }

            // If no numbered variants, try bare name: baseName.wav
            if (variants.Count == 0)
            {
                var barePath = Path.Combine(folder, $"{kvp.Value.baseName}.wav");
                var effect = LoadWav(barePath);
                if (effect != null)
                    variants.Add(effect);
            }

            if (variants.Count > 0)
                _effects[kvp.Key] = variants.ToArray();
        }

        // Load music
        var musicFolder = Path.Combine(audioRoot, "music");
        if (Directory.Exists(musicFolder))
        {
            foreach (var kvp in MusicPaths)
            {
                var oggPath = Path.Combine(musicFolder, $"{kvp.Value}.ogg");
                if (File.Exists(oggPath))
                {
                    try
                    {
                        var song = Song.FromUri(kvp.Value, new Uri(oggPath, UriKind.Absolute));
                        _music[kvp.Key] = song;
                    }
                    catch
                    {
                        // Graceful fallback — no music for this track
                    }
                }
            }
        }
    }

    private static SoundEffect LoadWav(string path)
    {
        if (!File.Exists(path)) return null;
        try
        {
            using var stream = File.OpenRead(path);
            return SoundEffect.FromStream(stream);
        }
        catch
        {
            return null;
        }
    }

    public void PlaySound(SoundId id, float volume = 1f, float pitch = 0f)
    {
        if (!_effects.TryGetValue(id, out var variants)) return;

        var effect = variants[_rng.Next(variants.Length)];
        var finalVolume = Math.Clamp(volume * SfxVolume * MasterVolume, 0f, 1f);
        if (finalVolume <= 0f) return;

        effect.Play(finalVolume, Math.Clamp(pitch, -1f, 1f), 0f);
    }

    public void PlaySoundAtDistance(SoundId id, float tileDistance, float maxDistance = 15f)
    {
        if (tileDistance >= maxDistance) return;

        var attenuation = 1f - (tileDistance / maxDistance);
        attenuation = Math.Clamp(attenuation, 0f, 1f);
        PlaySound(id, attenuation);
    }

    public void PlayFootstep(string tileName)
    {
        if (!TileToFootstep.TryGetValue(tileName ?? "", out var soundId))
            soundId = SoundId.FootstepDirt;

        // Slight pitch variation for natural feel
        var pitch = (_rng.NextSingle() - 0.5f) * 0.2f; // +/- 0.1
        PlaySound(soundId, 1f, pitch);
    }

    public void PlayFootstepAtDistance(string tileName, float tileDistance, float maxDistance = 15f)
    {
        if (tileDistance >= maxDistance) return;

        if (!TileToFootstep.TryGetValue(tileName ?? "", out var soundId))
            soundId = SoundId.FootstepDirt;

        var attenuation = 1f - (tileDistance / maxDistance);
        attenuation = Math.Clamp(attenuation, 0f, 1f);
        var pitch = (_rng.NextSingle() - 0.5f) * 0.2f;
        PlaySound(soundId, attenuation, pitch);
    }

    public void PlayMusic(MusicId id)
    {
        if (id == _currentMusic) return;

        if (id == MusicId.None)
        {
            StopMusic();
            return;
        }

        if (!_music.TryGetValue(id, out var song))
        {
            _currentMusic = id;
            return;
        }

        try
        {
            MediaPlayer.Volume = Math.Clamp(MusicVolume * MasterVolume, 0f, 1f);
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(song);
            _currentMusic = id;
        }
        catch
        {
            // Graceful fallback
        }
    }

    public void StopMusic()
    {
        try
        {
            MediaPlayer.Stop();
        }
        catch
        {
            // Ignore
        }
        _currentMusic = MusicId.None;
    }

    public void UpdateMusicVolume()
    {
        try
        {
            MediaPlayer.Volume = Math.Clamp(MusicVolume * MasterVolume, 0f, 1f);
        }
        catch
        {
            // Ignore
        }
    }

    public void SetAmbient(SoundId id)
    {
        if (!_effects.TryGetValue(id, out var variants)) return;

        StopAmbient();

        var effect = variants[0];
        _ambientLoop = effect.CreateInstance();
        _ambientLoop.IsLooped = true;
        _ambientLoop.Volume = Math.Clamp(AmbientVolume * MasterVolume, 0f, 1f);
        _ambientLoop.Play();
    }

    public void StopAmbient()
    {
        if (_ambientLoop != null)
        {
            _ambientLoop.Stop();
            _ambientLoop.Dispose();
            _ambientLoop = null;
        }
    }

    public void PauseAll()
    {
        _ambientLoop?.Pause();
        try { MediaPlayer.Pause(); } catch { }
    }

    public void ResumeAll()
    {
        _ambientLoop?.Resume();
        try { MediaPlayer.Resume(); } catch { }
    }

    public void Dispose()
    {
        StopAmbient();
        StopMusic();

        foreach (var variants in _effects.Values)
        {
            foreach (var effect in variants)
                effect.Dispose();
        }
        _effects.Clear();
        _music.Clear();
    }
}

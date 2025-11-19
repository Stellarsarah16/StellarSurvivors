namespace StellarSurvivors.Core;
using Raylib_cs;

public static class AudioManager
    {
        private static Random _random = new Random();
        private static Music _currentMusic;
        private static bool _isMusicPlaying = false;

        /// <summary>
        /// Plays a Sound Effect once (Fire and Forget).
        /// </summary>
        public static void PlaySfx(string name, float volume = 1.0f, float pitch = 1.0f)
        {
            Sound sound = AssetManager.GetSound(name);
            // Raylib quirk: checking sound.Id isn't as straightforward as Textures,
            // but AssetManager returns default if missing.
            Raylib.SetSoundVolume(sound, volume);
            Raylib.SetSoundPitch(sound, pitch); // Reset pitch in case it was changed
            Raylib.PlaySound(sound);
        }

        /// <summary>
        /// Plays a sound with slight pitch variation.
        /// This makes rapid sounds (like footsteps or shooting) feel dynamic and organic.
        /// </summary>
        public static void PlaySfxDynamic(string name, float minPitch = 0.9f, float maxPitch = 1.1f)
        {
            Sound sound = AssetManager.GetSound(name);
            
            // Calculate random pitch
            float pitch = minPitch + ((float)_random.NextDouble() * (maxPitch - minPitch));
            
            Raylib.SetSoundPitch(sound, pitch);
            Raylib.PlaySound(sound);
        }

        /// <summary>
        /// Starts playing a background music track.
        /// </summary>
        public static void PlayMusic(string name)
        {
            Music music = AssetManager.GetMusic(name);
            
            // Stop old music if playing
            if (_isMusicPlaying)
            {
                Raylib.StopMusicStream(_currentMusic);
            }

            _currentMusic = music;
            Raylib.PlayMusicStream(_currentMusic);
            Raylib.SetMusicVolume(_currentMusic, 0.5f); // Default volume lower for BGM
            _isMusicPlaying = true;
        }

        /// <summary>
        /// MUST be called every frame in Game.Update() to keep music streaming!
        /// </summary>
        public static void Update()
        {
            if (_isMusicPlaying)
            {
                Raylib.UpdateMusicStream(_currentMusic);
            }
        }
    }
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Innkeeper.Time;

namespace Innkeeper.World
{
    /// <summary>
    /// Drives a URP 2D <see cref="Light2D"/> (Global) from the in-game clock.
    /// Color and intensity are interpolated between hard-coded keyframes
    /// (dawn / day / dusk / night) so the world fades smoothly across the day
    /// instead of snapping on hour boundaries.
    /// </summary>
    /// <remarks>
    /// Polls <see cref="TimeSystem.Now"/> in <c>Update</c> rather than
    /// subscribing to <c>OnHourChanged</c> — visual smoothness matters here, so
    /// per-frame polling is the right tool. This is the polling half of the
    /// hybrid consumption model in ADR-006.
    /// <para/>
    /// Keyframes are intentionally simple: dawn 05:00, day 07:30, dusk 18:00,
    /// night 21:00. If iteration on the values gets fiddly, promote them to a
    /// <c>DayNightProfile</c> ScriptableObject.
    /// </remarks>
    [RequireComponent(typeof(Light2D))]
    public sealed class DayNightLight : MonoBehaviour
    {
        // Keyframes: (hour-of-day [0..24], color, intensity)
        // The wrap-around between night (21:00) and dawn (05:00 next day) is
        // handled by re-emitting the dawn key at hour 29 for interpolation math.
        private struct Key
        {
            public float Hour;
            public Color Color;
            public float Intensity;
        }

        [Header("Keyframe overrides (optional)")]
        [SerializeField, Tooltip("Override the default dawn color. Leave at default to use the built-in palette.")]
        private bool useCustomKeys = false;

        [SerializeField] private Color dawnColor   = new Color(1.00f, 0.78f, 0.55f);
        [SerializeField, Range(0f, 2f)] private float dawnIntensity = 0.7f;

        [SerializeField] private Color dayColor    = new Color(1.00f, 0.98f, 0.92f);
        [SerializeField, Range(0f, 2f)] private float dayIntensity = 1.0f;

        [SerializeField] private Color duskColor   = new Color(1.00f, 0.65f, 0.45f);
        [SerializeField, Range(0f, 2f)] private float duskIntensity = 0.75f;

        [SerializeField] private Color nightColor  = new Color(0.40f, 0.50f, 0.75f);
        [SerializeField, Range(0f, 2f)] private float nightIntensity = 0.35f;

        [Header("Keyframe times (hour of day)")]
        [SerializeField, Range(0, 23)] private int dawnHour  = 5;
        [SerializeField, Range(0, 23)] private int dayHour   = 8;   // full brightness reached
        [SerializeField, Range(0, 23)] private int duskHour  = 18;
        [SerializeField, Range(0, 23)] private int nightHour = 21;

        private Light2D _light;
        private Key[] _keys;

        private void Awake()
        {
            _light = GetComponent<Light2D>();
            RebuildKeys();
        }

        private void OnValidate()
        {
            // Keep keys in sync if a designer tunes them in the inspector while playing.
            if (Application.isPlaying && _light != null) RebuildKeys();
        }

        private void RebuildKeys()
        {
            // Defaults are the field values themselves; useCustomKeys is purely
            // a documentation hint at the moment — left here so we can swap to a
            // SO-driven profile cleanly later.
            _ = useCustomKeys;

            _keys = new Key[]
            {
                new Key { Hour = dawnHour,       Color = dawnColor,   Intensity = dawnIntensity  },
                new Key { Hour = dayHour,        Color = dayColor,    Intensity = dayIntensity   },
                new Key { Hour = duskHour,       Color = duskColor,   Intensity = duskIntensity  },
                new Key { Hour = nightHour,      Color = nightColor,  Intensity = nightIntensity },
                // Wrap-around: dawn of "tomorrow" so we can interpolate across midnight.
                new Key { Hour = dawnHour + 24f, Color = dawnColor,   Intensity = dawnIntensity  },
            };
        }

        private void Update()
        {
            if (TimeSystem.Instance == null) return;

            var now = TimeSystem.Now;
            float hourFloat = now.Hour + now.Minute / 60f;
            ApplyForHour(hourFloat);
        }

        private void ApplyForHour(float hourFloat)
        {
            // If we're before the first key, shift forward by 24 to use the wrap-around segment.
            if (hourFloat < _keys[0].Hour) hourFloat += 24f;

            for (int i = 0; i < _keys.Length - 1; i++)
            {
                var a = _keys[i];
                var b = _keys[i + 1];
                if (hourFloat >= a.Hour && hourFloat <= b.Hour)
                {
                    float span = b.Hour - a.Hour;
                    float t = span <= 0f ? 0f : Mathf.InverseLerp(a.Hour, b.Hour, hourFloat);
                    _light.color     = Color.Lerp(a.Color, b.Color, t);
                    _light.intensity = Mathf.Lerp(a.Intensity, b.Intensity, t);
                    return;
                }
            }

            // Fallback: shouldn't hit if keys cover the full day; clamp to last.
            var last = _keys[_keys.Length - 1];
            _light.color = last.Color;
            _light.intensity = last.Intensity;
        }
    }
}

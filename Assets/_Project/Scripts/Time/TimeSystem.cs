using System;
using UnityEngine;

namespace Innkeeper.Time
{
    /// <summary>
    /// The authoritative in-game clock (see ADR-006).
    /// Singleton MonoBehaviour auto-instantiated on first scene load; survives
    /// scene transitions via <c>DontDestroyOnLoad</c>. Drives game time
    /// independently of physics timestep and renders.
    /// </summary>
    /// <remarks>
    /// Public consumption is one of:
    /// <list type="bullet">
    ///   <item><description>Polling — <see cref="Now"/> returns the current <see cref="GameTime"/> snapshot.</description></item>
    ///   <item><description>Events — <see cref="OnTick"/>, <see cref="OnHourChanged"/>, <see cref="OnDayChanged"/>, <see cref="OnSeasonChanged"/>.</description></item>
    /// </list>
    /// Subscribers MUST unsubscribe in <c>OnDisable</c> or they leak across scene reloads
    /// (same discipline as <c>InteractionRegistry</c>; see ADR-002 / ADR-006).
    /// </remarks>
    [DefaultExecutionOrder(-1000)] // tick before gameplay scripts so subscribers see the new time the same frame
    public sealed class TimeSystem : MonoBehaviour
    {
        // --- Singleton wiring ---

        private static TimeSystem _instance;

        public static TimeSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    Debug.LogError("[TimeSystem] Instance accessed before bootstrap. " +
                                   "This usually means RuntimeInitializeOnLoadMethod hasn't run yet.");
                }
                return _instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (_instance != null) return;

            var go = new GameObject("[TimeSystem]");
            DontDestroyOnLoad(go);
            _instance = go.AddComponent<TimeSystem>();
        }

        // --- Configuration ---

        [SerializeField, Tooltip("Tunables. If null, defaults from Resources/TimeConfig will be used; if that's missing, hard-coded defaults apply.")]
        private TimeConfig config;

        // --- Runtime state ---

        private long _totalMinutes;
        private float _tickAccumulator;
        private GameTime _last;
        private bool _initialized;

        /// <summary>Pause flag (clock only). Does NOT affect <c>Time.timeScale</c>.</summary>
        public bool IsPaused { get; set; }

        /// <summary>Current calendar snapshot. Cheap to read every frame.</summary>
        public static GameTime Now => _instance != null
            ? new GameTime(_instance._totalMinutes)
            : default;

        // --- Events ---

        /// <summary>Fires every tick (after <c>totalMinutes</c> has advanced).</summary>
        public static event Action<GameTime> OnTick;

        /// <summary>Fires when the hour value changes.</summary>
        public static event Action<GameTime> OnHourChanged;

        /// <summary>Fires when the day rolls over.</summary>
        public static event Action<GameTime> OnDayChanged;

        /// <summary>Fires when the season changes.</summary>
        public static event Action<Season> OnSeasonChanged;

        // --- Unity lifecycle ---

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            if (config == null)
            {
                // Try to find one shipped in Resources; if not, fall back to a runtime default.
                config = Resources.Load<TimeConfig>("TimeConfig");
                if (config == null)
                {
                    Debug.LogWarning("[TimeSystem] No TimeConfig asset found. Using runtime defaults. " +
                                     "Create one via Assets > Create > Innkeeper > Time Config and assign it.");
                    config = ScriptableObject.CreateInstance<TimeConfig>();
                }
            }

            _totalMinutes = config.ComputeStartTotalMinutes();
            _last = new GameTime(_totalMinutes);
            _initialized = true;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                // Note: static events are intentionally NOT cleared here. Subscribers manage their own
                // lifetimes (subscribe in OnEnable / unsubscribe in OnDisable). Clearing here would
                // strand subscribers that were planning to re-target the next TimeSystem after a
                // domain reload in the editor.
            }
        }

        private void Update()
        {
            if (!_initialized || IsPaused) return;

            _tickAccumulator += UnityEngine.Time.deltaTime;

            float tickInterval = Mathf.Max(0.05f, config.RealSecondsPerTick);
            while (_tickAccumulator >= tickInterval)
            {
                _tickAccumulator -= tickInterval;
                Tick(config.MinutesPerTick);
            }
        }

        // --- Internal mutation ---

        private void Tick(int minutes)
        {
            _totalMinutes += minutes;
            FireBoundaryEvents();
        }

        /// <summary>
        /// Jump the clock forward to an absolute target. Each crossed boundary
        /// (hour, day, season) fires its event EXACTLY ONCE, regardless of how
        /// many would have elapsed during a real tick-by-tick advance.
        /// </summary>
        /// <remarks>
        /// Used by sleep and other time-skip mechanics. Backwards travel is rejected.
        /// </remarks>
        public void AdvanceTo(long targetTotalMinutes)
        {
            if (targetTotalMinutes <= _totalMinutes)
            {
                if (targetTotalMinutes < _totalMinutes)
                {
                    Debug.LogWarning($"[TimeSystem] AdvanceTo({targetTotalMinutes}) ignored: " +
                                     $"target is in the past (now {_totalMinutes}).");
                }
                return;
            }

            _totalMinutes = targetTotalMinutes;
            FireBoundaryEvents();
        }

        /// <summary>Convenience wrapper around <see cref="AdvanceTo(long)"/>.</summary>
        public void AdvanceBy(long minutes) => AdvanceTo(_totalMinutes + Math.Max(0, minutes));

        /// <summary>
        /// Advance to the next occurrence of the given local hour (0..23).
        /// If the current hour is past the target, rolls to the next day.
        /// </summary>
        public void AdvanceToNext(int hour, int minute = 0)
        {
            if (hour < 0 || hour > 23) throw new ArgumentOutOfRangeException(nameof(hour));
            if (minute < 0 || minute > 59) throw new ArgumentOutOfRangeException(nameof(minute));

            var now = new GameTime(_totalMinutes);
            long currentDayStart = now.TotalDays * GameTime.MinutesPerDay;
            long targetThisDay = currentDayStart + hour * GameTime.MinutesPerHour + minute;

            long target = targetThisDay > _totalMinutes
                ? targetThisDay
                : targetThisDay + GameTime.MinutesPerDay;

            AdvanceTo(target);
        }

        // --- Boundary detection ---

        private void FireBoundaryEvents()
        {
            var now = new GameTime(_totalMinutes);

            // Order matters: tick first, then hour, day, season — coarsest last so handlers
            // higher up the chain see consistent state.
            try { OnTick?.Invoke(now); } catch (Exception e) { Debug.LogException(e); }

            if (now.Hour != _last.Hour || now.TotalDays != _last.TotalDays)
            {
                try { OnHourChanged?.Invoke(now); } catch (Exception e) { Debug.LogException(e); }
            }

            if (now.TotalDays != _last.TotalDays)
            {
                try { OnDayChanged?.Invoke(now); } catch (Exception e) { Debug.LogException(e); }
            }

            if (now.Season != _last.Season)
            {
                try { OnSeasonChanged?.Invoke(now.Season); } catch (Exception e) { Debug.LogException(e); }
            }

            _last = now;
        }

        // --- Save / Load surface (Phase 4) ---

        /// <summary>Returns the single number that captures the entire clock state.</summary>
        public long GetSaveState() => _totalMinutes;

        /// <summary>
        /// Restores the clock from a save. Does NOT fire boundary events — load is treated as
        /// a discontinuity, and subscribers re-initialize themselves from <see cref="Now"/>
        /// on their own <c>OnEnable</c>/load hooks.
        /// </summary>
        public void LoadSaveState(long totalMinutes)
        {
            if (totalMinutes < 0) throw new ArgumentOutOfRangeException(nameof(totalMinutes));
            _totalMinutes = totalMinutes;
            _last = new GameTime(_totalMinutes);
            _tickAccumulator = 0f;
        }
    }
}

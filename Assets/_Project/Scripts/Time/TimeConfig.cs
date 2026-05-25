using UnityEngine;

namespace Innkeeper.Time
{
    /// <summary>
    /// Designer-tunable configuration for <see cref="TimeSystem"/>.
    /// Lives as an asset at <c>Assets/_Project/ScriptableObjects/TimeConfig.asset</c>
    /// and is referenced by the runtime singleton.
    /// </summary>
    /// <remarks>
    /// Tick rate is unknowable until playtest — expect to tune
    /// <see cref="realSecondsPerTick"/> once guests exist (Phase 6).
    /// </remarks>
    [CreateAssetMenu(menuName = "Innkeeper/Time Config", fileName = "TimeConfig")]
    public sealed class TimeConfig : ScriptableObject
    {
        [Header("Tick rate")]
        [SerializeField, Tooltip("In-game minutes advanced per tick. 10 is Stardew-like.")]
        [Min(1)]
        private int minutesPerTick = 10;

        [SerializeField, Tooltip("Real seconds between ticks. ~1.0 gives a 24h day in ~144 real seconds.")]
        [Min(0.05f)]
        private float realSecondsPerTick = 1.0f;

        [Header("Starting date")]
        [SerializeField, Tooltip("Starting hour, 0..23.")]
        [Range(0, 23)]
        private int startHour = 6;

        [SerializeField, Tooltip("Starting minute, 0..59.")]
        [Range(0, 59)]
        private int startMinute = 0;

        [SerializeField, Tooltip("Starting day of the season, 1..28.")]
        [Range(1, GameTime.DaysPerSeason)]
        private int startDay = 1;

        [SerializeField, Tooltip("Starting season.")]
        private Season startSeason = Season.Spring;

        [SerializeField, Tooltip("Starting year (0-based).")]
        [Min(0)]
        private int startYear = 0;

        public int MinutesPerTick => minutesPerTick;
        public float RealSecondsPerTick => realSecondsPerTick;

        /// <summary>Computes the absolute <c>totalMinutes</c> value for the configured start date.</summary>
        public long ComputeStartTotalMinutes()
        {
            long days =
                (long)startYear * GameTime.DaysPerYear +
                (long)startSeason * GameTime.DaysPerSeason +
                (startDay - 1);

            return days * GameTime.MinutesPerDay
                 + (long)startHour * GameTime.MinutesPerHour
                 + startMinute;
        }

        private void OnValidate()
        {
            if (minutesPerTick < 1) minutesPerTick = 1;
            if (realSecondsPerTick < 0.05f) realSecondsPerTick = 0.05f;
            if (startDay < 1) startDay = 1;
            if (startDay > GameTime.DaysPerSeason) startDay = GameTime.DaysPerSeason;
        }
    }
}

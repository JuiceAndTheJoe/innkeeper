using System;

namespace Innkeeper.Time
{
    /// <summary>
    /// Immutable view of the in-game calendar at a single moment.
    /// All fields are computed from <see cref="TotalMinutes"/>; this struct
    /// stores nothing else, which keeps it cheap to pass around and copy.
    /// </summary>
    /// <remarks>
    /// The calendar shape is fixed by ADR-006:
    /// 60 min/hour, 24 hr/day, 7 days/week, 4 weeks/season (28 days), 4 seasons/year.
    /// Changing these constants is a save-breaking change.
    /// </remarks>
    public readonly struct GameTime : IEquatable<GameTime>, IComparable<GameTime>
    {
        public const int MinutesPerHour = 60;
        public const int HoursPerDay = 24;
        public const int MinutesPerDay = MinutesPerHour * HoursPerDay;     // 1440
        public const int DaysPerWeek = 7;
        public const int DaysPerSeason = 28;
        public const int SeasonsPerYear = 4;
        public const int DaysPerYear = DaysPerSeason * SeasonsPerYear;     // 112

        /// <summary>Total in-game minutes elapsed since the calendar epoch (Year 0, Spring 1, 00:00).</summary>
        public long TotalMinutes { get; }

        public GameTime(long totalMinutes)
        {
            if (totalMinutes < 0)
                throw new ArgumentOutOfRangeException(nameof(totalMinutes), "Game time cannot be negative.");
            TotalMinutes = totalMinutes;
        }

        // --- Calendar views (all derived, no storage) ---

        /// <summary>Minute within the current hour, 0..59.</summary>
        public int Minute => (int)(TotalMinutes % MinutesPerHour);

        /// <summary>Hour within the current day, 0..23.</summary>
        public int Hour => (int)((TotalMinutes / MinutesPerHour) % HoursPerDay);

        /// <summary>Total days elapsed since epoch, 0-based.</summary>
        public long TotalDays => TotalMinutes / MinutesPerDay;

        /// <summary>Day-of-season, 1..28 (1-based for human display).</summary>
        public int Day => (int)(TotalDays % DaysPerSeason) + 1;

        /// <summary>Day-of-week, 0..6 (0 = first day of the week).</summary>
        public int DayOfWeek => (int)(TotalDays % DaysPerWeek);

        public Season Season => (Season)((TotalDays / DaysPerSeason) % SeasonsPerYear);

        /// <summary>Year since epoch, 0-based.</summary>
        public int Year => (int)(TotalDays / DaysPerYear);

        // --- Convenience queries ---

        /// <summary>
        /// True if the current time-of-day falls within [startHour, endHour).
        /// Handles overnight ranges (e.g. <c>IsBetween(22, 6)</c> covers 22:00..05:59).
        /// </summary>
        public bool IsBetween(int startHour, int endHour)
        {
            if (startHour < 0 || startHour > 23) throw new ArgumentOutOfRangeException(nameof(startHour));
            if (endHour < 0 || endHour > 24)     throw new ArgumentOutOfRangeException(nameof(endHour));

            int h = Hour;
            return startHour <= endHour
                ? h >= startHour && h < endHour
                : h >= startHour || h < endHour;
        }

        /// <summary>Returns a new GameTime advanced by the given number of minutes.</summary>
        public GameTime AddMinutes(long minutes) => new GameTime(TotalMinutes + minutes);

        // --- Equality & comparison ---

        public bool Equals(GameTime other) => TotalMinutes == other.TotalMinutes;
        public override bool Equals(object obj) => obj is GameTime other && Equals(other);
        public override int GetHashCode() => TotalMinutes.GetHashCode();
        public int CompareTo(GameTime other) => TotalMinutes.CompareTo(other.TotalMinutes);

        public static bool operator ==(GameTime a, GameTime b) => a.Equals(b);
        public static bool operator !=(GameTime a, GameTime b) => !a.Equals(b);
        public static bool operator <(GameTime a, GameTime b)  => a.TotalMinutes <  b.TotalMinutes;
        public static bool operator >(GameTime a, GameTime b)  => a.TotalMinutes >  b.TotalMinutes;
        public static bool operator <=(GameTime a, GameTime b) => a.TotalMinutes <= b.TotalMinutes;
        public static bool operator >=(GameTime a, GameTime b) => a.TotalMinutes >= b.TotalMinutes;

        public override string ToString() =>
            $"Y{Year} {Season} {Day:00} {Hour:00}:{Minute:00}";
    }

    public enum Season
    {
        Spring = 0,
        Summer = 1,
        Autumn = 2,
        Winter = 3
    }
}

using UnityEngine;
using Innkeeper.Interactions;
using Innkeeper.Time;

namespace Innkeeper.World
{
    /// <summary>
    /// A bed the player can sleep in. Interacting between
    /// <see cref="sleepStartHour"/> and <see cref="wakeHour"/> skips time to
    /// the configured wake hour the next morning. Outside that window the
    /// interaction shows a "too early" prompt and refuses.
    /// </summary>
    /// <remarks>
    /// Time skip goes through <see cref="TimeSystem.AdvanceToNext(int, int)"/>,
    /// which fires each boundary event (hour/day/season) exactly once regardless
    /// of how many in-game hours pass — see ADR-006.
    /// </remarks>
    public sealed class Bed : Interactable
    {
        [Header("Sleep window")]
        [SerializeField, Tooltip("Earliest hour the player can sleep (inclusive). 18 = 6 PM.")]
        [Range(0, 23)]
        private int sleepStartHour = 18;

        [SerializeField, Tooltip("Latest hour the player can sleep (exclusive). 6 = 6 AM the next morning.")]
        [Range(0, 23)]
        private int wakeHour = 6;

        [Header("Prompts")]
        [SerializeField] private string promptReady = "Sleep";
        [SerializeField] private string promptTooEarly = "Too early to sleep";

        public override string Prompt =>
            IsSleepWindow(TimeSystem.Now) ? promptReady : promptTooEarly;

        public override bool CanInteract => IsSleepWindow(TimeSystem.Now);
        public override bool ShowPrompt => true;

        public override void OnInteract()
        {
            var now = TimeSystem.Now;
            if (!IsSleepWindow(now))
            {
                Debug.Log($"[Bed] Refused sleep at {now} — outside window {sleepStartHour:00}..{wakeHour:00}.");
                return;
            }

            var ts = TimeSystem.Instance;
            if (ts == null)
            {
                Debug.LogError("[Bed] TimeSystem.Instance is null — cannot sleep.");
                return;
            }

            ts.AdvanceToNext(wakeHour);
            Debug.Log($"[Bed] Slept until {TimeSystem.Now}.");
        }

        private bool IsSleepWindow(GameTime t) => t.IsBetween(sleepStartHour, wakeHour);
    }
}

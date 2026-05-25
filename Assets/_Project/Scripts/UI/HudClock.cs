using TMPro;
using UnityEngine;
using Innkeeper.Time;

namespace Innkeeper.UI
{
    /// <summary>
    /// Screen-space HUD clock. Subscribes to <see cref="TimeSystem.OnTick"/>
    /// (event-driven, not per-frame polling) and renders the current time on a
    /// single TextMeshPro label.
    /// </summary>
    /// <remarks>
    /// Lives on a screen-space-overlay Canvas — distinct from the world-space
    /// <c>WorldUI</c> Canvas used by interaction prompts (see ADR-003).
    /// Subscribe/unsubscribe discipline mirrors <c>InteractionRegistry</c> /
    /// ADR-002: subscribe in <c>OnEnable</c>, unsubscribe in <c>OnDisable</c>.
    /// </remarks>
    [RequireComponent(typeof(TMP_Text))]
    public sealed class HudClock : MonoBehaviour
    {
        [SerializeField, Tooltip("Show year in the secondary line. Off until Phase 2 testing wraps.")]
        private bool showYear = false;

        [SerializeField, Tooltip("Optional secondary label for date / day-of-week. Leave empty if you only want the time line.")]
        private TMP_Text secondaryLabel;

        private TMP_Text _label;

        private static readonly string[] DayNames =
            { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

        private void Awake()
        {
            _label = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            TimeSystem.OnTick += Render;
            // Render once immediately so the clock isn't blank for the first tick interval.
            if (TimeSystem.Instance != null) Render(TimeSystem.Now);
        }

        private void OnDisable()
        {
            TimeSystem.OnTick -= Render;
        }

        private void Render(GameTime t)
        {
            _label.text = $"{t.Hour:00}:{t.Minute:00}";

            if (secondaryLabel != null)
            {
                string day = DayNames[t.DayOfWeek];
                secondaryLabel.text = showYear
                    ? $"{day} · {t.Season} {t.Day} · Y{t.Year}"
                    : $"{day} · {t.Season} {t.Day}";
            }
        }
    }
}

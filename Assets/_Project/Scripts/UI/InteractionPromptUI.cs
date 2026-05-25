using TMPro;
using UnityEngine;
using Innkeeper.Player;
using Innkeeper.Interactions;

namespace Innkeeper.UI
{
    /// <summary>
    /// Displays a floating prompt above whatever Interactable the player
    /// is currently targeting. Hides itself when no target is in range.
    ///
    /// Lives on a world-space Canvas child. Repositions itself each frame
    /// based on the player's PlayerInteraction.CurrentTarget.
    /// </summary>
    public class InteractionPromptUI : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The player's PlayerInteraction component (drag the Player GameObject here).")]
        [SerializeField] private PlayerInteraction playerInteraction;

        [Tooltip("The TextMeshPro text element that shows the prompt string.")]
        [SerializeField] private TMP_Text promptText;

        [Header("Display")]
        [Tooltip("World-space offset from the interactable's position. " +
                 "Positive Y puts the prompt above the object.")]
        [SerializeField] private Vector3 worldOffset = new Vector3(0, 0.7f, 0);

        [Tooltip("Prefix shown before the prompt verb, e.g. '[E] '.")]
        [SerializeField] private string keyPrefix = "[E] ";

        private CanvasGroup canvasGroup;

        private void Awake()
        {
            // CanvasGroup lets us fade/show/hide cleanly without disabling
            // the whole GameObject (which would block Update from running).
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            SetVisible(false);
        }

        private void LateUpdate()
        {
            // LateUpdate so we run AFTER PlayerInteraction.Update has refreshed
            // its targets for this frame. Avoids one frame of stale state.
            if (playerInteraction == null)
            {
                SetVisible(false);
                return;
            }

            Interactable visible = playerInteraction.CurrentVisible;
            if (visible == null)
            {
                SetVisible(false);
                return;
            }

            SetVisible(true);
            // Only show the [E] key hint when the interactable is actually actionable.
            bool actionable = playerInteraction.CurrentTarget == visible;
            promptText.text = actionable ? keyPrefix + visible.Prompt : visible.Prompt;
            transform.position = visible.transform.position + worldOffset;
        }

        private void SetVisible(bool visible)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
        }
    }
}
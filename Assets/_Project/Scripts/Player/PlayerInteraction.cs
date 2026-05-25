using UnityEngine;
using UnityEngine.InputSystem;
using Innkeeper.Core;
using Innkeeper.Interactions;

namespace Innkeeper.Player
{
    /// <summary>
    /// Detects what Interactable (if any) is on the tile directly in front
    /// of the player, and triggers OnInteract() when the player presses
    /// the Interact key.
    ///
    /// Requires a GridActor (typically PlayerMovement) on the same GameObject.
    /// </summary>
    [RequireComponent(typeof(GridActor))]
    public class PlayerInteraction : MonoBehaviour
    {
        private GridActor actor;
        private PlayerControls controls;

        /// <summary>
        /// The Interactable currently being looked at AND actionable. Null if
        /// nothing is in front of the player, or if the thing in front returns
        /// CanInteract == false. The Interact key uses this.
        /// </summary>
        public Interactable CurrentTarget { get; private set; }

        /// <summary>
        /// The Interactable currently being looked at, regardless of whether it's
        /// actionable. UI reads this to decide whether to show a prompt at all;
        /// it pairs with CurrentTarget to decide whether to show the [E] key hint.
        /// </summary>
        public Interactable CurrentVisible { get; private set; }

        private void Awake()
        {
            actor = GetComponent<GridActor>();
            controls = new PlayerControls();
            controls.Player.Interact.performed += OnInteractPressed;
        }

        private void OnEnable() => controls.Player.Enable();
        private void OnDisable() => controls.Player.Disable();

        private void OnDestroy()
        {
            controls.Player.Interact.performed -= OnInteractPressed;
        }

        private void Update()
        {
            // Track what's in front of us every frame so UI stays responsive.
            // CurrentVisible answers "is there something to describe?";
            // CurrentTarget answers "is there something to act on?".
            Interactable found = InteractionRegistry.GetAt(actor.TileInFront);
            CurrentVisible = (found != null && found.ShowPrompt) ? found : null;
            CurrentTarget  = (found != null && found.CanInteract) ? found : null;
        }

        private void OnInteractPressed(InputAction.CallbackContext ctx)
        {
            if (CurrentTarget != null)
            {
                CurrentTarget.OnInteract();
            }
        }
    }
}
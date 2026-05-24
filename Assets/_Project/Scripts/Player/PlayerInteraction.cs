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
        /// The Interactable currently being looked at, or null. UI can read
        /// this to show/hide a prompt.
        /// </summary>
        public Interactable CurrentTarget { get; private set; }

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
            Interactable found = InteractionRegistry.GetAt(actor.TileInFront);
            CurrentTarget = (found != null && found.CanInteract) ? found : null;
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
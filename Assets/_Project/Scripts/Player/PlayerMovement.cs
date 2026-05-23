using UnityEngine;
using UnityEngine.InputSystem;
using Innkeeper.Core;

namespace Innkeeper.Player
{
    /// <summary>
    /// Input-driven controller. Reads WASD/stick input and asks the
    /// underlying GridActor to step one tile at a time. Continuous input
    /// produces continuous stepping (one tile per arrival).
    /// </summary>
    public class PlayerMovement : GridActor
    {
        private PlayerControls controls;
        private Vector2 inputDirection;

        protected override void Awake()
        {
            base.Awake();
            controls = new PlayerControls();
        }

        private void OnEnable() => controls.Player.Enable();
        private void OnDisable() => controls.Player.Disable();

        private void Update()
        {
            inputDirection = controls.Player.Move.ReadValue<Vector2>();
        }

        protected override void FixedUpdate()
        {
            // If we've finished the previous step and input is still held,
            // start a new step in the dominant input direction.
            if (!IsMoving && inputDirection != Vector2.zero)
            {
                TryStep(DominantCardinal(inputDirection));
            }

            // Let the base class handle the actual smooth motion.
            base.FixedUpdate();
        }

        /// <summary>
        /// Reduces a free 2D input to one of the four cardinal directions
        /// (no diagonals). Picks the axis with larger magnitude.
        /// </summary>
        private static Vector2Int DominantCardinal(Vector2 input)
        {
            if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
                return new Vector2Int((int)Mathf.Sign(input.x), 0);
            else
                return new Vector2Int(0, (int)Mathf.Sign(input.y));
        }
    }
}
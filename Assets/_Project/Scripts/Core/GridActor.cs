using UnityEngine;

namespace Innkeeper.Core
{
    /// <summary>
    /// Base class for anything that moves on the world's tile grid:
    /// player, staff, guests, animals.
    ///
    /// Subclasses decide WHEN and WHERE to move by calling TryStep().
    /// This class handles HOW: smoothing between tiles, snapping on spawn,
    /// and (later) collision checks against the world.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class GridActor : MonoBehaviour
    {
        [Header("Grid Movement")]
        [Tooltip("Tiles per second when moving.")]
        [SerializeField] protected float moveSpeed = 4f;

        [Tooltip("World-space size of one tile. Keep at 1 for now.")]
        [SerializeField] protected float tileSize = 1f;

        protected Rigidbody2D rb;
        protected Vector2 targetPosition;

        /// <summary>True while the actor is moving between tiles.</summary>
        public bool IsMoving => (Vector2)rb.position != targetPosition;

        /// <summary>The tile the actor is currently standing on (or moving toward).</summary>
        public Vector2Int CurrentTile =>
            new Vector2Int(
                Mathf.RoundToInt(targetPosition.x / tileSize),
                Mathf.RoundToInt(targetPosition.y / tileSize));

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            targetPosition = SnapToGrid(rb.position);
            rb.position = targetPosition;
        }

        protected virtual void FixedUpdate()
        {
            // Smoothly slide toward target tile.
            Vector2 newPos = Vector2.MoveTowards(
                rb.position,
                targetPosition,
                moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);
        }

        /// <summary>
        /// Request a one-tile step in a cardinal direction. Ignored if
        /// already mid-step. Returns true if the step was accepted.
        /// </summary>
        /// <param name="direction">Should be (1,0), (-1,0), (0,1), or (0,-1).</param>
        public bool TryStep(Vector2Int direction)
        {
            if (IsMoving) return false;
            if (direction == Vector2Int.zero) return false;

            // TODO: when we have a Tilemap with collision, check the target
            // tile here and reject the step if blocked. For now, all moves
            // are allowed.
            Vector2 step = new Vector2(direction.x, direction.y) * tileSize;
            targetPosition = (Vector2)rb.position + step;
            return true;
        }

        protected Vector2 SnapToGrid(Vector2 worldPos)
        {
            return new Vector2(
                Mathf.Round(worldPos.x / tileSize) * tileSize,
                Mathf.Round(worldPos.y / tileSize) * tileSize);
        }
    }
}
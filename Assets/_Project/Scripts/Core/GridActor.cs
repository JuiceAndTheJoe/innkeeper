using UnityEngine;

namespace Innkeeper.Core
{
    /// <summary>
    /// Base class for anything that moves on the world's tile grid:
    /// player, staff, guests, animals.
    ///
    /// Subclasses decide WHEN and WHERE to move by calling TryStep().
    /// This class handles HOW: smoothing between tiles, snapping on spawn,
    /// collision checks, and tracking which way the actor is facing.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class GridActor : MonoBehaviour
    {
        [Header("Grid Movement")]
        [Tooltip("Tiles per second when moving.")]
        [SerializeField] protected float moveSpeed = 4f;

        [Tooltip("World-space size of one tile. Keep at 1 for now.")]
        [SerializeField] protected float tileSize = 1f;

        [Header("Collision")]
        [Tooltip("Layers that block movement (e.g. Walls).")]
        [SerializeField] protected LayerMask blockingLayers;

        [Tooltip("Radius of the overlap check at the target tile center. " +
                 "Slightly smaller than half a tile to avoid catching adjacent walls.")]
        [SerializeField] protected float collisionCheckRadius = 0.4f;

        protected Rigidbody2D rb;
        protected Vector2 targetPosition;

        /// <summary>Last cardinal direction the actor moved or tried to move. Defaults to down.</summary>
        public Vector2Int Facing { get; protected set; } = Vector2Int.down;

        /// <summary>True while the actor is moving between tiles.</summary>
        public bool IsMoving => (Vector2)rb.position != targetPosition;

        /// <summary>The tile the actor is currently on (or moving toward).</summary>
        public Vector2Int CurrentTile =>
            new Vector2Int(
                Mathf.RoundToInt(targetPosition.x / tileSize),
                Mathf.RoundToInt(targetPosition.y / tileSize));

        /// <summary>The tile directly in front of the actor.</summary>
        public Vector2Int TileInFront => CurrentTile + Facing;

        /// <summary>The world-space center of the tile in front of the actor.</summary>
        public Vector2 PositionInFront => (Vector2)TileInFront * tileSize;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            targetPosition = SnapToGrid(rb.position);
            rb.position = targetPosition;
        }

        protected virtual void FixedUpdate()
        {
            Vector2 newPos = Vector2.MoveTowards(
                rb.position,
                targetPosition,
                moveSpeed * Time.fixedDeltaTime);
            rb.MovePosition(newPos);
        }

        /// <summary>
        /// Request a one-tile step in a cardinal direction. Updates Facing
        /// even if the step is blocked (so the actor turns to face walls).
        /// Returns true if the step was accepted.
        /// </summary>
        public bool TryStep(Vector2Int direction)
        {
            if (direction == Vector2Int.zero) return false;

            // Update facing regardless of whether we actually move.
            Facing = direction;

            if (IsMoving) return false;

            Vector2 step = new Vector2(direction.x, direction.y) * tileSize;
            Vector2 candidate = (Vector2)rb.position + step;

            if (IsBlocked(candidate)) return false;

            targetPosition = candidate;
            return true;
        }

        /// <summary>
        /// Returns true if the given world-space position overlaps anything
        /// on a blocking layer.
        /// </summary>
        protected bool IsBlocked(Vector2 worldPos)
        {
            return Physics2D.OverlapCircle(worldPos, collisionCheckRadius, blockingLayers) != null;
        }

        protected Vector2 SnapToGrid(Vector2 worldPos)
        {
            return new Vector2(
                Mathf.Round(worldPos.x / tileSize) * tileSize,
                Mathf.Round(worldPos.y / tileSize) * tileSize);
        }
    }
}
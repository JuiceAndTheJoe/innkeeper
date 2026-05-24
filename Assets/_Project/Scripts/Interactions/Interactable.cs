using UnityEngine;

namespace Innkeeper.Interactions
{
    /// <summary>
    /// Base class for any object the player can interact with.
    /// Place on a GameObject sitting at a tile center. Subclasses override
    /// OnInteract() to define what actually happens.
    ///
    /// Interactables register themselves with InteractionRegistry on enable
    /// so the player's InteractionDetector can find them efficiently without
    /// per-frame scene scans.
    /// </summary>
    public abstract class Interactable : MonoBehaviour
    {
        [Header("Interaction")]
        [Tooltip("World-space size of one tile. Used to compute occupied tile.")]
        [SerializeField] protected float tileSize = 1f;

        [Tooltip("Short prompt shown when the player is near. e.g. 'Repair'.")]
        [SerializeField] protected string prompt = "Interact";

        /// <summary>The tile this interactable occupies, rounded from its world position.</summary>
        public Vector2Int OccupiedTile =>
            new Vector2Int(
                Mathf.RoundToInt(transform.position.x / tileSize),
                Mathf.RoundToInt(transform.position.y / tileSize));

        /// <summary>Text the UI should show when this interactable is targeted.</summary>
        public string Prompt => prompt;

        /// <summary>Whether the player can currently interact with this object.</summary>
        public virtual bool CanInteract => true;

        protected virtual void OnEnable() => InteractionRegistry.Register(this);
        protected virtual void OnDisable() => InteractionRegistry.Unregister(this);

        /// <summary>
        /// Called when the player presses the interact key while facing this object.
        /// Subclasses override to implement their behavior.
        /// </summary>
        public abstract void OnInteract();
    }
}
using System.Collections.Generic;
using UnityEngine;

namespace Innkeeper.Interactions
{
    /// <summary>
    /// Static registry mapping tile coordinates to the Interactable on that tile.
    /// Interactables register/unregister themselves in OnEnable/OnDisable.
    /// The player's InteractionDetector queries this to find what's in front
    /// of the player.
    ///
    /// One Interactable per tile is enforced (last write wins, with a warning).
    /// </summary>
    public static class InteractionRegistry
    {
        private static readonly Dictionary<Vector2Int, Interactable> tileToInteractable
            = new Dictionary<Vector2Int, Interactable>();

        public static void Register(Interactable interactable)
        {
            Vector2Int tile = interactable.OccupiedTile;
            if (tileToInteractable.TryGetValue(tile, out var existing) && existing != interactable)
            {
                Debug.LogWarning(
                    $"[InteractionRegistry] Tile {tile} already occupied by '{existing.name}'. " +
                    $"Overwriting with '{interactable.name}'.",
                    interactable);
            }
            tileToInteractable[tile] = interactable;
        }

        public static void Unregister(Interactable interactable)
        {
            Vector2Int tile = interactable.OccupiedTile;
            if (tileToInteractable.TryGetValue(tile, out var existing) && existing == interactable)
            {
                tileToInteractable.Remove(tile);
            }
        }

        /// <summary>
        /// Returns the Interactable at the given tile, or null if none.
        /// </summary>
        public static Interactable GetAt(Vector2Int tile)
        {
            tileToInteractable.TryGetValue(tile, out var result);
            return result;
        }
    }
}
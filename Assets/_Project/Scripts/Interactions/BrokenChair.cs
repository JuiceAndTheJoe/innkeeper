using UnityEngine;

namespace Innkeeper.Interactions
{
    /// <summary>
    /// A broken chair the player can repair. On interaction, the chair
    /// changes color to indicate it's fixed. Placeholder behavior — later
    /// this will check inventory for materials, play an animation, etc.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class BrokenChair : Interactable
    {
        [Header("Visuals")]
        [SerializeField] private Color brokenColor = new Color(0.5f, 0.3f, 0.2f); // dull brown
        [SerializeField] private Color repairedColor = new Color(0.9f, 0.7f, 0.4f); // warm wood

        private SpriteRenderer sr;
        private bool isRepaired;

        public override bool CanInteract => !isRepaired;

        protected override void OnEnable()
        {
            base.OnEnable();
            sr = GetComponent<SpriteRenderer>();
            UpdateVisual();
        }

        public override void OnInteract()
        {
            if (isRepaired) return;
            isRepaired = true;
            UpdateVisual();
            Debug.Log($"[{name}] Repaired!");
        }

        private void UpdateVisual()
        {
            if (sr != null)
            {
                sr.color = isRepaired ? repairedColor : brokenColor;
            }
        }
    }
}
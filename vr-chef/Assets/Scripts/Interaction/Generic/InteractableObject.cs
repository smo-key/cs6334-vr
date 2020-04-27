using UnityEngine;
using UnityEditor;
using System;

namespace Assets.Scripts.Interaction.Generic
{
    /// <summary>
    /// Represents an object with an interaction behavior. 
    /// When the user is within a certain distance of the object, the object can be grabbed by the user.
    /// </summary>
    public abstract class InteractableObject : MonoBehaviour
    {
        protected virtual Renderer ObjectRenderer { get; private set; }

        protected static Color DefaultOutlineColor = Color.white;
        protected static Color SelectedOutlineColor = new Color(255.0f / 255.0f, 228.0f / 255.0f, 0.0f / 255.0f);

        public Color? MaterialTintOverride
        {
            get => _materialTintOverride;
            set {
                _materialTintOverride = value;
                UpdateMaterial();
            }
        }
        private Color? _materialTintOverride = null;

        public bool IsNearHand { get; private set; } = false;


        protected virtual float SelectedOutlineMultiplier => 2.0f;

        float defaultOutlineWidth;

        protected void UpdateMaterial()
        {
            Color tint = MaterialTintOverride.HasValue ? MaterialTintOverride.Value : (IsNearHand ? SelectedOutlineColor : DefaultOutlineColor);

            foreach (var material in ObjectRenderer.materials)
            {
                material.SetColor("_Tint", tint);
                material.SetColor("_OutlineColor", tint);
                material.SetFloat("_OutlineWidth", IsNearHand ? defaultOutlineWidth * SelectedOutlineMultiplier : defaultOutlineWidth);
            }
        }

        public virtual void Start()
        {
            ObjectRenderer = gameObject.GetComponentInChildren<Renderer>();
            if (!ObjectRenderer) throw new Exception("Interactable object does not have a renderer node within itself or any of its children.");

            defaultOutlineWidth = ObjectRenderer.material.GetFloat("_OutlineWidth");
            UpdateMaterial();
        }

        public virtual void OnFrame(InteractionController controller)
        {
            // do nothing
        }

        public virtual void OnEnterClosest(InteractionController controller)
        {
            //don't do anything if there's an object in the hand
            if (controller.ControlledObject) return;

            IsNearHand = true;
            UpdateMaterial();
        }    

        public virtual void OnLeaveClosest(InteractionController controller)
        {
            IsNearHand = false;
            UpdateMaterial();
        }

        public virtual void OnGrab(InteractionController controller)
        {
            IsNearHand = false;
            UpdateMaterial();
        }

        public virtual void OnDrop(InteractionController controller)
        {
            IsNearHand = false;
            UpdateMaterial();
        }
    }
}
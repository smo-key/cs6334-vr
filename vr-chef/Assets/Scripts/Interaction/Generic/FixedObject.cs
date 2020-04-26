using UnityEngine;
using System.Collections;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Assets.Scripts.Interaction.Generic
{
    /// <summary>
    /// A grabbable object can be carried by a user. To override the "carry" behavior, 
    /// </summary>
    public abstract class FixedObject : InteractableObject
    {
        protected bool isGrabbed = false;
        protected Vector3 originalRotationOnGrab;

        public override void Start()
        {
            base.Start();
        }

        public override void OnFrame(InteractionController controller)
        {
            base.OnFrame(controller);

            if (isGrabbed)
            {
                var hand = controller.GetHand();
                OnFrameWhenGrabbed(controller, hand);
            }
        }

        public abstract void OnFrameWhenGrabbed(InteractionController controller, GameObject hand);

        public override void OnEnterClosest(InteractionController controller)
        {
            base.OnEnterClosest(controller);
        }

        public override void OnLeaveClosest(InteractionController controller)
        {
            base.OnLeaveClosest(controller);
        }

        public override void OnGrab(InteractionController controller)
        {
            base.OnGrab(controller);

            var handRenderer = controller.HandRenderer;

            //make the hand mesh invisible
            isGrabbed = true;
            handRenderer.enabled = false;
        }

        public override void OnDrop(InteractionController controller)
        {
            base.OnDrop(controller);

            var handRenderer = controller.HandRenderer;

            //remove item from hand
            isGrabbed = false;
            handRenderer.enabled = true;
        }
    }

}
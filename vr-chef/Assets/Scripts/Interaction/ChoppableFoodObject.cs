using UnityEngine;
using System.Collections;
using Assets.Scripts.Interaction.Generic;

namespace Assets.Scripts.Interaction
{
    public class ChoppableFoodObject : GrabbableObject
    {
        protected override float SelectedOutlineMultiplier => 2.0f;

        public GameObject SliceReferenceObject;

        public override void Start()
        {
            base.Start();
        }

        public void OnStartChop()
        {
            //make item directly kinematic to prevent movement
            objectRigidbody.isKinematic = true;
        }

        public void OnEndChop()
        {
            objectRigidbody.isKinematic = false;
        }

        public void OnChopped()
        {
            //TODO increase chop count, update renderer
            print("CHOPPED!");


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
    }

}
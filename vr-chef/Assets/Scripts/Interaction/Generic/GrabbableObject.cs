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
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class GrabbableObject : InteractableObject
    {
        protected Rigidbody objectRigidbody;
        protected bool isGrabbed = false;
        protected Vector3 originalRotationOnGrab;

        static float GRAB_COLLIDER_DISABLE_TIMEOUT = 0.2f; //seconds
        Dictionary<InteractionController, float> timesDropped = new Dictionary<InteractionController, float>();

        //static Vector3 RotationBias = new Vector3(90f, 0, 0);

        public override void Start()
        {
            base.Start();

            //get rigidbody
            objectRigidbody = gameObject.GetComponent<Rigidbody>();
        }

        public override void OnFrame(InteractionController controller)
        {
            base.OnFrame(controller);

            if (isGrabbed)
            {
                var hand = controller.GetHand();
                OnFrameWhenGrabbed(controller, hand);
            }

            // re-enable colliders if time arrived
            if ((timesDropped.ContainsKey(controller)) && (Time.time - timesDropped[controller] >= GRAB_COLLIDER_DISABLE_TIMEOUT))
            {
                timesDropped.Remove(controller);
                ToggleHandCollisions(controller, true);
            }
        }

        public virtual void OnFrameWhenGrabbed(InteractionController controller, GameObject hand)
        {
            //bind the object to exactly the hand's position
            objectRigidbody.MovePosition(controller.Target.transform.position);
            Vector3 euler = controller.Target.transform.rotation.eulerAngles;
            objectRigidbody.MoveRotation(Quaternion.Euler(euler));

            /*//attach item position to hand mesh's position and rotation every frame if grabbed
            gameObject.transform.position = controller.Target.transform.position;

            Vector3 newAngles = originalRotationOnGrab - controller.Target.transform.rotation.eulerAngles;// + controller.RotationBias;
            gameObject.transform.rotation = Quaternion.Euler(newAngles);*/
        }

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

            //get original hand rotation
            originalRotationOnGrab = controller.Target.transform.rotation.eulerAngles; //+ gameObject.transform.rotation.eulerAngles;

            //freeze rotation
            objectRigidbody.freezeRotation = true;
            gameObject.transform.position = controller.Target.transform.position;

            //create joint
            FixedJoint joint = gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = controller.HandRigidbody;

            timesDropped.Remove(controller);
        }

        private void ToggleHandCollisions(InteractionController controller, bool enable)
        {
            foreach (var collider in controller.HandColliders)
            {
                collider.enabled = enable;
            }
        }

        public override void OnDrop(InteractionController controller)
        {
            base.OnDrop(controller);

            var handRenderer = controller.HandRenderer;

            //disable hand collisions for a bit
            ToggleHandCollisions(controller, false);

            //remove item from hand
            isGrabbed = false;
            handRenderer.enabled = true;

            //disable joint
            Destroy(gameObject.GetComponent<FixedJoint>());

            //apply force equal to controller velocity
            objectRigidbody.AddForceAtPosition(controller.Velocity * 50f, controller.Position, ForceMode.Force);

            //unfreeze rotation 
            objectRigidbody.freezeRotation = false;

            //prapare to reinsert hand collisions
            timesDropped[controller] = Time.time;
        }
    }

}
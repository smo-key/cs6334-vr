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
        static Vector3 RotationBias = new Vector3(90f, -90f, 0f);

        protected Rigidbody objectRigidbody;
        protected Collider objectCollider;
        protected bool isGrabbed = false;
        protected Quaternion originalRotationOnGrab;

        static float GRAB_COLLIDER_DISABLE_TIMEOUT = 0.2f; //seconds
        Dictionary<InteractionController, float> timesDropped = new Dictionary<InteractionController, float>();

        //static Vector3 RotationBias = new Vector3(90f, 0, 0);

        public override void Start()
        {
            base.Start();

            //get rigidbody
            objectRigidbody = gameObject.GetComponent<Rigidbody>();

            //get collider
            objectCollider = gameObject.GetComponent<Collider>();
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

            Quaternion rot = controller.Target.transform.rotation;
            Quaternion around = Quaternion.Euler(RotationBias);
            rot *= around * originalRotationOnGrab;
            objectRigidbody.MoveRotation(rot);
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
            originalRotationOnGrab = Quaternion.Inverse(controller.Target.transform.rotation * gameObject.transform.rotation);

            //freeze rotation
            objectRigidbody.freezeRotation = true;
            gameObject.transform.position = controller.Target.transform.position;

            //disable collider
            if (objectCollider) objectCollider.enabled = false;

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

            //enable collider
            if (objectCollider) objectCollider.enabled = true;

            //apply force equal to controller velocity
            objectRigidbody.AddForceAtPosition(controller.Velocity * 50f, controller.Position, ForceMode.Force);

            //unfreeze rotation 
            objectRigidbody.freezeRotation = false;

            //prapare to reinsert hand collisions
            timesDropped[controller] = Time.time;
        }
    }

}
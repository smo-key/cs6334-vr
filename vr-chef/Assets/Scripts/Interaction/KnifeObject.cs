using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Interaction.Generic;
using UnityEngine.Experimental.PlayerLoop;
using System;
using Assets.Scripts.Collisions;

namespace Assets.Scripts.Interaction
{
    public class KnifeObject : GrabbableObject
    {
        protected override float SelectedOutlineMultiplier => 3.0f;
        
        static Vector3 RotationBias = new Vector3(90f, -90f, 0f);
        static float MIN_ENTRANCE_VELOCITY = 0.4f;
        static float MIN_EXIT_VELOCITY = 0.4f;

        public GameObject DefaultColliders;
        public GameObject HeldBoundaryColliders;
        public Collider ChopCollider;
        public MeshRenderer MeshRenderer;

        AudioSource audioData;

        HashSet<GameObject> objectsChopping = new HashSet<GameObject>();

        public override void Start()
        {
            base.Start();

            audioData = gameObject.GetComponent<AudioSource>();
        }

        void UpdateColliders()
        {
            DefaultColliders.SetActive(!isGrabbed);
            HeldBoundaryColliders.SetActive(isGrabbed && objectsChopping.Count == 0);
            ChopCollider.enabled = isGrabbed;
        }

        public override void OnFrameWhenGrabbed(InteractionController controller, GameObject hand)
        {
            //bind the knife to exactly the hand's position, plus a slight offset
            objectRigidbody.MovePosition(controller.Target.transform.position);

            Quaternion rot = controller.Target.transform.rotation;
            Quaternion around = Quaternion.Euler(RotationBias);
            rot *= around;
            objectRigidbody.MoveRotation(rot);
            
            //Vector3 euler = controller.Target.transform.rotation.eulerAngles + RotationBias;
            //objectRigidbody.MoveRotation(Quaternion.Euler(euler));
        }

        public void OnEdgeCollisionEnter(Collider collider, ColliderListenerAction listener)
        {
            if (!isGrabbed) return;

            //print("Collided with " + collider.gameObject.name + " at " + listener.CurrentVelocity.magnitude);
            if (listener.CurrentVelocity.y >= 0) return;    // ensure that we are cutting down
            if (listener.CurrentVelocity.magnitude < MIN_ENTRANCE_VELOCITY) return;

            //ensure item is choppable
            var foodListener = collider.gameObject.GetComponent<ChoppableFoodObject>();
            if (!foodListener) return;

            //update collision and state
            objectsChopping.Add(collider.gameObject);
            UpdateColliders();

            //inform food item that chopping has begun
            foodListener.OnStartChop();
        }

        public void OnEdgeCollisionFrame(Collider collider, ColliderListenerAction listener)
        {

        }

        public void OnEdgeCollisionExit(Collider collider, ColliderListenerAction listener)
        {
            if (!isGrabbed) return;
            if (!objectsChopping.Contains(collider.gameObject)) return;
            objectsChopping.Remove(collider.gameObject);
            UpdateColliders();

            //inform food item that chopping has ended
            var foodListener = collider.gameObject.GetComponent<ChoppableFoodObject>();
            foodListener.OnEndChop();

            //check chop velocity
            if (listener.CurrentVelocity.magnitude < MIN_EXIT_VELOCITY) return;

            //send chopped event
            foodListener.OnChopped();

            //play chop audio
            audioData.Play(0);
        }

        public override void OnFrame(InteractionController controller)
        {
            base.OnFrame(controller);
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

            UpdateColliders();
        }

        public override void OnDrop(InteractionController controller)
        {
            base.OnDrop(controller);

            UpdateColliders();
        }
    }

}
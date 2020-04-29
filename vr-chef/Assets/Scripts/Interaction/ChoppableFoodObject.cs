using UnityEngine;
using System.Collections;
using Assets.Scripts.Interaction.Generic;

namespace Assets.Scripts.Interaction
{
    public class ChoppableFoodObject : GrabbableObject
    {
        protected override float SelectedOutlineMultiplier => 2.0f;

        const int NUM_SLICES = 3;
        const float SLICE_SPATIAL_DELTA = 0.05f;

        public GameObject SliceReferenceObject;

        GameObject environmentRoot;
        Vector3 chopStartPosition;

        public override void Start()
        {
            base.Start();

            environmentRoot = GameObject.Find("Environment");
        }

        public void OnStartChop(KnifeObject knife)
        {
            //make item directly kinematic to prevent movement
            objectRigidbody.isKinematic = true;

            //TODO we also need the knife normal to form a chop plane
            chopStartPosition = knife.gameObject.transform.position;
        }

        public void OnEndChop(KnifeObject knife)
        {
            objectRigidbody.isKinematic = false;
        }

        public void OnChopped(KnifeObject knife)
        {
            print("CHOPPED!");

            //split object into slices
            GameObject[] slices = new GameObject[NUM_SLICES];

            for (int i = 0; i < NUM_SLICES; i++)
            {
                slices[i] = GameObject.Instantiate(SliceReferenceObject);

                //get recipe item
                var item = slices[i].GetComponent<RecipeItemObject>();

                //locate the chop plane
                Vector3 newPos = gameObject.transform.position;
                newPos.x = newPos.x + SLICE_SPATIAL_DELTA * Mathf.Cos(i / (float)NUM_SLICES * 2.0f * Mathf.PI);
                newPos.z = newPos.z + SLICE_SPATIAL_DELTA * Mathf.Sin(i / (float)NUM_SLICES * 2.0f * Mathf.PI);
                newPos.y = newPos.y + SLICE_SPATIAL_DELTA;

                item.gameObject.transform.position = newPos;
                item.gameObject.transform.parent = environmentRoot.transform;
                item.gameObject.SetActive(true);
                Debug.Log(newPos);
            }

            //destroy this object
            Object.Destroy(this.gameObject);
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
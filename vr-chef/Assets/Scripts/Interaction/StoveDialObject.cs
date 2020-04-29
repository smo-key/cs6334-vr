using UnityEngine;
using System.Collections;
using Assets.Scripts.Interaction.Generic;

namespace Assets.Scripts.Interaction
{
    public class StoveDialObject : FixedObject
    {
        protected static new float SelectedOutlineMultiplier = 2.0f;

        private float ANGLE_ZERO_DEADBAND = -35f;
        private float ANGLE_MAX = 145f;
        private float MAX_INTERACTION_DISTANCE = 0.25f;

        float startEulerRotation = 0;

        public GameObject FireObject;

        public override void Start()
        {
            base.Start();

            FireObject.SetActive(false);
        }

        public override void OnFrameWhenGrabbed(InteractionController controller, GameObject hand)
        {
            //disable interaction once the hand leaves the region of interest
            float distance = MathUtil.Distance(controller.Target.transform.position, gameObject.transform.position);
            if (distance > MAX_INTERACTION_DISTANCE)
            {
                controller.Drop();
                return;
            }

            //hide hand
            var handRenderer = controller.HandRenderer;
            handRenderer.enabled = false;

            //rotate the dial as the hand rotates
            float targetAngle = controller.Target.transform.rotation.eulerAngles.z - startEulerRotation;
            targetAngle *= -1;
            targetAngle = Mathf.DeltaAngle(0f, targetAngle); // converts to -180..180

            if (targetAngle > ANGLE_ZERO_DEADBAND && targetAngle < 0)
            {
                //round to zero
                targetAngle = 0;
            }
            if (targetAngle > 0 && targetAngle < ANGLE_MAX)
            {
                //round to nearest
                if (ANGLE_MAX - targetAngle > targetAngle) targetAngle = 0;
                else targetAngle = ANGLE_MAX;
            }

            FireObject.SetActive(targetAngle != 0);
            Vector3 rot = new Vector3(gameObject.transform.localRotation.x, gameObject.transform.localRotation.y, gameObject.transform.localRotation.z);
            rot.z = targetAngle;
            gameObject.transform.localRotation = Quaternion.Euler(rot);
        }

        public override void OnGrab(InteractionController controller)
        {
            base.OnGrab(controller);

            //get initial rotation of the hand
            startEulerRotation = controller.Target.transform.rotation.eulerAngles.z +
                gameObject.transform.localRotation.eulerAngles.z;
        }

        public override void OnDrop(InteractionController controller)
        {
            base.OnDrop(controller);
        }
    }

}
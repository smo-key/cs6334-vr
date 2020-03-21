using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(MeshRenderer))]
public class DialInteractionListener : InteractionListener
{
    public Material SelectedMaterial;
    private Material DefaultMaterial;

    bool IsGrabbed = false;
    MeshRenderer ObjectRenderer;
    private Vector3 StartEulerRotation = Vector3.zero;

    private float ANGLE_ZERO_DEADBAND = -35f;
    private float ANGLE_MAX = 145f;
    private float MAX_INTERACTION_DISTANCE = 0.25f;

    public void Start()
    {
        ObjectRenderer = gameObject.GetComponentInChildren<MeshRenderer>();
        DefaultMaterial = ObjectRenderer.material;
    }

    public override void OnFrame(InteractionController controller)
    {
        if (IsGrabbed)
        {
            //disable interaction once the hand leaves the region of interest
            var hand = controller.GetHand();
            float distance = MathUtil.Distance(controller.Target.transform.position, gameObject.transform.position);
            if (distance > MAX_INTERACTION_DISTANCE)
            {
                controller.Drop();
                return;
            }

            //hide hand
            var handRenderer = hand.GetComponent<MeshRenderer>();
            handRenderer.enabled = false;

            //rotate the dial as the hand rotates
            float targetAngle = controller.Target.transform.rotation.eulerAngles.z - StartEulerRotation.z;
            targetAngle *= -1;
            targetAngle = Mathf.DeltaAngle(0f, targetAngle);                            // converts to -180..180
            float originalAngle = Mathf.DeltaAngle(0f, StartEulerRotation.z);           // converts to -180..180
            float diffAngle = Mathf.DeltaAngle(originalAngle, targetAngle);             // total diff

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

            gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, targetAngle));
        }
    }

    public override void OnEnterClosest(InteractionController controller)
    {
        //don't do anything if there's an object in the hand
        if (controller.ControlledObject) return;

        //highlight it
        ObjectRenderer.material = SelectedMaterial;
    }

    public override void OnLeaveClosest(InteractionController controller)
    {
        //unhighlight it
        ObjectRenderer.material = DefaultMaterial;
    }

    public override void OnGrab(InteractionController controller)
    {
        var hand = controller.GetHand();
        var handRenderer = hand.GetComponent<MeshRenderer>();

        //make the hand mesh invisible
        IsGrabbed = true;
        handRenderer.enabled = false;

        //get initial rotation of the hand
        StartEulerRotation = controller.Target.transform.rotation.eulerAngles + 
            gameObject.transform.rotation.eulerAngles;
    }

    public override void OnDrop(InteractionController controller)
    {
        var hand = controller.GetHand();
        var handRenderer = hand.GetComponent<MeshRenderer>();

        //remove food item
        IsGrabbed = false;
        handRenderer.enabled = true;
    }
}

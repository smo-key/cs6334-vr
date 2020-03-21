using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(MeshRenderer))]
public class OvenDoorInteractionListener : InteractionListener
{
    public Material SelectedMaterial;
    public GameObject Door;

    private GameObject DoorHandle;
    private Material DefaultMaterial;

    private bool IsGrabbed = false;
    private MeshRenderer ObjectRenderer;
    private Vector3 StartEulerRotation = Vector3.zero;

    private float MAX_INTERACTION_DISTANCE = 0.25f;

    private Vector3 DoorHingeLocation;
    private float DoorHingeToHandleRadius;

    public void Start()
    {
        DoorHandle = gameObject;
        ObjectRenderer = DoorHandle.GetComponentInChildren<MeshRenderer>();
        DefaultMaterial = ObjectRenderer.material;

        //calculate door size
        var doorBounds = Door.GetComponent<Collider>().bounds;
        var doorHandlePosition = DoorHandle.transform.position;
        DoorHingeLocation = new Vector3(doorBounds.center.x, doorBounds.min.y, doorBounds.center.z);
        DoorHingeToHandleRadius = MathUtil.Distance(DoorHingeLocation, doorHandlePosition);
    }

    public override void OnFrame(InteractionController controller)
    {
        if (IsGrabbed)
        {
            //disable interaction once the hand leaves the region of interest
            //var hand = controller.GetHand();
            //float distance = MathUtil.Distance(controller.Target.transform.position, gameObject.transform.position);
            //if (distance > MAX_INTERACTION_DISTANCE)
            //{
            //    controller.Drop();
            //    return;
            //}

            //rotate the oven door such that it is as close to the hand as possible
            //this motion is described by finding the point closest to another (on a circle)
            var hand = controller.GetHand();
            var closestPoint = MathUtil.ClosestPointOnCircle(hand.transform.position, DoorHingeLocation, DoorHingeToHandleRadius);

            //if the hand is too far from that point, drop the door
            float distanceFromHandToHandle = MathUtil.Distance(hand.transform.position, closestPoint);
            if (distanceFromHandToHandle > MAX_INTERACTION_DISTANCE)
            {
                controller.Drop();
                return;
            }

            //calculate door rotation angle
            float diffZ = (closestPoint.z - DoorHingeLocation.z) / DoorHingeToHandleRadius;
            float angle = Mathf.Asin(diffZ);

            //perform rotation
            Door.transform.rotation = Quaternion.Euler(angle, 0f, 0f);

            //hide hand
            var handRenderer = hand.GetComponent<MeshRenderer>();
            handRenderer.enabled = false;
            

            ////rotate the dial as the hand rotates
            //float targetAngle = controller.Target.transform.rotation.eulerAngles.z - StartEulerRotation.z;
            //targetAngle *= -1;
            //targetAngle = Mathf.DeltaAngle(0f, targetAngle);                            // converts to -180..180
            //float originalAngle = Mathf.DeltaAngle(0f, StartEulerRotation.z);           // converts to -180..180
            //float diffAngle = Mathf.DeltaAngle(originalAngle, targetAngle);             // total diff

            //if (targetAngle > ANGLE_ZERO_DEADBAND && targetAngle < 0)
            //{
            //    //round to zero
            //    targetAngle = 0;
            //}
            //if (targetAngle > 0 && targetAngle < ANGLE_MAX)
            //{
            //    //round to nearest
            //    if (ANGLE_MAX - targetAngle > targetAngle) targetAngle = 0;
            //    else targetAngle = ANGLE_MAX;
            //}

            //gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, targetAngle));
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

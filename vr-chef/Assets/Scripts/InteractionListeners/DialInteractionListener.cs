using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(MeshRenderer))]
public class DialInteractionListener : InteractionListener
{
    public static Color DefaultOutlineColor = Color.white;
    public static Color SelectedOutlineColor = new Color(255.0f / 255.0f, 228.0f / 255.0f, 0.0f / 255.0f);
    public static float SelectedOutlineMultiplier = 2.0f;

    bool isGrabbed = false;
    MeshRenderer renderer;
    Vector3 startEulerRotation = Vector3.zero;
    float defaultOutlineWidth;

    private float ANGLE_ZERO_DEADBAND = -35f;
    private float ANGLE_MAX = 145f;
    private float MAX_INTERACTION_DISTANCE = 0.25f;

    public GameObject FireObject;

    public void Start()
    {
        renderer = gameObject.GetComponentInChildren<MeshRenderer>();
        defaultOutlineWidth = renderer.material.GetFloat("_OutlineWidth");
        FireObject.SetActive(false);
        UpdateMaterial(false);
    }

    void UpdateMaterial(bool isNearHand)
    {
        foreach (var material in renderer.materials)
        {
            material.SetColor("_Tint", isNearHand ? SelectedOutlineColor : DefaultOutlineColor);
            material.SetColor("_OutlineColor", isNearHand ? SelectedOutlineColor : DefaultOutlineColor);
            material.SetFloat("_OutlineWidth", isNearHand ? defaultOutlineWidth * SelectedOutlineMultiplier : defaultOutlineWidth);
        }

        renderer.UpdateGIMaterials();
    }

    public override void OnFrame(InteractionController controller)
    {
        if (isGrabbed)
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
            var handRenderer = hand.GetComponent<SkinnedMeshRenderer>();
            handRenderer.enabled = false;

            //rotate the dial as the hand rotates
            float targetAngle = controller.Target.transform.rotation.eulerAngles.z - startEulerRotation.z;
            targetAngle *= -1;
            targetAngle = Mathf.DeltaAngle(0f, targetAngle);                            // converts to -180..180
            float originalAngle = Mathf.DeltaAngle(0f, startEulerRotation.z);           // converts to -180..180
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

            FireObject.SetActive(targetAngle != 0);

            gameObject.transform.rotation = Quaternion.Euler(new Vector3(0, 0, targetAngle));
        }
    }

    public override void OnEnterClosest(InteractionController controller)
    {
        //don't do anything if there's an object in the hand
        if (controller.ControlledObject) return;

        //highlight it
        UpdateMaterial(true);
    }

    public override void OnLeaveClosest(InteractionController controller)
    {
        //unhighlight it
        UpdateMaterial(false);
    }

    public override void OnGrab(InteractionController controller)
    {
        var hand = controller.GetHand();
        var handRenderer = hand.GetComponent<SkinnedMeshRenderer>();

        //make the hand mesh invisible
        isGrabbed = true;
        handRenderer.enabled = false;

        //get initial rotation of the hand
        startEulerRotation = controller.Target.transform.rotation.eulerAngles + 
            gameObject.transform.rotation.eulerAngles;

        UpdateMaterial(false);
    }

    public override void OnDrop(InteractionController controller)
    {
        var hand = controller.GetHand();
        var handRenderer = hand.GetComponent<SkinnedMeshRenderer>();

        //remove food item
        isGrabbed = false;
        handRenderer.enabled = true;
    }
}

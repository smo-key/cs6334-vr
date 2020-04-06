using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Rigidbody))]
public class FoodInteractionListener : InteractionListener
{
    public static Color DefaultOutlineColor = Color.white;
    public static Color SelectedOutlineColor = new Color(255.0f/255.0f, 209.0f/255.0f, 43.0f/255.0f);
    public static float SelectedOutlineMultiplier = 3.0f;

    MeshRenderer renderer;
    Rigidbody rb;
    bool isGrabbed = false;
    bool isChopped = false;
    float defaultOutlineWidth;

    void UpdateMaterial(bool isNearHand)
    {
        print(isNearHand);

        foreach (var material in renderer.materials)
        {
            material.SetColor("_OutlineColor", isNearHand ? SelectedOutlineColor : DefaultOutlineColor);
            material.SetFloat("_OutlineWidth", isNearHand ? defaultOutlineWidth * SelectedOutlineMultiplier : defaultOutlineWidth);
        }

        renderer.UpdateGIMaterials();
    }

    public void Start()
    {
        renderer = gameObject.GetComponentInChildren<MeshRenderer>();
        rb = gameObject.GetComponent<Rigidbody>();
        defaultOutlineWidth = renderer.material.GetFloat("_OutlineWidth");

        //update material
        UpdateMaterial(false);
    }

    public void onChopped()
    {
        isChopped = true;
    }

    public override void OnFrame(InteractionController controller)
    {
        //attach food item position to hand mesh's position and rotation every frame if grabbed
        if (isGrabbed)
        {
            var hand = controller.GetHand();
            var handRenderer = hand.GetComponent<MeshRenderer>();
            gameObject.transform.position = controller.Target.transform.position;
            handRenderer.enabled = false;
        }
    }

    public override void OnEnterClosest(InteractionController controller)
    {
        //don't do anything if there's an object in the hand
        if (controller.ControlledObject) return;

        UpdateMaterial(true);
    }

    public override void OnLeaveClosest(InteractionController controller)
    {
        UpdateMaterial(false);
    }

    public override void OnGrab(InteractionController controller)
    {
        var hand = controller.GetHand();
        var handRenderer = hand.GetComponent<MeshRenderer>();

        //make the hand mesh invisible
        isGrabbed = true;
        handRenderer.enabled = false;
        rb.isKinematic = true;

        UpdateMaterial(false);
    }

    public override void OnDrop(InteractionController controller)
    {
        var hand = controller.GetHand();
        var handRenderer = hand.GetComponent<MeshRenderer>();

        //remove food item
        isGrabbed = false;
        handRenderer.enabled = true;
        rb.isKinematic = false;
    }
}

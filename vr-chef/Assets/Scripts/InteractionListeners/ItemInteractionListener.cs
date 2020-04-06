using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Rigidbody))]
public class ItemInteractionListener : InteractionListener
{
    public static Color DefaultOutlineColor = Color.white;
    public static Color SelectedOutlineColor = new Color(255.0f / 255.0f, 228.0f / 255.0f, 0.0f / 255.0f);
    public static float SelectedOutlineMultiplier = 2.0f;

    MeshRenderer renderer;
    Rigidbody rb;
    bool isGrabbed = false;
    float defaultOutlineWidth;
    Vector3 originalRotationOnGrab;
    int chopCount = 0;

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

    public void Start()
    {
        renderer = gameObject.GetComponentInChildren<MeshRenderer>();
        rb = gameObject.GetComponent<Rigidbody>();
        defaultOutlineWidth = renderer.material.GetFloat("_OutlineWidth");

        //update material
        UpdateMaterial(false);
    }

    public void OnChopped()
    {
        //TODO increase chop count, update renderer
        chopCount++;
        print("CHOPPED!");
    }

    public override void OnFrame(InteractionController controller)
    {
        //attach food item position to hand mesh's position and rotation every frame if grabbed
        if (isGrabbed)
        {
            var hand = controller.GetHand();
            var handRenderer = hand.GetComponent<SkinnedMeshRenderer>();
            gameObject.transform.position = controller.Target.transform.position;

            Vector3 newAngles = controller.Target.transform.rotation.eulerAngles - originalRotationOnGrab;
            gameObject.transform.rotation = Quaternion.Euler(newAngles);
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
        var handRenderer = hand.GetComponent<SkinnedMeshRenderer>();

        originalRotationOnGrab = controller.Target.transform.rotation.eulerAngles;

        //make the hand mesh invisible
        isGrabbed = true;
        handRenderer.enabled = false;
        rb.isKinematic = true;

        UpdateMaterial(false);
    }

    public override void OnDrop(InteractionController controller)
    {
        var hand = controller.GetHand();
        var handRenderer = hand.GetComponent<SkinnedMeshRenderer>();

        //remove food item
        isGrabbed = false;
        handRenderer.enabled = true;
        rb.isKinematic = false;
    }
}

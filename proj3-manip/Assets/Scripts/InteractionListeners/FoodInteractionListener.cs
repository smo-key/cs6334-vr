using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class FoodInteractionListener : InteractionListener
{
    public Material DefaultMaterial;
    public Material SelectedMaterial;
    
    bool IsGrabbed = false;
    MeshRenderer ObjectRenderer;

    public void Start()
    {
        ObjectRenderer = gameObject.GetComponentInChildren<MeshRenderer>();
    }

    public override void OnFrame(InteractionController controller)
    {
        //attach food item position to hand mesh's position and rotation every frame if grabbed
        if (IsGrabbed)
        {
            var hand = controller.GetHand();
            var handRenderer = hand.GetComponent<MeshRenderer>();

            gameObject.transform.position = controller.Target.transform.position;
            handRenderer.enabled = false;
        }
    }

    public override void OnEnterClosest(InteractionController controller)
    {
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

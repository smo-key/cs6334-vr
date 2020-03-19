using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class KnifeInteractionListener : InteractionListener
{
    public Material DefaultMaterial;
    public Material SelectedMaterial;
    
    bool IsGrabbed = false;
    //MeshRenderer ObjectRenderer;
    List<MeshRenderer> objectRenderers;
    Rigidbody rb;

    public void Start()
    {
        //ObjectRenderer = gameObject.GetComponentInChildren<MeshRenderer>();
        rb = gameObject.GetComponent<Rigidbody>();
        objectRenderers = new List<MeshRenderer>();
        foreach (Transform child in transform)
        {
            objectRenderers.Add(child.GetComponent<MeshRenderer>());
        }
    }

    public override void OnFrame(InteractionController controller)
    {
        //attach food item position to hand mesh's position and rotation every frame if grabbed
        if (IsGrabbed)
        {
            var hand = controller.GetHand();
            var handRenderer = hand.GetComponent<MeshRenderer>();
            gameObject.transform.position = controller.Target.transform.position;
            gameObject.transform.rotation = controller.Target.transform.rotation;
            handRenderer.enabled = false;
            print("Knife: " + gameObject.transform.rotation);
            print("Hand: " + controller.Target.transform.rotation);
            gameObject.transform.Rotate(controller.transform.rotation.x + 45, controller.transform.rotation.y + 45, controller.transform.rotation.z + 45);
        }
    }

    public override void OnEnterClosest(InteractionController controller)
    {
        //highlight it
        //ObjectRenderer.material = SelectedMaterial;
        foreach(MeshRenderer renderer in objectRenderers)
        {
            renderer.material = SelectedMaterial;
        }
    }

    public override void OnLeaveClosest(InteractionController controller)
    {
        //unhighlight it
        //ObjectRenderer.material = DefaultMaterial;
        foreach (MeshRenderer renderer in objectRenderers)
        {
            renderer.material = DefaultMaterial;
        }
    }

    public override void OnGrab(InteractionController controller)
    {
        var hand = controller.GetHand();
        var handRenderer = hand.GetComponent<MeshRenderer>();

        //make the hand mesh invisible
        IsGrabbed = true;
        handRenderer.enabled = false;
        rb.isKinematic = true;
    }

    public override void OnDrop(InteractionController controller)
    {
        var hand = controller.GetHand();
        var handRenderer = hand.GetComponent<MeshRenderer>();

        //remove food item
        IsGrabbed = false;
        handRenderer.enabled = true;
        rb.isKinematic = false;
    }
}

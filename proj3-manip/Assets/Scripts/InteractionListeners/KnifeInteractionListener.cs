using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Rigidbody))]
public class KnifeInteractionListener : InteractionListener
{
    public Material SelectedMaterial;

    private Material[] OriginalMaterials;

    MeshRenderer ObjectRenderer;
    bool IsGrabbed = false;
    Rigidbody RigidBody;

    public void Start()
    {
        ObjectRenderer = gameObject.GetComponentInChildren<MeshRenderer>();
        RigidBody = gameObject.GetComponent<Rigidbody>();

        // load materials
        OriginalMaterials = gameObject.GetComponent<MeshRenderer>().materials;
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
        //highlight the object
        Material[] materials = new Material[ObjectRenderer.materials.Length];
        for (int i=0; i<materials.Length; i++)
        {
            materials[i] = SelectedMaterial;
        }
        ObjectRenderer.materials = materials;
    }

    public override void OnLeaveClosest(InteractionController controller)
    {
        //unhighlight the object
        ObjectRenderer.materials = OriginalMaterials;
    }

    public override void OnGrab(InteractionController controller)
    {
        var hand = controller.GetHand();
        var handRenderer = hand.GetComponent<MeshRenderer>();

        //make the hand mesh invisible
        IsGrabbed = true;
        handRenderer.enabled = false;
        RigidBody.isKinematic = true;
    }

    public override void OnDrop(InteractionController controller)
    {
        var hand = controller.GetHand();
        var handRenderer = hand.GetComponent<MeshRenderer>();

        //remove food item
        IsGrabbed = false;
        handRenderer.enabled = true;
        RigidBody.isKinematic = false;
    }
}

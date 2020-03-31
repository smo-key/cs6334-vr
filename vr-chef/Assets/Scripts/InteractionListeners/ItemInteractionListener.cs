﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Rigidbody))]
public class ItemInteractionListener : InteractionListener
{
    public Material SelectedMaterial;

    private Material DefaultMaterial;

    bool IsGrabbed = false;
    MeshRenderer ObjectRenderer;
    Rigidbody rb;

    public void Start()
    {
        ObjectRenderer = gameObject.GetComponent<MeshRenderer>();
        rb = gameObject.GetComponent<Rigidbody>();
        DefaultMaterial = ObjectRenderer.material;
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
        if (rb) rb.isKinematic = true;
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

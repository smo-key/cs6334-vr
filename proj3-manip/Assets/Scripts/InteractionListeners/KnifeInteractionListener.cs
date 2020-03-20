using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Rigidbody))]
public class KnifeInteractionListener : InteractionListener
{
    public Material SelectedMaterial;

    public Material ChoppedMaterial;

    private Material[] OriginalMaterials;

    MeshRenderer ObjectRenderer;
    bool IsGrabbed = false;
    Rigidbody RigidBody;

    System.DateTime startTime;
    System.TimeSpan timeSpan;
    double prevY = 0;
    double currentY = 0;
    bool isChopping = false;

    public void Start()
    {
        ObjectRenderer = gameObject.GetComponentInChildren<MeshRenderer>();
        RigidBody = gameObject.GetComponent<Rigidbody>();

        // load materials
        OriginalMaterials = gameObject.GetComponent<MeshRenderer>().materials;
    }

    private void OnCollisionEnter(Collision collision)
    {
        print(collision.gameObject.name);
        if (isChopping && collision.gameObject.name.Contains("Tomato"))
        {
            DialInteractionListener foodInteractionListener = collision.gameObject.GetComponent<DialInteractionListener>();
            foodInteractionListener.onChopped();
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
            //print("Knife: " + gameObject.transform.rotation);
            //print("Hand: " + controller.Target.transform.rotation);
            if (controller.Name.Equals("Left"))
            {
                gameObject.transform.Rotate(controller.transform.rotation.x + 45, controller.transform.rotation.y + 45, controller.transform.rotation.z + 45);
            }
            else if (controller.Name.Equals("Right"))
            {
                gameObject.transform.Rotate(controller.transform.rotation.x + 45, controller.transform.rotation.y - 45, controller.transform.rotation.z + 45);
            }
            //print("Knife: " + gameObject.transform.position);
            timeSpan = System.DateTime.UtcNow - startTime;
            //print("Elasped MS: " + timeSpan.TotalMilliseconds);
            double timeThreshold = 1000;
            if(timeSpan.TotalMilliseconds >= timeThreshold)
            {
                currentY = gameObject.transform.position.y;
                double diff = currentY - prevY;
                print("DIFF: " + diff.ToString());
                ObjectRenderer.materials = OriginalMaterials;
                isChopping = false;
                if (diff <= -0.15)
                {
                    print("Chopped Down");
                    isChopping = true;
                    Material[] materials2 = new Material[ObjectRenderer.materials.Length];
                    for (int i = 0; i < materials2.Length; i++)
                    {
                        materials2[i] = ChoppedMaterial;
                    }
                    ObjectRenderer.materials = materials2;
                }
                prevY = currentY;
                startTime = System.DateTime.UtcNow;
            }
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
        startTime = System.DateTime.UtcNow;
        currentY = gameObject.transform.position.y;
        prevY = currentY;
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

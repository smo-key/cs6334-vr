using UnityEngine;
using System.Collections;

public class FoodInteractionListener : MonoBehaviour, IInteractionListener
{

    public Material[] materials;
    bool isGrabbed = false;

    public void OnFrame(InteractionController controller)
    {
        //attach food item position to hand mesh's position and rotation every frame if grabbed
        if (!isGrabbed)
        {
            this.gameObject.transform.position = controller.Target.transform.position;
            var hand = controller.transform.GetChild(1).transform.GetChild(0);
            hand.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public void OnEnterClosest(InteractionController controller)
    {
        //highlight it
        //Debug.Log("Enter closest: " + controller.DistanceFromObject(this.gameObject));
        var child = this.gameObject.transform.GetChild(0);
        print("Closest object is " + child.name);
        var renderer = child.GetComponent<MeshRenderer>();
        renderer.material = materials[0];

    }

    public void OnLeaveClosest(InteractionController controller)
    {
        //unhighlight it
        var child = this.gameObject.transform.GetChild(0);
        print("Closest object is " + child.name);
        var renderer = child.GetComponent<MeshRenderer>();
        renderer.material = materials[1];
    }

    public void OnGrab(InteractionController controller)
    {
        //make the hand mesh invisible
        isGrabbed = true;
        controller.GetComponent<MeshRenderer>().enabled = false;
    }

    public void OnDrop(InteractionController controller)
    {
        //remove food item
        isGrabbed = false;
        controller.GetComponent<MeshRenderer>().enabled = true;
    }
}

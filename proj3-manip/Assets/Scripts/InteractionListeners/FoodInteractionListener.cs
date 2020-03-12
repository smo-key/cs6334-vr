using UnityEngine;
using System.Collections;

public class FoodInteractionListener : MonoBehaviour, IInteractionListener
{

    public Material[] materials;

    public void OnFrame(InteractionController controller)
    {
        //attach food item position to hand mesh's position and rotation every frame if grabbed
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
    }

    public void OnDrop(InteractionController controller)
    {
        //remove food item
    }
}

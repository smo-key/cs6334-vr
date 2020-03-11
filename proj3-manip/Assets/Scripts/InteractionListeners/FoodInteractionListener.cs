using UnityEngine;
using System.Collections;

public class FoodInteractionListener : MonoBehaviour, IInteractionListener
{
    public void OnFrame(InteractionController controller)
    {
        //attach food item position to hand mesh's position and rotation every frame if grabbed
    }

    public void OnEnterClosest(InteractionController controller)
    {
        //highlight it
        Debug.Log("Enter closest: " + controller.DistanceFromObject(this.gameObject));
    }

    public void OnLeaveClosest(InteractionController controller)
    {
        //unhighlight it
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

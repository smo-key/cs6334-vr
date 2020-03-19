using UnityEngine;
using UnityEditor;

public abstract class InteractionListener : MonoBehaviour
{
    public abstract void OnFrame(InteractionController controller);

    public abstract void OnEnterClosest(InteractionController controller);

    public abstract void OnLeaveClosest(InteractionController controller);

    public abstract void OnGrab(InteractionController controller);

    public abstract void OnDrop(InteractionController controller);
}
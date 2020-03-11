using UnityEngine;
using UnityEditor;

public interface IInteractionListener
{
    void OnFrame(InteractionController controller);

    void OnEnterClosest(InteractionController controller);

    void OnLeaveClosest(InteractionController controller);

    void OnGrab(InteractionController controller);

    void OnDrop(InteractionController controller);
}
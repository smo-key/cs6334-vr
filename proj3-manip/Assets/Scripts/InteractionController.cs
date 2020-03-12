using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteractionController : MonoBehaviour
{

    [SerializeField] public GameObject Target;
    [Range(0, 1f)] public float InteractionRange = 0.5f;

    public GameObject ControlledObject { get; private set; }
    public GameObject NearestObject { get; private set; }

    void Start()
    {
        
    }

    float distance3(Vector3 a, Vector3 b)
    {
        return Mathf.Sqrt((b.x - a.x) * (b.x - a.x) + (b.y - a.y) * (b.y - a.y) + (b.z - a.z) * (b.z - a.z));
    }

    public float DistanceFromObject(GameObject obj)
    {
        return distance3(obj.transform.position, Target.transform.position);
    }

    public void Drop()
    {
        if (!ControlledObject) return;
        ControlledObject.GetComponent<IInteractionListener>().OnDrop(this);
        ControlledObject = null;
    }

    void Update()
    {
        //locate nearest interactable, excluding the object we are currently controlling
        var objectsExcludingControlling = GameObject.FindGameObjectsWithTag("Interactable")
            .Where(o => !o.Equals(ControlledObject));

        //get nearest object
        NearestObject = objectsExcludingControlling.Aggregate((curMin, o) => (curMin == null || distance3(o.transform.position, Target.transform.position) <
            distance3(curMin.transform.position, Target.transform.position) ? o : curMin));

        //send on frame event to all interactables
        foreach (var interactable in objectsExcludingControlling)
        {
            interactable.GetComponent<IInteractionListener>().OnFrame(this);
        }

        //if (NearestObject != null) print(DistanceFromObject(NearestObject));

        if (NearestObject != null && DistanceFromObject(NearestObject) < InteractionRange)
        {
            //send event to interactable
            NearestObject.GetComponent<IInteractionListener>().OnEnterClosest(this);
        }
    }
}

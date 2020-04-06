using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRTK.Prefabs.CameraRig.UnityXRCameraRig.Input;
using Zinnia.Tracking.Collision;

public class InteractionController : MonoBehaviour
{
    [SerializeField] public string Name;
    [SerializeField] public GameObject Target;
    [SerializeField] public GameObject[] TriggerButtons;
    [SerializeField] public GameObject EnvironmentRoot;
    [SerializeField] public Vector3 RotationBias;
    [Range(0, 1f)] public float InteractionRange = 0.25f;

    public GameObject ControlledObject { get; private set; } = null;
    public GameObject NearestObject { get; private set; } = null;

    private static HashSet<GameObject> AllControlledObjects = new HashSet<GameObject>();

    void Start()
    {
        //listen for trigger event
        foreach (var button in TriggerButtons)
        {
            button.GetComponentInChildren<UnityButtonAction>().ValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>(OnTriggerChanged));
        }
    }

    void OnTriggerChanged(bool triggerPressed)
    {
        if (triggerPressed)
        {
            //try grab
            Grab();
        } else {
            //try drop
            Drop();
        }
    }

    public GameObject GetHand()
    {
        return GetComponentInChildren<SkinnedMeshRenderer>().gameObject;
    }

    public float DistanceFromObject(GameObject obj)
    {
        return MathUtil.Distance(obj.transform.position, Target.transform.position);
    }

    public void Grab()
    {
        if (ControlledObject) return;
        if (!NearestObject) return;

        //grab nearest object
        ControlledObject = NearestObject;
        AllControlledObjects.Add(ControlledObject);
        ControlledObject.GetComponent<InteractionListener>().OnGrab(this);
    }

    public void Drop()
    {
        if (!ControlledObject) return;

        //drop held item
        ControlledObject.GetComponent<InteractionListener>().OnDrop(this);
        AllControlledObjects.Remove(ControlledObject);
        ControlledObject = null;
    }

    void Update()
    {
        //locate nearest interactable, excluding any currently held objects
        var objectsExcludingControlling = EnvironmentRoot
            .GetComponentsInChildren<InteractionListener>()
            .Select(o => o.gameObject)
            .Where(o => !AllControlledObjects.Contains(o));

        //get nearest object
        var newNearestObject = objectsExcludingControlling.Count() > 0 ? objectsExcludingControlling.Aggregate((curMin, o) => (curMin == null || MathUtil.Distance(o.transform.position, Target.transform.position) <
            MathUtil.Distance(curMin.transform.position, Target.transform.position) ? o : curMin)) : null;

        //verify that nearest object is within our radius
        if (newNearestObject != null && DistanceFromObject(newNearestObject) >= InteractionRange)
        {
            newNearestObject = null;
        }

        //check if there the old object is no longer the nearest
        if (newNearestObject != NearestObject && NearestObject != null)
        {
            NearestObject.GetComponent<InteractionListener>().OnLeaveClosest(this);
        }
        
        //check if there is a new nearest object
        if (newNearestObject != NearestObject && newNearestObject != null)
        {
            newNearestObject.GetComponent<InteractionListener>().OnEnterClosest(this);
        }

        //update nearest object
        NearestObject = newNearestObject;

        //send on frame event to all interactables
        var objectsIncludingControlling = ControlledObject ? objectsExcludingControlling.Union(new List<GameObject>() { ControlledObject }) : objectsExcludingControlling;
        foreach (var interactable in objectsIncludingControlling)
        {
            interactable.GetComponent<InteractionListener>().OnFrame(this);
        }
    }
}

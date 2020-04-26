using Assets.Scripts.Interaction.Generic;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRTK.Prefabs.CameraRig.UnityXRCameraRig.Input;
using Zinnia.Tracking.Collision;
using Zinnia.Tracking.Velocity;

namespace Assets.Scripts
{
    [RequireComponent(typeof(AverageVelocityEstimator))]
    public class InteractionController : MonoBehaviour
    {
        #region Config

        [SerializeField] public string Name;
        [SerializeField] public GameObject Target;
        [SerializeField] public GameObject[] TriggerButtons;
        [SerializeField] public GameObject EnvironmentRoot;
        [SerializeField] public Vector3 RotationBias;

        const float INTERACTION_RANGE = 0.2f; //meters

        #endregion

        #region Public

        public GameObject ControlledObject { get; private set; } = null;
        public GameObject NearestObject { get; private set; } = null;
        public Rigidbody HandRigidbody => GetComponentInChildren<Rigidbody>();
        public SkinnedMeshRenderer HandRenderer => GetComponentInChildren<SkinnedMeshRenderer>();
        public GameObject HandObject => HandRenderer.gameObject;
        public Collider[] HandColliders => GetComponentsInChildren<Collider>();
        public Vector3 Velocity => GetComponent<AverageVelocityEstimator>().GetVelocity();
        public Vector3 AngularVelocity => GetComponent<AverageVelocityEstimator>().GetAngularVelocity();
        public Vector3 Position => gameObject.transform.position;

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
            ControlledObject.GetComponent<InteractableObject>().OnGrab(this);
        }

        public void Drop()
        {
            if (!ControlledObject) return;

            //drop held item
            ControlledObject.GetComponent<InteractableObject>().OnDrop(this);
            AllControlledObjects.Remove(ControlledObject);
            ControlledObject = null;
        }

        public GameObject GetHand()
        {
            return HandObject;
        }

        #endregion

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
            }
            else
            {
                //try drop
                Drop();
            }
        }

        void Update()
        {
            //locate nearest interactable, excluding any currently held objects
            var objectsExcludingControlling = EnvironmentRoot
                .GetComponentsInChildren<InteractableObject>()
                .Select(o => o.gameObject)
                .Where(o => !AllControlledObjects.Contains(o));

            //get nearest object
            var newNearestObject = objectsExcludingControlling.Count() > 0 ? objectsExcludingControlling.Aggregate((curMin, o) => (curMin == null || MathUtil.Distance(o.transform.position, Target.transform.position) <
                MathUtil.Distance(curMin.transform.position, Target.transform.position) ? o : curMin)) : null;

            //verify that nearest object is within our radius
            if (newNearestObject != null && DistanceFromObject(newNearestObject) >= INTERACTION_RANGE)
            {
                newNearestObject = null;
            }

            //check if there the old object is no longer the nearest
            if (newNearestObject != NearestObject && NearestObject != null)
            {
                NearestObject.GetComponent<InteractableObject>().OnLeaveClosest(this);
            }

            //check if there is a new nearest object
            if (newNearestObject != NearestObject && newNearestObject != null)
            {
                newNearestObject.GetComponent<InteractableObject>().OnEnterClosest(this);
            }

            //update nearest object
            NearestObject = newNearestObject;

            //send on frame event to all interactables
            var objectsIncludingControlling = ControlledObject ? objectsExcludingControlling.Union(new List<GameObject>() { ControlledObject }) : objectsExcludingControlling;
            foreach (var interactable in objectsIncludingControlling)
            {
                interactable.GetComponent<InteractableObject>().OnFrame(this);
            }
        }
    }
}

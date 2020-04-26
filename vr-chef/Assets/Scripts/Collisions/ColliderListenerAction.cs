using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using System;

namespace Assets.Scripts.Collisions
{
    [Serializable]
    public class ColliderListenerAction : MonoBehaviour
    {
        [SerializeField] public CustomCollisionEvent OnCollisionEnterEvent;
        [SerializeField] public CustomCollisionEvent OnCollisionStayEvent;
        [SerializeField] public CustomCollisionEvent OnCollisionExitEvent;

        public Vector3 PreviousPosition { get; private set; }
        public Vector3 CurrentVelocity { get; private set; }

        public void Start()
        {
            PreviousPosition = gameObject.transform.position;
        }

        public void FixedUpdate()
        {
            CurrentVelocity = (gameObject.transform.position - PreviousPosition) / Time.fixedDeltaTime;
            PreviousPosition = gameObject.transform.position;
        }

        public void OnTriggerEnter(Collider collider)
        {
            OnCollisionEnterEvent?.Invoke(collider, this);
        }

        public void OnTriggerStay(Collider collider)
        {
            OnCollisionStayEvent?.Invoke(collider, this);
        }

        public void OnTriggerExit(Collider collider)
        {
            OnCollisionExitEvent?.Invoke(collider, this);
        }
    }
}
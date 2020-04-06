using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Collision
{
    [Serializable]
    public class CustomCollisionEvent : UnityEvent<UnityEngine.Collider, ColliderListenerAction>
    {

    }
}

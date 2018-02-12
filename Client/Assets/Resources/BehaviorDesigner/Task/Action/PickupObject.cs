using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskCategory("My Action")]

    public class PickupObject : Action
    {
        public SharedGameObject TargetObjectToPickup;

        private PlayerBase _selfPlayer;

        public override void OnStart()
        {
            _selfPlayer = gameObject.GetComponent<PlayerBase>();
        }

        public override TaskStatus OnUpdate()
        {
            if(TargetObjectToPickup.Value != null && _selfPlayer != null)
            {
                _selfPlayer.PickupObject(TargetObjectToPickup.Value, TargetObjectToPickup.Value.transform.position);
                return TaskStatus.Success;
            }
            else
            {
                return TaskStatus.Failure;
            }
        }
    }

}
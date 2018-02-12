using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections;

namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskCategory("My Conditional")]

    public class IsPlayerMoving : Conditional
    {
        [Tooltip("The GameObject that the task operates on. If null the task GameObject is used.")]
        public SharedGameObject TargetObject;

        private PlayerBase _targetPlayer;

        public override void OnStart()
        {
            GameObject targetObject = GetDefaultGameObject(TargetObject.Value);
            _targetPlayer = targetObject.GetComponent<PlayerBase>();
        }

        public override TaskStatus OnUpdate()
        {
            if(_targetPlayer != null)
            {
                float curPlayerSpeed = _targetPlayer.GetCurSpeed();
                if (curPlayerSpeed > 0.01f)
                {
                    return TaskStatus.Success;
                }
            }

            return TaskStatus.Failure;
        }
    }

}
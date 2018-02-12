using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskCategory("My Action")]

    [TaskDescription("Gets the cur movement velocity when following a path. Returns Success.")]
    public class GetNavAgentCurVelocity : Action
    {
        [Tooltip("The GameObject that the task operates on. If null the task GameObject is used.")]
        public SharedGameObject targetGameObject;
        [SharedRequired]
        [Tooltip("The NavMeshAgent speed")]
        public  SharedVector3 storeValue;

        // cache the navmeshagent component
        private UnityEngine.AI.NavMeshAgent navMeshAgent;
        private GameObject prevGameObject;

        public override void OnStart()
        {
            var currentGameObject = GetDefaultGameObject(targetGameObject.Value);
            if (currentGameObject != prevGameObject)
            {
                navMeshAgent = currentGameObject.GetComponent<UnityEngine.AI.NavMeshAgent>();
                prevGameObject = currentGameObject;
            }
        }

        public override TaskStatus OnUpdate()
        {
            if (navMeshAgent == null)
            {
                Debug.LogWarning("NavMeshAgent is null");
                return TaskStatus.Failure;
            }

            storeValue.Value = navMeshAgent.velocity;

            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            targetGameObject = null;
            storeValue = new Vector3();
        }
    }
}
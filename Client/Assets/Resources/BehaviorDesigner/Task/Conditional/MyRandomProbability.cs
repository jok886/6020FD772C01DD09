using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskCategory("My Conditional")]
    public class MyRandomProbability : Conditional
    {
        [Tooltip("The chance that the task will return success")]
        public SharedFloat successProbability = 0.5f;

        public override void OnAwake()
        {
        }

        public override TaskStatus OnUpdate()
        {
            float rand01 = (float)MersenneTwister.MT19937.Real1();
            //Debug.Log("MyRandomProbability: "+ rand01);

            if (rand01 <= successProbability.Value)
            {
                return TaskStatus.Success;
            }
            else
            {
                return TaskStatus.Failure;
            }
        }
    }
}

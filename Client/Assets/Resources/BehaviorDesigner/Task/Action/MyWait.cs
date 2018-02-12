using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskDescription("Wait a specified amount of time. The task will return running until the task is done waiting. It will return success after the wait time has elapsed.")]
    [TaskCategory("My Action")]
    [HelpURL("http://www.opsive.com/assets/BehaviorDesigner/documentation.php?id=22")]
    [TaskIcon("{SkinColor}WaitIcon.png")]
    public class MyWait : Action
    {
        [Tooltip("The amount of time to wait")]
        public SharedFloat WaitTime = 1f;
        [Tooltip("Should the wait be randomized?")]

        public SharedBool RandomWait = false;
        [Tooltip("The minimum wait time if random wait is enabled")]
        public SharedFloat RandomWaitMin = 1f;
        [Tooltip("The maximum wait time if random wait is enabled")]
        public SharedFloat RandomWaitMax = 1f;

        private float _alreadyWaitTime;
        // The time to wait
        private float _waitDuration;

        public override void OnStart()
        {
            _alreadyWaitTime = 0f;

            if (RandomWait.Value)
            {
                float rand01 = (float)MersenneTwister.MT19937.Real1();
                _waitDuration = RandomWaitMin.Value + (RandomWaitMax.Value - RandomWaitMin.Value)*rand01;
                //_waitDuration = Random.Range(randomWaitMin.Value, randomWaitMax.Value);
            }
            else
            {
                _waitDuration = WaitTime.Value;
            }
        }

        public override TaskStatus OnUpdate()
        {
            _alreadyWaitTime += GameManager.s_deltaTime;

            if (_alreadyWaitTime > _waitDuration)
            {
                return TaskStatus.Success;
            }

            // Otherwise we are still waiting.
            return TaskStatus.Running;
        }

        public override void OnPause(bool paused)
        {
        }

        public override void OnReset()
        {
            // Reset the public properties back to their original values
            WaitTime = 1;

            RandomWait = false;
            RandomWaitMin = 1;
            RandomWaitMax = 1;
        }
    }
}
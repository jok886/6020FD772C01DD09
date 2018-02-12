using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections.Generic;

namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskCategory("My Action")]

    public class FindARandomGameObject : Action
    {
        [Tooltip("The tag of the GameObject to find")]
        public SharedString Tag;

        public SharedFloat MinDist = 0f;
        public SharedFloat MaxDist = 100f;

        [Tooltip("The objects found")]
        [RequiredField]
        public SharedGameObject StoreValue;

        public override void OnStart()
        {
        }

        public override TaskStatus OnUpdate()
        {
            List<GameObject> foundObjs = new List<GameObject>();

            var gameObjects = GameObject.FindGameObjectsWithTag(Tag.Value);
            for (int i = 0; i < gameObjects.Length; ++i)
            {
                var curGameObj = gameObjects[i];

                Vector3 dist = curGameObj.transform.position - this.gameObject.transform.position;
                if (dist.magnitude >= MinDist.Value && dist.magnitude <= MaxDist.Value)
                {
                    foundObjs.Add(curGameObj);
                }
            }

            if (foundObjs.Count > 0)
            {
                float rand01 = UnityEngine.Random.Range(0f, 1.0f); ;// (float)MersenneTwister.MT19937.Real3();//(0,1)
                int index = (int)(foundObjs.Count * rand01) % foundObjs.Count;//[0,foundObjs.Count-1]
                StoreValue.Value = foundObjs[index];

                return TaskStatus.Success;
            }

            return TaskStatus.Failure;
        }

        public override void OnReset()
        {
            Tag.Value = null;
            StoreValue.Value = null;
        }

    }

}
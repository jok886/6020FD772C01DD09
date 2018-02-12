using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using System.Collections;

namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskDescription("Check to see if the any objects are within sight of the agent.")]
    [TaskCategory("My Conditional")]
    //[HelpURL("http://www.opsive.com/assets/BehaviorDesigner/Movement/documentation.php?id=11")]
    [TaskIcon("Assets/Behavior Designer Movement/Editor/Icons/{SkinColor}CanSeeObjectIcon.png")]

    public class MyCanSeeObject : Conditional
    {
        //[BehaviorDesigner.Runtime.Tasks.Tooltip("The object that we are searching for.")]
        //public SharedGameObject TargetObject;

        public PlayerTeam.PlayerTeamType TargetTeamType = PlayerTeam.PlayerTeamType.HideTeam;
        private ArrayList _targetPlayerArray;

        //[BehaviorDesigner.Runtime.Tasks.Tooltip("The Tag of the objects that we are searching for")]
        //public string TargetTag;

        [BehaviorDesigner.Runtime.Tasks.Tooltip("The field of view angle of the agent (in degrees)")]
        public SharedFloat fieldOfViewAngle = 90;

        [BehaviorDesigner.Runtime.Tasks.Tooltip("The distance that the agent can see ")]
        public SharedFloat viewDistance = 10;

        [SharedRequired]
        [BehaviorDesigner.Runtime.Tasks.Tooltip("The object that is within sight")]
        public SharedGameObject StoreObjectInSight;

        //private GameObject[] _targetObjects;


        public override void OnStart()
        {
            //if(TargetObject.Value == null)
            //{
            //    _targetObjects = GameObject.FindGameObjectsWithTag(TargetTag);
            //}
            PlayerTeam team = GameObjectsManager.GetInstance().GetPlayerTeam(TargetTeamType);
            _targetPlayerArray = team.GetPlayers();
        }


        // Returns success if an object was found otherwise failure
        public override TaskStatus OnUpdate()
        {
            foreach(PlayerBase targetPlayer in _targetPlayerArray)
            {
                if (targetPlayer != null && targetPlayer.Hp > 0)
                {
                    //Debug.Log("MyCanSeeObject:fieldOfViewAngle=" + fieldOfViewAngle.Value);
                    bool canSeeTarge = MathHelper.CheckIsTargetInSector(this.gameObject.transform.position, this.gameObject.transform.forward, targetPlayer.gameObject, viewDistance.Value, fieldOfViewAngle.Value);
                    if (canSeeTarge)
                    {
                        if (StoreObjectInSight != null)
                        {
                            StoreObjectInSight.Value = targetPlayer.gameObject;
                        }

                        return TaskStatus.Success;
                    }
                }
            }

            if (StoreObjectInSight != null)
            {
                StoreObjectInSight.Value = null;
            }
            return TaskStatus.Failure;
        }

        // Draw the line of sight representation within the scene window
        public override void OnDrawGizmos()
        {
            //BehaviorDesigner.Runtime.Tasks.Movement.MovementUtility.DrawLineOfSight(Owner.transform, Vector3.zero, fieldOfViewAngle.Value, viewDistance.Value, false);
        }
    }

}
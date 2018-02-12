using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace BehaviorDesigner.Runtime.Tasks
{
    [TaskCategory("My Action")]

    public class MyMoveTo : Action
    {
        [Tooltip("The GameObject that the task operates on. If null the task GameObject is used.")]
        public SharedGameObject GameObjectToMove;

        public SharedGameObject DestinationGameObject;
        public SharedFloat StoppingDistance = 0f;

        public bool IsBlockMove = true;

        public bool MoveToPlaceNearby = false;
        public SharedFloat MaxNearDist = 3f; 

        private GameObject _gameObjectToMove;
        private PlayerBase _player;
        private Animator _animator;
        private UnityEngine.AI.NavMeshAgent _navMeshAgent;

        private Transform _destTrans;

        private float _offsetX;
        private float _offsetZ;

        public override void OnStart()
        {
            _offsetX = (float)(MersenneTwister.MT19937.Real1() * 2f - 1f) * MaxNearDist.Value;
            _offsetZ = (float)(MersenneTwister.MT19937.Real1() * 2f - 1f) * MaxNearDist.Value;

            _gameObjectToMove = GetDefaultGameObject(GameObjectToMove.Value);
            _player = _gameObjectToMove.GetComponent<PlayerBase>();
            _animator = _gameObjectToMove.GetComponent<Animator>();
            _navMeshAgent = _gameObjectToMove.GetComponent<UnityEngine.AI.NavMeshAgent>();

            if (DestinationGameObject != null && DestinationGameObject.Value != null)
            {
                _destTrans = DestinationGameObject.Value.transform;
            }

            if (_navMeshAgent != null && _destTrans != null)
            {
                _navMeshAgent.speed = _player.MaxMoveSpeed;
                _navMeshAgent.stoppingDistance = StoppingDistance.Value;

                Vector3 destPos = _destTrans.position;
                if (MoveToPlaceNearby)
                {
                    destPos.x += _offsetX;
                    destPos.z += _offsetZ;
                }

                //ת��  
                destPos.y = _gameObjectToMove.transform.position.y;
                _gameObjectToMove.transform.LookAt(destPos);

                //����Ѱ·��Ŀ���  
                _navMeshAgent.SetDestination(destPos);

            }
        }


        public override TaskStatus OnUpdate()
        {
            if(_navMeshAgent != null && _destTrans != null && _player.Hp > 0)
            {
                //LookAt 
                Vector3 destPos = _destTrans.position;
                destPos.y = _gameObjectToMove.transform.position.y;
                _gameObjectToMove.transform.LookAt(destPos);

                //Destination
                if (MoveToPlaceNearby)
                {
                    destPos.x += _offsetX;
                    destPos.z += _offsetZ;
                }
                destPos.y = _gameObjectToMove.transform.position.y;
                _navMeshAgent.SetDestination(destPos);

                //navMeshAgent����setDestination �󣬻���һ������·����ʱ�䣬
                //���������pathPendingΪtrue. �����������remainingDistanceһֱΪ0.
                if (!_navMeshAgent.pathPending && _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
                {
                    _player.AIMoveSpeed = 0f;

                    return TaskStatus.Success;
                }
                else
                {
                    //if (_animator)
                    //{
                    //    _animator.SetFloat("Speed_f", 1.0f);// _navMeshAgent.velocity.magnitude
                    //}
                    _player.AIMoveSpeed = 4.0f;

                    //if (_navMeshAgent.velocity.magnitude <= 0.01f)
                    //{
                    //    Debug.Log(" why speed < 0.01f");
                    //}

                    if(IsBlockMove)
                    {
                        return TaskStatus.Running;
                    }
                    else
                    {
                        return TaskStatus.Failure;
                    }
                    
                }
            }
            else
            {
                return TaskStatus.Failure;
            }
        }

    }

}
using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AI;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Polyperfect.Common
{
    [RequireComponent(typeof(Animator)), RequireComponent(typeof(CharacterController))]
    public class Common_WanderScript : MonoBehaviour
    {
        
        [SerializeField] public IdleState[] idleStates;
        [SerializeField] private MovementState[] movementStates;
        [SerializeField] private AIState[] attackingStates;
        [SerializeField] private AIState[] deathStates;

        [SerializeField] public string species = "NA";

        [SerializeField, Tooltip("This specific animal stats asset, create a new one from the asset menu under (LowPolyAnimals/NewAnimalStats)")]
        public AIStats stats;

        [SerializeField, Tooltip("How far away from it's origin this animal will wander by itself.")]
        private float wanderZone = 10f;

        public float MaxDistance
        {
            get { return wanderZone; }
            set
            {
#if UNITY_EDITOR
                SceneView.RepaintAll();
#endif
                wanderZone = value;
            }
        }

        // [SerializeField, Tooltip("How dominent this animal is in the food chain, agressive animals will attack less dominant animals.")]
        private int dominance = 1;
        private int originalDominance = 0;

        [SerializeField, Tooltip("How far this animal can sense a predator.")]
        private float awareness = 30f;

        [SerializeField, Tooltip("How far this animal can sense it's prey.")]
        private float scent = 30f;

        private float originalScent = 0f;

        // [SerializeField, Tooltip("How many seconds this animal can run for before it gets tired.")]
        private float stamina = 10f;
        public float Stamina
        {
            get { return stamina; }
        }

        // [SerializeField, Tooltip("How much this damage this animal does to another animal.")]
        private float power = 10f;

        // [SerializeField, Tooltip("How much health this animal has.")]
        private float toughness = 5f;

        // [SerializeField, Tooltip("Chance of this animal attacking another animal."), Range(0f, 100f)]
        private float aggression = 0f;
        private float originalAggression = 0f;

        // [SerializeField, Tooltip("How quickly the animal does damage to another animal (every 'attackSpeed' seconds will cause 'power' amount of damage).")]
        private float attackSpeed = 0.5f;

        // [SerializeField, Tooltip("If true, this animal will attack other animals of the same specices.")]
        private bool territorial = false;

        // [SerializeField, Tooltip("Stealthy animals can't be detected by other animals.")]
        private bool stealthy = false;

        [SerializeField, Tooltip("This animal will be peaceful towards species in this list.")]
        private string[] nonAgressiveTowards;

        //[Space(), Space(5)]
        [SerializeField, Tooltip("If true, this animal will rotate to match the terrain. Ensure you have set the layer of the terrain as 'Terrain'.")]
        private bool matchSurfaceRotation = true;

        [SerializeField, Tooltip("How fast the animnal rotates to match the surface rotation.")]
        private float surfaceRotationSpeed = 2f;

        //[Space(), Space(5)]
        [SerializeField, Tooltip("If true, AI changes to this animal will be logged in the console.")]
        private bool logChanges = false;

        [SerializeField, Tooltip("If true, gizmos will be drawn in the editor.")]
        private bool showGizmos = false;

        [SerializeField] private bool drawWanderRange = true;
        [SerializeField] private bool drawScentRange = true;
        [SerializeField] private bool drawAwarenessRange = true;

        public UnityEngine.Events.UnityEvent deathEvent;
        public UnityEngine.Events.UnityEvent attackingEvent;
        public UnityEngine.Events.UnityEvent idleEvent;
        public UnityEngine.Events.UnityEvent movementEvent;


        private Color distanceColor = new Color(0f, 0f, 205f);
        private Color awarnessColor = new Color(1f, 0f, 1f, 1f);
        private Color scentColor = new Color(1f, 0f, 0f, 1f);
        private Animator animator;
        private CharacterController characterController;
        private Vector3 origin;

        private Vector3 targetLocation = Vector3.zero;

        private float turnSpeed = 0f;

        private static List<Common_WanderScript> allAnimals = new List<Polyperfect.Common.Common_WanderScript>();

        public static List<Common_WanderScript> AllAnimals
        {
            get { return allAnimals; }
        }
        public enum WanderState
        {
            Walking,
            Running,
            Attack,
            Dead
        }

        float attackTimer = 0;
        float MinimumStaminaForAggression
        {
            get { return stats.stamina * .9f; }
        }

        float MinimumStaminaForFlee
        {
            get { return stats.stamina * .1f; }
        }

        public WanderState CurrentState;
        Common_WanderScript primaryPrey;
        Common_WanderScript primaryPursuer;
        Common_WanderScript attackTarget;
        float moveSpeed = 0f;
        float attackReach = 2f;
        bool forceUpdate = false;

        //성원 추가
        bool hasKilled = false;
        public bool HasKilled
        {
            get { return hasKilled; }
            set { hasKilled = value; }
        }

        MovementState runningState; //달리는 애니메이션 및 파라미터
        MovementState walkingState; //걷는 애니메이션 및 파라미터

        float staminaThreshold; //뛰려면 가지고 있어야하는 최소한의 스태미나
        //성원 추가 끝

        public void OnDrawGizmosSelected()
        {
            if (!showGizmos)
                return;

            if (drawWanderRange)
            {
                // Draw circle of radius wander zone
                Gizmos.color = distanceColor;
                Gizmos.DrawWireSphere(origin == Vector3.zero ? transform.position : origin, wanderZone);

                Vector3 IconWander = new Vector3(transform.position.x, transform.position.y + wanderZone, transform.position.z);
                Gizmos.DrawIcon(IconWander, "ico-wander", true);
            }

            if (drawAwarenessRange)
            {
                //Draw circle radius for Awarness.
                Gizmos.color = awarnessColor;
                Gizmos.DrawWireSphere(transform.position, awareness);


                Vector3 IconAwareness = new Vector3(transform.position.x, transform.position.y + awareness, transform.position.z);
                Gizmos.DrawIcon(IconAwareness, "ico-awareness", true);
            }

            if (drawScentRange)
            {
                //Draw circle radius for Scent.
                Gizmos.color = scentColor;
                Gizmos.DrawWireSphere(transform.position, scent);

                Vector3 IconScent = new Vector3(transform.position.x, transform.position.y + scent, transform.position.z);
                Gizmos.DrawIcon(IconScent, "ico-scent", true);
            }

            if (!Application.isPlaying)
                return;

            
            if (targetLocation != Vector3.zero)
            {
                Gizmos.DrawSphere(targetLocation + new Vector3(0f, 0.1f, 0f), 0.2f);
                Gizmos.DrawLine(transform.position, targetLocation);
            }
           
        }

        private void Awake()
        {
            if (!stats)
            {
                Debug.LogError(string.Format("No stats attached to {0}'s Wander Script.", gameObject.name));
                enabled = false;
                return;
            }

            animator = GetComponent<Animator>();

            var runtimeController = animator.runtimeAnimatorController;
            if (animator)
                animatorParameters.UnionWith(animator.parameters.Select(p => p.name));

            if (logChanges)
            {
                if (runtimeController == null)
                {
                    Debug.LogError(string.Format(
                        "{0} has no animator controller, make sure you put one in to allow the character to walk. See documentation for more details (1)",
                        gameObject.name));
                    enabled = false;
                    return;
                }

                if (animator.avatar == null)
                {
                    Debug.LogError(string.Format("{0} has no avatar, make sure you put one in to allow the character to animate. See documentation for more details (2)",
                        gameObject.name));
                    enabled = false;
                    return;
                }

                if (animator.hasRootMotion == true)
                {
                    Debug.LogError(string.Format(
                        "{0} has root motion applied, consider turning this off as our script will deactivate this on play as we do not use it (3)", gameObject.name));
                    animator.applyRootMotion = false;
                }

                if (idleStates.Length == 0 || movementStates.Length == 0)
                {
                    Debug.LogError(string.Format("{0} has no idle or movement states, make sure you fill these out. See documentation for more details (4)",
                        gameObject.name));
                    enabled = false;
                    return;
                }

                if (idleStates.Length > 0)
                {
                    for (int i = 0; i < idleStates.Length; i++)
                    {
                        if (idleStates[i].animationBool == "")
                        {
                            Debug.LogError(string.Format(
                                "{0} has " + idleStates.Length +
                                " Idle states, you need to make sure that each state has an animation boolean. See documentation for more details (4)", gameObject.name));
                            enabled = false;
                            return;
                        }
                    }
                }

                if (movementStates.Length > 0)
                {
                    for (int i = 0; i < movementStates.Length; i++)
                    {
                        if (movementStates[i].animationBool == "")
                        {
                            Debug.LogError(string.Format(
                                "{0} has " + movementStates.Length +
                                " Movement states, you need to make sure that each state has an animation boolean to see the character walk. See documentation for more details (4)",
                                gameObject.name));
                            enabled = false;
                            return;
                        }

                        if (movementStates[i].moveSpeed <= 0)
                        {
                            Debug.LogError(string.Format(
                                "{0} has a movement state with a speed of 0 or less, you need to set the speed higher than 0 to see the character move. See documentation for more details (4)",
                                gameObject.name));
                            enabled = false;
                            return;
                        }

                        if (movementStates[i].turnSpeed <= 0)
                        {
                            Debug.LogError(string.Format(
                                "{0} has a turn speed state with a speed of 0 or less, you need to set the speed higher than 0 to see the character turn. See documentation for more details (4)",
                                gameObject.name));
                            enabled = false;
                            return;
                        }
                    }
                }

                if (attackingStates.Length == 0)
                {
                    Debug.Log(string.Format("{0} has " + attackingStates.Length + " this character will not be able to attack. See documentation for more details (4)",
                        gameObject.name));
                }

                if (attackingStates.Length > 0)
                {
                    for (int i = 0; i < attackingStates.Length; i++)
                    {
                        if (attackingStates[i].animationBool == "")
                        {
                            Debug.LogError(string.Format(
                                "{0} has " + attackingStates.Length +
                                " attacking states, you need to make sure that each state has an animation boolean. See documentation for more details (4)",
                                gameObject.name));
                            enabled = false;
                            return;
                        }
                    }
                }

                if (stats == null)
                {
                    Debug.LogError(string.Format("{0} has no AI stats, make sure you assign one to the wander script. See documentation for more details (5)",
                        gameObject.name));
                    enabled = false;
                    return;
                }

                if (animator)
                {
                    foreach (var item in AllStates)
                    {
                        if (!animatorParameters.Contains(item.animationBool))
                        {
                            Debug.LogError(string.Format(
                                "{0} did not contain {1}. Make sure you set it in the Animation States on the character, and have a matching parameter in the Animator Controller assigned.",
                                gameObject.name, item.animationBool));
                            enabled = false;
                            return;
                        }
                    }
                }
            }

            origin = transform.position;
            animator.applyRootMotion = false;
            characterController = GetComponent<CharacterController>();
            

            //Assign the stats to variables
            originalDominance = stats.dominance;
            dominance = originalDominance;

            toughness = stats.toughness;
            territorial = stats.territorial;

            stamina = stats.stamina;

            originalAggression = stats.agression;
            aggression = originalAggression;

            attackSpeed = stats.attackSpeed;
            stealthy = stats.stealthy;

            originalScent = scent;
            scent = originalScent;

            if (matchSurfaceRotation && transform.childCount > 0)
            {
                transform.GetChild(0).gameObject.AddComponent<Common_SurfaceRotation>().SetRotationSpeed(surfaceRotationSpeed);
            }


            //성원 추가
            staminaThreshold = stats.stamina * 0.2f;

            runningState = null;
            var maxSpeed = 0f;
            foreach (var state in movementStates)
            {
                var stateSpeed = state.moveSpeed;
                if (stateSpeed > maxSpeed)
                {
                    runningState = state;
                    maxSpeed = stateSpeed;
                }
            }

            UnityEngine.Assertions.Assert.IsNotNull(runningState, string.Format("{0}'s wander script does not have any movement states.", gameObject.name));

            walkingState = null;
            var minSpeed = float.MaxValue;
            foreach (var state in movementStates)
            {
                var stateSpeed = state.moveSpeed;
                if (stateSpeed < minSpeed)
                {
                    walkingState = state;
                    minSpeed = stateSpeed;
                }
            }

            UnityEngine.Assertions.Assert.IsNotNull(walkingState, string.Format("{0}'s wander script does not have any movement states.", gameObject.name));
            //성원 추가 끝
        }

        IEnumerable<AIState> AllStates
        {
            get
            {
                foreach (var item in idleStates)
                    yield return item;
                foreach (var item in movementStates)
                    yield return item;
                foreach (var item in attackingStates)
                    yield return item;
                foreach (var item in deathStates)
                    yield return item;
            }
        }

        void OnEnable()
        {
            allAnimals.Add(this);
        }

        void OnDisable()
        {
            allAnimals.Remove(this);
            StopAllCoroutines();
        }

        //private void Start()
        //{
        //    setStart();
        //}

        public void setStart()
        {
            if (Common_WanderManager.Instance != null && Common_WanderManager.Instance.PeaceTime)
            {
                SetPeaceTime(true);
            }
            StartCoroutine(RandomStartingDelay());
        }

        bool started = false;
        readonly HashSet<string> animatorParameters = new HashSet<string>();

        public void updateAnimalState(Vector3 direction, int inputState)
        {
            if (!started) return;
            SetState((WanderState)inputState);
            switch (CurrentState)
            {
                case WanderState.Attack: break;
                case WanderState.Running:
                    stamina -= Time.deltaTime;
                    if (stamina <= 0f) SetState(WanderState.Walking);
                    break;
                case WanderState.Walking:
                    stamina = Mathf.MoveTowards(stamina, stats.stamina, Time.deltaTime);
                    break;
            }
            FaceDirection(direction.normalized);
            characterController.SimpleMove(moveSpeed * direction.normalized);
        }

        void FaceDirection(Vector3 facePosition)
        {
            transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(Vector3.RotateTowards(transform.forward,
                facePosition, turnSpeed * Time.deltaTime * Mathf.Deg2Rad, 0f), Vector3.up), Vector3.up);
        }

        public bool TakeDamage(float damage)
        {
            toughness -= damage;
            if (toughness <= 0f)
            {
                Die();
                return true;
            }
            return false;
        }
        public void Die()
        {
            SetState(WanderState.Dead);
        }

        public void SetPeaceTime(bool peace)
        {
            if (peace)
            {
                dominance = 0;
                scent = 0f;
                aggression = 0f;
            }
            else
            {
                dominance = originalDominance;
                scent = originalScent;
                aggression = originalAggression;
            }
        }

        public void SetState(WanderState state)
        {
            switch (state)
            {
                case WanderState.Running:
                    if (stamina > staminaThreshold)
                    {
                        CurrentState = state;
                        HandleBeginRunning();
                    }
                    break;
                case WanderState.Walking:
                    CurrentState = state;
                    HandleBeginWalking();
                    break;
                case WanderState.Attack:
                    CurrentState = state;
                    HandleBeginAttack();
                    break;
                case WanderState.Dead:
                    CurrentState = state;
                    HandleBeginDeath();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void ClearAnimatorBools()
        {
            foreach (var item in idleStates)
                TrySetBool(item.animationBool, false);
            foreach (var item in movementStates)
                TrySetBool(item.animationBool, false);
            foreach (var item in attackingStates)
                TrySetBool(item.animationBool, false);
            foreach (var item in deathStates)
                TrySetBool(item.animationBool, false);
        }

        void TrySetBool(string parameterName, bool value)
        {
            if (!string.IsNullOrEmpty(parameterName))
            {
                if (logChanges || animatorParameters.Contains(parameterName))
                    animator.SetBool(parameterName, value);
            }
        }

        void HandleBeginDeath()
        {
            ClearAnimatorBools();
            if (deathStates.Length > 0) TrySetBool(deathStates[Random.Range(0, deathStates.Length)].animationBool, true);

            //deathEvent.Invoke();
            enabled = false;
        }

        void HandleBeginAttack()
        {
            var attackState = Random.Range(0, attackingStates.Length);
            turnSpeed = 120f;
            ClearAnimatorBools();
            TrySetBool(attackingStates[attackState].animationBool, true);
            //attackingEvent.Invoke();
        }

        void HandleBeginRunning()
        {
            SetMoveFast();
            //movementEvent.Invoke();
        }

        void HandleBeginWalking()
        {
            primaryPrey = null;
            SetMoveSlow();
        }

        void SetMoveFast()
        {
            turnSpeed = runningState.turnSpeed;
            moveSpeed = runningState.moveSpeed;
            ClearAnimatorBools();
            TrySetBool(runningState.animationBool, true);
        }

        void SetMoveSlow()
        {
            turnSpeed = walkingState.turnSpeed;
            moveSpeed = walkingState.moveSpeed;
            ClearAnimatorBools();
            TrySetBool(walkingState.animationBool, true);
        }

        IEnumerator RandomStartingDelay()
        {
            yield return new WaitForSeconds(Random.Range(0f, 2f));
            started = true;
        }

        [ContextMenu("This will delete any states you have set, and replace them with the default ones, you can't undo!")]
        public void BasicWanderSetUp()
        {
            MovementState walking = new MovementState(), running = new MovementState();
            IdleState idle = new IdleState();
            AIState attacking = new AIState(), death = new AIState();

            walking.stateName = "Walking";
            walking.animationBool = "isWalking";
            running.stateName = "Running";
            running.animationBool = "isRunning";
            movementStates = new MovementState[2];
            movementStates[0] = walking;
            movementStates[1] = running;


            idle.stateName = "Idle";
            idle.animationBool = "isIdling";
            idleStates = new IdleState[1];
            idleStates[0] = idle;

            attacking.stateName = "Attacking";
            attacking.animationBool = "isAttacking";
            attackingStates = new AIState[1];
            attackingStates[0] = attacking;

            death.stateName = "Dead";
            death.animationBool = "isDead";
            deathStates = new AIState[1];
            deathStates[0] = death;
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer != gameObject.layer) return; //맞닿은 object가 Animal이 아니라면 return. generality 가 떨어지므로 추후 수정해야할 코드.
            if (started == false) return;
            if (CurrentState == WanderState.Attack) return; //만약 이미 공격 중이라면 새로운 object 가 공격 사거리에 들어와도 무시한다.

            Common_WanderScript targetObject = other.GetComponent<Common_WanderScript>();
             
            if(targetObject.dominance<dominance) //타겟이 피식자라면 공격 코루틴 시작
            {
                SetState(WanderState.Attack);
                StartCoroutine(attackCoroutine(targetObject));
            }
        }
        
        IEnumerator attackCoroutine(Common_WanderScript targetObject)
        {
            attackTimer = attackSpeed;
            while (attackTimer - 0.1 > 0) { attackTimer -= 0.1f; yield return new WaitForSeconds(0.1f); }//매 0.1초 만큼 시간을 태운다.
            if (targetObject.enabled == true) //공격 모션이 끝났다. 타겟이 아직 살아있다면 공격 시작
            {
                if (targetObject.TakeDamage(power)) //공격했는데 목표가 죽었다면
                {
                    hasKilled = true; //죽였음을 count 한다.
                }
            }
            SetState(WanderState.Walking); // 현재 상태를 걷기로 전환
            //findOtherTarget(); //사거리에 다른 목표가 있으면 찾고 공격한다. 
        }
    }
}
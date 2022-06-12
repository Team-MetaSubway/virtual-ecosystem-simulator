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

        // [SerializeField, Tooltip("How dominent this animal is in the food chain, agressive animals will attack less dominant animals.")]
        protected int dominance = 1;
        private int originalDominance = 0;

        public int Dominance
        {
            get { return dominance; }
        }

        // [SerializeField, Tooltip("How many seconds this animal can run for before it gets tired.")]
        protected float stamina = 10f;
        private float maxStamina;
        public float Stamina
        {
            get { return stamina; }
        }
        public float MaxStamina
        {
            get { return maxStamina; }
        }

        // [SerializeField, Tooltip("How much this damage this animal does to another animal.")]
        private float power = 10f;

        // [SerializeField, Tooltip("How much health this animal has.")]
        private float toughness = 5f;
        private float maxToughness;
        public float Toughness
        {
            get { return toughness; }
        }
        public float MaxToughness
        {
            get { return maxToughness; }
        }

        // [SerializeField, Tooltip("How quickly the animal does damage to another animal (every 'attackSpeed' seconds will cause 'power' amount of damage).")]
        private float attackSpeed = 0.5f;

        //[Space(), Space(5)]
        private bool matchSurfaceRotation = true;

        private float surfaceRotationSpeed = 2f;


        [SerializeField, Tooltip("If true, gizmos will be drawn in the editor.")]
        private bool showGizmos = false;

        public UnityEngine.Events.UnityEvent deathEvent;
        public UnityEngine.Events.UnityEvent attackingEvent;
        public UnityEngine.Events.UnityEvent idleEvent;
        public UnityEngine.Events.UnityEvent movementEvent;


        private Color distanceColor = new Color(0f, 0f, 205f);
        private Color awarnessColor = new Color(1f, 0f, 1f, 1f);
        private Color scentColor = new Color(1f, 0f, 0f, 1f);
        private Animator animator;

        public CharacterController characterController;
        private Vector3 origin;

        private Vector3 targetLocation = Vector3.zero;

        private float turnSpeed = 0f;
        public float TurnSpeed
        {
            get { return turnSpeed; }
        }

        private static List<Common_WanderScript> allAnimals = new List<Polyperfect.Common.Common_WanderScript>();

        public static List<Common_WanderScript> AllAnimals
        {
            get { return allAnimals; }
        }
        public enum WanderState
        {
            Walking, //반드시 0
            Running, //반드시 1
            Attack,
            Dead,
            FoundFood
        }

        public enum AnimalType //초식동물인가 육식동물인가?
        {
            Calnivore, //육식동물이 디폴트
            Herbivore //초식동물
        }
        [HideInInspector]
        public WanderState CurrentState;
        [HideInInspector]
        public float moveSpeed = 0f;

        //성원 추가
        [HideInInspector]
        public bool endFlag = false;

        protected float growthDuration;

        private float previousSpeed;

        private float detectionRange;
        public float DetectionRange
        {
            get { return detectionRange; }
        }

        [HideInInspector]
        public AnimalType animalType;

        bool hasKilled = false;
        public bool HasKilled
        {
            get { return hasKilled; }
            set { hasKilled = value; }
        }

        MovementState runningState; //달리는 애니메이션 및 파라미터
        MovementState walkingState; //걷는 애니메이션 및 파라미터

        float staminaThreshold; //뛰려면 가지고 있어야하는 최소한의 스태미나
        public float StaminaThreshold
        {
            get { return staminaThreshold; }
        }

        private HashSet<Common_WanderScript> attackTargetBuffer;
        List<Common_WanderScript> targetToErase;

        protected float attackRange;
        protected float attackRangeSquare;

        protected float hunger;
       
        protected float maxHunger;
        public float Hunger
        {
            get { return hunger; }
            set { hunger = value; }
        }
        public float MaxHunger
        {
            get { return maxHunger; }
        }

        float hpFactor;
        [HideInInspector]
        public float hungerFactor;

        private string objectTag;

        protected static int deadBodyLayer;
        protected static int animalLayer;

        protected float growthFactor;

        protected float weatherFactor;

        static float growthFactorEpsilon = 0.01f;

        private Vector3 direction;
        public Vector3 Direction
        {
            get { return Direction; }
            set { direction = value; }
        }

        protected float duration = 2f;

        protected float reproduceDuration;
        //성원 추가 끝

        public void OnDrawGizmosSelected()
        {
            if (!showGizmos)
                return;
            /*
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
            */
        }

        public virtual void Awake()
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
            /*
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
            */
            origin = transform.position;
            animator.applyRootMotion = false;
            characterController = GetComponent<CharacterController>();


            //Assign the stats to variables
            originalDominance = stats.dominance;
            dominance = originalDominance;

            toughness = stats.toughness;
            maxToughness = stats.toughness;

            stamina = stats.stamina;
            maxStamina = stats.stamina;


            attackSpeed = stats.attackSpeed;

            power = stats.power;

            hunger = stats.hunger;
            maxHunger = stats.hunger;

            hungerFactor = maxHunger * 0.3f;
            hpFactor = maxToughness * 0.01f;

            animalType = AnimalType.Calnivore;

            //Debug.Log("나는" + gameObject.tag + "조건은" + matchSurfaceRotation + transform.childCount);
            if (matchSurfaceRotation && transform.childCount > 0)
            {
                //Debug.Log("나는 " + gameObject.tag + " 로테이션 헬퍼 활성화.");
                transform.GetChild(0).gameObject.AddComponent<Common_SurfaceRotation>().SetRotationSpeed(surfaceRotationSpeed);
            }


            //성원 추가

            endFlag = false;

            growthDuration = DayNightSystem.instance.fullDayLength * 0.7f;

            reproduceDuration = DayNightSystem.instance.fullDayLength;

            if (gameObject.CompareTag("Lion"))
            {
                reproduceDuration = DayNightSystem.instance.fullDayLength * 2.6f;
                duration = 1.8f;
            }
            if(gameObject.CompareTag("Bear"))
            {
                reproduceDuration = DayNightSystem.instance.fullDayLength * 2.4f;
                duration = 1.9f;
            }
            if (gameObject.CompareTag("Cat"))
            {
                reproduceDuration = DayNightSystem.instance.fullDayLength * 1.4f;
            }
            if (gameObject.CompareTag("Boar"))
            {
                duration = 2.3f;
                reproduceDuration = DayNightSystem.instance.fullDayLength * 1.3f;
            }
            if (gameObject.CompareTag("Wolf"))
            {
                duration = 10.3f;
                reproduceDuration = DayNightSystem.instance.fullDayLength;
            }
            weatherFactor = 1.0f;
            detectionRange = stats.detectionRange;

            deadBodyLayer = LayerMask.NameToLayer("Ignore Raycast");
            animalLayer = LayerMask.NameToLayer("Animal");
            objectTag = gameObject.tag;
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

            attackRange = GetComponentInChildren<CapsuleCollider>().radius;
            attackRangeSquare = attackRange*attackRange;

            targetToErase = new List<Common_WanderScript>();
            attackTargetBuffer = new HashSet<Common_WanderScript>();
            //성원 추가 끝
        }

        public virtual IEnumerable<AIState> AllStates
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

        public virtual void OnEnable()
        {
            characterController.enabled = false;
            transform.localPosition = RandomObjectGenerator.instance.GetRandomPosition();
            transform.localRotation = Quaternion.Euler(0, Random.Range(0f, 359f), 0);
            characterController.enabled = true;

            gameObject.layer = animalLayer;
            gameObject.tag = objectTag;
            SetStart();
        }

        public virtual void OnDisable()
        {
            StopAllCoroutines();
            if (endFlag == false)
            {
                StartCoroutine(ReapDeadBodyCoroutine());
            }
            gameObject.layer = deadBodyLayer;
            gameObject.tag = "DeadBody";
            if (gameObject.transform.Find("Free Camera") != null) PlayerableController.instance.UpdateManually();
        }

        public virtual void SetStart()
        {
            if (Common_WanderManager.Instance != null && Common_WanderManager.Instance.PeaceTime)
            {
                SetPeaceTime(true);
            }
            stamina = maxStamina/2f;
            toughness = maxToughness/2f;
            hunger = maxHunger / 2f;
            growthFactor = 1f;

            SetState(WanderState.Walking);
            StartCoroutine(HungerCoroutine());
            StartCoroutine(HpCoroutine());
#if ENABLE_RESPAWN //ENABLE_RESPAWN 은 강화학습 용 세팅. 동물이 죽으면 자동 스폰. 강화학습할 때는 번식 코루틴을 실행하지 않는다.
            //nothing
#else
            StartCoroutine(ReproduceCoroutine());
#endif

            //StartCoroutine(RandomStartingDelay());
        }

        readonly HashSet<string> animatorParameters = new HashSet<string>();


        public virtual void FixedUpdate()
        {
            if (CurrentState == WanderState.Dead) return;
            if (toughness <= 0)
            {
                Die();
                return;
            }
            switch (CurrentState)
            {
                case WanderState.Running:
                    stamina -= Time.deltaTime;
                    break;
                case WanderState.Walking:
                    stamina = Mathf.MoveTowards(stamina, stats.stamina, Time.deltaTime);
                    break;
            }
            //FaceDirection(direction);
            characterController.SimpleMove(moveSpeed * direction);
        }

        public virtual void UpdateAnimalState(int inputState)
        {
            if (CurrentState == WanderState.Attack || CurrentState == WanderState.Dead) return;
            if (toughness <= 0)
            {
                Die();
                return;
            }
            if (CurrentState != (WanderState)inputState)
            {
                SetState((WanderState)inputState);
            }
        }

        public virtual void FaceDirection(Vector3 facePosition)
        {
            transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(Vector3.RotateTowards(transform.forward,
                facePosition, turnSpeed * Time.deltaTime * Mathf.Deg2Rad, 0f), Vector3.up), Vector3.up);
        }

        public virtual bool TakeDamage(float damage)
        {
            toughness -= damage;
            if (toughness <= 0f)
            {
                Die();
                return true;
            }
            return false;
        }

        public virtual void Die()
        {
            SetState(WanderState.Dead);
        }

        public virtual void SetPeaceTime(bool peace)
        {
            if (peace)
            {
                dominance = 0;
            }
            else
            {
                dominance = originalDominance;
            }
        }

        public virtual void SetState(WanderState state)
        {
            CurrentState = state;
            switch (state)
            {
                case WanderState.Running:
                    HandleBeginRunning();
                    break;
                case WanderState.Walking:
                    HandleBeginWalking();
                    break;
                case WanderState.Attack:
                    HandleBeginAttack();
                    break;
                case WanderState.Dead:
                    HandleBeginDeath();
                    break;
                case WanderState.FoundFood:
                    HandleBeginFoundFood();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public virtual  void ClearAnimatorBools()
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

        public virtual void TrySetBool(string parameterName, bool value)
        {
            if (!string.IsNullOrEmpty(parameterName))
            {
                if (animatorParameters.Contains(parameterName))
                    animator.SetBool(parameterName, value);
            }
        }

        public virtual void HandleBeginDeath()
        {
            ClearAnimatorBools();
            if (deathStates.Length > 0) TrySetBool(deathStates[Random.Range(0, deathStates.Length)].animationBool, true);

            //deathEvent.Invoke();
            enabled = false;
        }

        public virtual void HandleBeginAttack()
        {
            var attackState = Random.Range(0, attackingStates.Length);
            turnSpeed = 120f;
            ClearAnimatorBools();
            TrySetBool(attackingStates[attackState].animationBool, true);
            //attackingEvent.Invoke();
        }

        public virtual void HandleBeginRunning()
        {
            SetMoveFast();
            //movementEvent.Invoke();
        }

        public virtual void HandleBeginWalking()
        {
            SetMoveSlow();
        }

        public virtual void HandleBeginFoundFood()
        {
            SetMoveSlow();
        }

        public virtual void SetMoveFast()
        {
            turnSpeed = runningState.turnSpeed;
            moveSpeed = runningState.moveSpeed*growthFactor*weatherFactor;
            ClearAnimatorBools();
            TrySetBool(runningState.animationBool, true);
        }

        public virtual void SetMoveSlow()
        {
            turnSpeed = walkingState.turnSpeed;
            moveSpeed = walkingState.moveSpeed*growthFactor*weatherFactor;
            ClearAnimatorBools();
            TrySetBool(walkingState.animationBool, true);
        }

        public virtual IEnumerator RandomStartingDelay()
        {
            yield return new WaitForSeconds(Random.Range(0f, 2f));
        }

        [ContextMenu("This will delete any states you have set, and replace them with the default ones, you can't undo!")]
        public virtual void BasicWanderSetUp()
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

        private bool isCollidedWithWall = false;
        public bool IsCollidedWithWall
        {
            get { return isCollidedWithWall; }
            set { isCollidedWithWall = value; }
        }
        public virtual void OnTriggerEnter(Collider other)
        {
            //Terrain(특히 Env 처리), Animal(animalLayer 변수에 캐시됨), AttackBoundary(무시해야함), Detector 레이어(무시해야함)에 대해 처리.
            
            if (CurrentState == WanderState.Dead) return; //내가 현재 죽었다면 리턴. 오류 방지용 코드.
            
            if(other.CompareTag("Env")) //Terrain의 Env인 경우
            {
                Debug.Log("벽에 닿았다!");
                isCollidedWithWall = true;
                return;
            }

            if(other.gameObject.layer == animalLayer)
            {
                Common_WanderScript targetObject = other.GetComponent<Common_WanderScript>();

                if (targetObject.dominance < dominance) //타겟이 피식자라면 공격 코루틴 시작
                {
                    attackTargetBuffer.Add(targetObject);//삽입
                    if (CurrentState == WanderState.Attack) // 내가 현재 공격 중이라면 attackTargetBuffer에 삽입 후 리턴.
                    {
                        return;
                    }
                    SetState(WanderState.Attack);
                    StartCoroutine(AttackCoroutine(targetObject));
                }
            }
        }

        public virtual IEnumerator AttackCoroutine(Common_WanderScript targetObject)
        {
            yield return new WaitForSeconds(attackSpeed);//공격 속도만큼 시간을 태운다.
            if (targetObject.enabled == true) //공격 모션이 끝났다. 타겟이 아직 살아있다면 공격 시작
            {
                if (targetObject.TakeDamage(power)) //공격했는데 목표가 죽었다면
                {
                    CalculateHungerAndHp(targetObject);
                    if (gameObject.CompareTag("Wolf"))
                    {
                        gameObject.GetComponent<WolfAgent>().EatTogether();
                    }
                }
            }
            FindOtherTarget(); //사거리에 다른 목표가 있으면 찾고 공격한다. 
        }
        public virtual void FindOtherTarget()
        {
            //버퍼 안 object는 동물이고 dominance가 자신 object 보다 낮음이 보장됨.
            Common_WanderScript tmpAttackTarget = null;
            foreach (var target in attackTargetBuffer)
            {
                if (target.CurrentState == WanderState.Dead || (target.transform.position - transform.position).sqrMagnitude > (target.attackRange + attackRange) * (target.attackRange + attackRange)) //효율을 위해 제곱 비교.
                {
                    targetToErase.Add(target);
                    continue;
                }
                else
                {
                    tmpAttackTarget = target; //다음 공격 대상 찾음.
                    break;
                }
            }
            foreach (var target in targetToErase) attackTargetBuffer.Remove(target);
            targetToErase.Clear();
            if (tmpAttackTarget != null) StartCoroutine(AttackCoroutine(tmpAttackTarget));
            else SetState(WanderState.Walking); // 현재 상태를 걷기로 전환
        }
        public virtual IEnumerator HungerCoroutine()
        {
            while (true)
            {
                if (hunger > 0) hunger -= 1.0f; //배고픔이 0 이상일 경우 1 감소
                yield return new WaitForSeconds(duration); //1초 후에 다시 실행
            }
        }
        public virtual IEnumerator HpCoroutine()
        {
            while (true)
            {
                
                if (hunger > maxHunger * 0.5f) toughness = Mathf.Clamp(toughness + hpFactor, 0, 1.1f*maxToughness);
                else if (hunger <= 0) toughness -= hpFactor;
                
                yield return new WaitForSeconds(duration); //1초 후에 다시 실행.
            }
        }

        public virtual IEnumerator ReproduceCoroutine()
        {
            bool isHappy = false;
            while(true)
            {
                yield return new WaitForSeconds(reproduceDuration*(1 + Random.Range(-0.3f,0.3f))); //하루 기다린다.
                if (toughness >= 0.65f * maxToughness && hunger >= 0.65f * maxHunger) //조건 만족했을 경우
                {
                    if (isHappy == false) //처음 조건을 만족한 경우
                    {
                        isHappy = true;
                    }
                    else //두번 연속으로 조건을 만족한 경우 = 번식.
                    {
                        if (gameObject.CompareTag("Wolf")) RandomObjectGenerator.instance.AriseWolfNumber(gameObject);
                        else RandomObjectGenerator.instance.ReproduceAnimal(gameObject);//번식 코드 삽입
                        isHappy = false;
                    }
                }
                else continue;
            }
        }

        public virtual void CalculateHungerAndHp(Common_WanderScript target)
        {
            hunger = Mathf.Clamp( hunger + hungerFactor * (1 + target.MaxToughness / maxToughness),0,1.1f*maxHunger);
            toughness = Mathf.Clamp(toughness + 0.05f * maxToughness, 0, 1.1f*maxToughness);
        }

       
        public IEnumerator ChildGrowthCoroutine(GameObject parentObject)
        {
            growthFactor = 0.5f;
            
            maxToughness = growthFactor * stats.toughness; //최대 체력 반 까고
            toughness = 0.5f*maxToughness; //반피 맞춰줌.
            hpFactor = 0.01f * maxToughness;
            power = growthFactor * stats.power; //공격력 반 깜.
            transform.localScale = new Vector3(growthFactor,growthFactor,growthFactor); //크기 반으로 줄임.

            while (true)
            {
                yield return new WaitForSeconds(growthDuration); // 1/3일 기다리고
                growthFactor += 0.1f; //성장.

                maxToughness = growthFactor*stats.toughness;
                hpFactor = 0.01f * maxToughness;
                toughness += 0.05f * maxToughness;
                power = growthFactor * stats.power;
                //이동속도는 매번 바뀌어서, 여기서 안 바꾸고 부득이하게 SetMoveSlow, SetMoveFast에서 growthFactor 곱해줌.
                transform.localScale = new Vector3(growthFactor, growthFactor, growthFactor);

                if (growthFactor >= 1.0f - growthFactorEpsilon) break;
                else continue;
            }
        }

        public void RainImpact()
        {
            weatherFactor = 0.2f;
            SetState(CurrentState);
        }
        public void RainUnimpact()
        {
            weatherFactor = 1.0f;
            SetState(CurrentState);
        }

        IEnumerator ReapDeadBodyCoroutine()
        {
            yield return new WaitForSeconds(5.0f);
            gameObject.transform.parent = RandomObjectGenerator.instance.heaven.transform;
            yield return new WaitForSeconds(5.0f);
            Destroy(gameObject);
        }

        public void startGrowth(GameObject parentObject)
        {
            StartCoroutine(ChildGrowthCoroutine(parentObject));
        }
    }
}
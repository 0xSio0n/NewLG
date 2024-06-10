using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum ActionState {
    Interaction, Abort, Pause, Select,
    Attack, Skill, Switch, Target,
    Walk, Dash, Jump, Death,
    Left, Right,
}

public class PlayerActive : MonoBehaviour {
    //public Transform DamagePop;

    public CharacterController Body;
    //public AudioSource Sound;
    public Animator Anim;
    public Vector3 MoveUpdate;
    public float[] ActionTime;

    [Header("Character Module")]
    public InputPlayModule InputPlay;
    public EquipmentModule Equipment;
    public PocketModule Pocket;

    [Header("Character Target")]
    public Transform EnemyTarget;
    public List<Transform> EnemiesList;
    public GameObject[] EnemiesAll;
    public float EnemyRange;
    public int EnemyIndex;

    [Header("Character Data")]
    public Warlord UnitData;
    public float Speed = 6f;

    [Header("Character Equip")]
    public Transform FirstSlot;
    public Transform SecondSlot;
    public Transform ThirdSlot;
    public Transform FourthSlot;
    public Transform RightSlot;
    public Transform LeftSlot;

    [Header("Character State")]
    public bool IsIdle;
    public bool IsWalk;
    public bool IsDash;
    public bool IsJump;
    public bool IsAttack;
    public bool IsFall;
    public bool IsCast;
    public bool IsDeath;
    public bool mustDash;

    public static PlayerActive Instance { get; private set; }

    public Action OnEquipReady { get; set; }
    public Action OnSkillReady { get; set; }

    public event Action OnDashAffected;

    public event Action OnInteraction{
        add {
            interactHandler = null;
            if (value != null) {
                interactHandler += value;
            }
        }
        remove {
            interactHandler -= value;
        }
    }

    public event Action OnAbort {
        add {
            abortHandler = null;
            if (value != null) {
                abortHandler += value;
            }
        }
        remove {
            abortHandler -= value;
        }
    }

    private Action interactHandler;
    private Action abortHandler;
    private float gravity = -7.5f;

    private void Action(ActionState State) {
        switch (State) {
            case ActionState.Dash:
                IsDash = true;
                break;
            case ActionState.Walk:
                IsWalk = !IsWalk;
                break;
            case ActionState.Jump:
                StartCoroutine(JumpCoroutine());
                break;
            case ActionState.Attack:
                IsAttack = ActionTime[0] == 0;
                break;
            case ActionState.Target:
                TargetSwitch();
                break;
            case ActionState.Skill:
                OnSkillReady?.Invoke();
                break;
            case ActionState.Switch:
                OnEquipReady?.Invoke();
                break;
            case ActionState.Death:
                break;
            case ActionState.Interaction:
                interactHandler?.Invoke();
                break;
            case ActionState.Abort:
                abortHandler?.Invoke();
                break;
        }
    }

    private void TargetSwitch() {
        if (EnemiesList.Count > 0) {
            var count = EnemiesList.Count;
            if (count > 0) {
                ++EnemyIndex;
                if (EnemyIndex == count) {
                    EnemyIndex = 0;
                }
                EnemyTarget = EnemiesList[EnemyIndex];
            }
        } else {
            EnemyTarget = null;
            EnemiesList.Clear();
            TargetClosest();
            IsAttack = false;
        }
    }

    private void TargetClosest() {
        if (!IsDeath) {
            EnemiesAll = GameObject.FindGameObjectsWithTag("Enemy");
            for (int i = 0; i < EnemiesAll.Length; i++) {
                var distance = Vector3.Distance(transform.position, EnemiesAll[i].transform.position);
                if (distance > EnemyRange) {
                    EnemiesAll[i] = null;
                }
            }

            for (int i = 0; i < EnemiesAll.Length; i++) {
                if (EnemiesAll[i] != null && EnemiesList.Count < 4 && !EnemiesList.Contains(EnemiesAll[i].transform)) {
                    EnemiesList.Add(EnemiesAll[i].transform);
                }
            }

            if (EnemyTarget == null && EnemiesList.Count > 0) {
                EnemyTarget = EnemiesList[0];
            }
        }

        if (EnemiesList.Count > 0 && !IsDeath) {
            try {
                foreach (var enemy in EnemiesList) {
                    if (enemy == null) {
                        EnemiesList.Remove(enemy.transform);
                        break;
                    }

                    var distance = Vector3.Distance(transform.position, enemy.transform.position);
                    if (distance > EnemyRange) {
                        EnemiesList.Remove(enemy.transform);
                        EnemyTarget = EnemiesList.Count > 0 ? EnemiesList[0] : null;
                    }
                }
            } catch {
                EnemyTarget = null;
                EnemiesList.Clear();
                TargetClosest();
                IsAttack = false;
            }
        }
    }

    private void RangeAttack() {
        //var pos = (transform.position + MoveUpdate - transform.position).normalized;
        //var zValue = Mathf.Atan2(pos.x, pos.y) * Mathf.Rad2Deg;
        //var rotate = Quaternion.Euler(0f, 0f, zValue);
        //Instantiate(Projectile, transform.position, Quaternion.identity);
        //ActionTime[0] = 0.5f;
    }

    //private void ExternAction(Action action) {
    //    var evtLen = action.GetInvocationList().Length;
    //    for (int i = 0; i < evtLen - 1 && evtLen > 1; i++) {
    //        action.GetInvocationList()[i] = null;
    //    }
    //    action?.Invoke();
    //}

    private void MeleeAttack() {
        TargetClosest();
        if (EnemyTarget) {
            var dist = Vector3.Distance(EnemyTarget.position, transform.position);
            var toward = EnemyTarget.position - transform.position;
            transform.rotation = Rotation(toward, 50f);  

            if (dist < 15f) {
                if (dist > 7f) {
                    mustDash = true;
                }
                MoveUpdate = toward.normalized;
                if (dist > 1.7f) {
                    Move(toward);
                    if (dist < 7f && mustDash) {
                        StartCoroutine(DashCoroutine());
                    }
                    return;
                }
            }
        }
        StartCoroutine(AttackCoroutine());
        IsAttack = mustDash = false;

        //var cast = Physics.CircleCast(transform.position, 3f, MoveUpdate, 1.7f, LayerMask.GetMask("Enemy"));
        //if (cast.collider != null && cast.collider.gameObject) {
        //    var enemy = cast.collider.GetComponent<EnemyActive>();
        //    if (enemy.CurrentHp > 0) {
        //        enemy.TakeDamage = (transform.position, 1f);
        //        DamageActive.InstanceDamage(DamagePop, enemy.transform.position, 1f, DamageState.PlayerPhs);
        //    }
        //}
        ActionTime[0] = 0.5f;
    }

    private float Idle() {
        Anim.SetFloat("Move", 0f);
        Anim.SetFloat("AxisX", 0f);
        Anim.SetFloat("AxisZ", 0f);
        //Sound.Stop();
        return 0;
    }

    private float Move(Vector3 vector) {
        Anim.SetBool("IsMove", IsWalk);
        Anim.SetFloat("Move", IsWalk ? 1f : 2f);
        Anim.SetFloat("AxisX", IsWalk ? vector.x : 0f);
        Anim.SetFloat("AxisZ", IsWalk ? vector.z : 0f);
        //if (!Sound.isPlaying) {
        //    Sound.clip = Resources.Load<AudioClip>("Audio/Run");
        //    Sound.Play();
        //}
        return IsWalk ? 3f : 6f;
    }

    private Quaternion Rotation(Vector3 vector) {
        var forward = Camera.main.transform.forward;
        var right = Camera.main.transform.right;
        forward.y = right.y = 0;
        MoveUpdate = (vector.z * forward.normalized) + (vector.x * right.normalized);
        var rotate = Quaternion.LookRotation(MoveUpdate);
        return Quaternion.Slerp(rotate, transform.rotation, Time.deltaTime);
    }

    private Quaternion Rotation(Vector3 vector, float speed) {
        var lookPos = Quaternion.LookRotation(vector);
        return Quaternion.Slerp(transform.rotation, lookPos, Time.deltaTime * speed);
    }

    private IEnumerator JumpCoroutine() {
        if (!IsJump) {
            IsFall = false;
            IsJump = true;
            Anim.SetBool("IsJump", true);
            MoveUpdate.y++;
            Body.Move(MoveUpdate * Time.deltaTime);

            yield return new WaitForSeconds(0.5f);
            IsFall = true;
        }
    }

    private IEnumerator DashCoroutine() {
        Speed = 15f;
        Anim.SetFloat("Move", 3f);
        yield return new WaitForSeconds(0.25f);
        IsDash = false;
        OnDashAffected?.Invoke();
    }

    private IEnumerator AttackCoroutine() {
        IsCast = true;
        Speed = Idle();
        Anim.SetFloat("Variety", 0f);
        Anim.SetTrigger("Attack");
        yield return new WaitForSeconds(0.75f);
        IsCast = false;
    }

    private IEnumerator KnockbackCoroutine(Vector3 pos, float range) {
        yield return new WaitForSeconds(0.5f);
        transform.position += (transform.position - pos).normalized * range;
    }

    private void Movement() {
        Fall(); 
        var vector = InputPlay ? InputPlay.MoveHandler.normalized : Vector3.zero;
        IsIdle = (vector.x, vector.z) == (0, 0);
        if (IsIdle) {
            Idle();
        } else {
            Speed = Move(vector);
            if (IsDash && !IsWalk && !IsJump) {
                StartCoroutine(DashCoroutine());
            }
            if (!IsJump) {
                MoveUpdate = new Vector3(vector.x, MoveUpdate.y, vector.z);
                if (!IsWalk) {
                    transform.rotation = Rotation(vector);
                }
            }
        }
        Body.Move(Speed * Time.deltaTime * MoveUpdate);
    }

    //private void Movement(Vector3 vector) {
    //    Fall();
    //    IsRun = (vector.x, vector.z) == (0, 0);
    //    if (IsRun) {
    //        Idle();
    //        if (IsAttack) {
    //            MeleeAttack();
    //        }
    //    } else if (!IsCast) {
    //        Speed = Move(vector);
    //        if (IsDash && !IsWalk && !IsJump) {
    //            StartCoroutine(DashCoroutine());
    //        }
    //        IsAttack = false;
    //        if (!IsJump) {
    //            MoveUpdate = new Vector3(vector.x, MoveUpdate.y, vector.z);
    //            if (!IsWalk) {
    //                transform.rotation = Rotation(vector);
    //            }
    //        }
    //    }
    //    Body.Move(MoveUpdate * Speed * Time.deltaTime);
    //}

    private void Death() {
        //Anim.SetBool("IsDeath", IsDeath = CurrentHp <= 0);
    }

    private void Fall() {
        if (IsFall) {
            MoveUpdate.y += gravity * Time.deltaTime;
            Body.Move(MoveUpdate * Time.deltaTime);
            if (Body.isGrounded) {
                MoveUpdate = Vector3.zero;
                Anim.SetBool("IsJump", false);
                IsJump = false;
            }
        }
    }

    private void OnDestroy() {
        Equipment.ItemTmp = new();
        Equipment.Weapon = new();
        Equipment.Armor = new();
        Equipment.Talent = new();
        Equipment.RightWeapon = null;
        Equipment.LeftWeapon = null;
        Equipment.ArmorTier = null;
    }

    private void Start() {
        Instance = this;
        Body = GetComponent<CharacterController>();
        Anim = GetComponent<Animator>();
        //Sound = GetComponent<AudioSource>();

        ActionTime = new[] { 0f, 0f, 0f };
        UnitData = new Warlord(10, 5, 5, 10);
        Equipment = new(
            (prefab, parent) => { return Instantiate(prefab, parent); }, 
            (prefab) => { Destroy(prefab); }
        );

        EnemiesList = new();
        EnemyRange = 15f;
        EnemyIndex = 0;

        InputPlay.OnAction = Action;
        //Equipment.Player = this;
        //Input_GamePlay.Instance.OnMovement = Movement;
        //Input_GamePlay.Instance.OnAction = Action;
    }

    private void Update() {
        for (int i = 0; i < ActionTime.Length; i++) {
            if (ActionTime[i] > 0) {
                ActionTime[i] -= Time.deltaTime;
            }

            if (ActionTime[i] < 0) {
                ActionTime[i] = 0;
            }
        }
        Movement();
        Death();
    }
}

[Serializable]
public struct UnitPoint {
    public float CurrentPoint;
    public float MaximumPoint;

    public float CurrentStock {
        get {
            return CurrentPoint;
        }
        set {
            if (value > MaximumPoint) {
                value = MaximumPoint;
            } else if (value < 0) {
                value = 0;
            }
            CurrentPoint = value;
        }
    }

    public float MaximumStock {
        get {
            return MaximumPoint;
        }
        set {
            CurrentPoint = MaximumPoint = value;
        }
    }
}

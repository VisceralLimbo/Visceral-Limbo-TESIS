using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corpus_Attack_HammerCombo : BaseState, IStateEnergyCost
{

    [Header("References")]
    [SerializeField] AnimatorHandler _AnimHandler;
    [SerializeField] Corpus_Controller _Main_State;
    [SerializeField] WalkMovementStrategy _CheckMove;
    [SerializeField] IMovementStrategy _MoveStrat;
    [SerializeField] Transform _Target;
    [Space]

    [Header("Variables")]

    [SerializeField] string TransitionKey;
    [SerializeField] int EnergyCost;

    [Tooltip("Windup time")]
    [SerializeField] float TimingDuration;

    [Tooltip("Attack Duration")]
    [SerializeField] float AttackWindow;
    [SerializeField] bool CanExit = false;
    [SerializeField] float _KnockbackValue;
    [SerializeField] float attackradius;
    [SerializeField] float AttackDamage;
    [SerializeField] LayerMask _Mask;

    HashSet<Health_Component> DamagedEntities = new HashSet<Health_Component>();
    HashSet<Collider> ColliderEntities = new HashSet<Collider>();

    [Space]
    [Header("For Testing purposes")]
    [SerializeField] bool DrawWireframe;
    [SerializeField] Transform _Model;


    public override bool EvaluateTransitions(Dictionary<string, bool> GlobalParams, out BaseState TO)
    {
        if (CanExit == false) 
        {
            TO = null;
            return false;
        }

        stateMachine.SetGlobalCondition(TransitionKey, false);
        stateMachine.SetGlobalCondition(_Main_State.CheckDistanceForTransitions(), true);
        return base.EvaluateTransitions(GlobalParams, out TO);
    }

    public override void OnDeInitialize(VisceralStateMachine CTX)
    {
        base.OnDeInitialize(CTX);
    }

    Collider[] hits;
    public override void OnEnter(VisceralStateMachine CTX)
    {

        CTX.SetGlobalCondition(TransitionKey, false);
        base.OnEnter(CTX);

        CanExit = false;
        _Main_State.KillMovement();
        pulse = 0;
        DrawWireframe = true;
        _Target = _Main_State.Target;

        _AnimHandler.SetParameter("Corpus_Anim", "HammerCombo", AnimatorControllerParameterType.Trigger);

        DamagedEntities.Clear();
        ColliderEntities.Clear();
    }

    public override void OnExit(VisceralStateMachine CTX)
    {
        _Main_State.NotifyAttackFinished();
        DamagedEntities.Clear();
        ColliderEntities.Clear();
        base.OnExit(CTX);
    }

    public override void OnInitialize(VisceralStateMachine CTX)
    {
        base.OnInitialize(CTX);
        _Main_State = GetComponentInParent<Corpus_Controller>();

        if (_MoveStrat == null) _MoveStrat = CTX.GetComponentInChildren<IMovementStrategy>();
        _CheckMove = (WalkMovementStrategy)_MoveStrat;



    }

    float pulse = 0;

  
    public override void OnTick(VisceralStateMachine CTX, float TickRate)
    {
        if(_MoveStrat!= null)
        {
            _MoveStrat.UpdateVelocity(Vector3.zero);
        }
        /*
         if(pulse < TimingDuration) 
         {
             pulse += Time.deltaTime;

             if(pulse > 0.5)
             {
                 DrawWireframe = false;
             }

             //rotar hacia player
             if (_Target != null)
             {
                 Vector3 Dir =  _Target.transform.position - _MoveStrat.GetKCC().Capsule.transform.position;
                 _MoveStrat.UpdateRotation(Dir);
             }
         }
         else
         {
             hits = Physics.OverlapSphere(_Model.transform.position, attackradius);

             if (hits.Length > 0)
             {
                 foreach (Collider collider in hits)
                 {
                     // busca solo player healthcomp
                     if (collider.TryGetComponent(out Player_HealthComp playerHp))
                     {
                         if (playerHp.Context == _Main_State.playerContext)
                         {
                             continue;
                         }

                         var DamageScore = new DamageScore();
                         DamageScore.Attacker = _Main_State.playerContext;
                         DamageScore.Victim = playerHp.Context;
                         DamageScore.DamageAmount = AttackDamage;
                         DamageScore.ElementalDamage = ElementType.Physical;
                         DamageScore.FactionID = _Main_State.playerContext.faction;

                         Vector3 Dir = playerHp.Context.PlayerTransform.position - _Main_State.playerContext.PlayerTransform.position;
                         Dir.Normalize();

                         playerHp.TakeDamageWithKnockback(Dir,_KnockbackValue,DamageScore);
                     }
                 }
             }

             pulse = 0;
             CanExit = true;
         }
        */

        pulse += Time.deltaTime * TimeDilationManager.GlobalTimeScale;

        // 1) windup phase
        if(pulse < TimingDuration)
        {
            DrawWireframe = false;
            if( _Target != null)
            {
                Vector3 dir = _Target.transform.position - _MoveStrat.GetKCC().Capsule.transform.position;
                dir.y = 0;
                _MoveStrat.UpdateRotation(dir);
            }
        }

        // 2) attack phase
        else if(pulse < TimingDuration + AttackWindow)
        {
            DrawWireframe = true;

            Collider[] Hits = Physics.OverlapSphere(_Model.transform.position + (_Model.transform.forward + Vector3.forward), attackradius, _Mask);

            if(Hits.Length > 0)
            {
                foreach(Collider col in Hits)
                {
                    #region DamageCalculation:
                    if (col.TryGetComponent(out IDamageable IDamage) && !ColliderEntities.Contains(col))
                    {
                        if(IDamage.GetHealthComponent(out Health_Component IDam_HPComp) && !DamagedEntities.Contains(IDam_HPComp))
                        {
                            if(IDam_HPComp.Context != null && IDam_HPComp.Context != _Main_State.playerContext)
                            {
                                DamageScore Dms = new DamageScore
                                {
                                    Attacker = _Main_State.playerContext,
                                    DamageAmount = AttackDamage,
                                    FactionID = _Main_State.playerContext.faction,
                                };

                                Vector3 KnockbackDir = _Target.transform.position - _MoveStrat.GetKCC().Capsule.transform.position;
                                KnockbackDir.y = 0;
                                KnockbackDir.Normalize();
                                KnockbackDir.y = 0.7f;


                                IDam_HPComp.TakeDamageWithKnockback(KnockbackDir, _KnockbackValue, Dms);

                                DamagedEntities.Add(IDam_HPComp);
                                ColliderEntities.Add(col);

                            }
                            else if(IDam_HPComp.Context == _Main_State.playerContext)
                            {
                                DamagedEntities.Add(IDam_HPComp);
                                ColliderEntities.Add(col);
                            }
                        }
                    }
                    // fallback
                    else
                    {
                        Health_Component HPComp = col.GetComponentInChildren<Health_Component>();

                        if(HPComp != null && HPComp.Context != null && !ColliderEntities.Contains(col) && HPComp.Context != _Main_State.playerContext)
                        {
                            DamageScore Dms = new DamageScore
                            {
                                Attacker = _Main_State.playerContext,
                                DamageAmount = AttackDamage,
                                FactionID = _Main_State.playerContext.faction,
                            };

                            Vector3 KnockbackDir = _Target.transform.position - _MoveStrat.GetKCC().Capsule.transform.position;
                            KnockbackDir.y = 0;
                            KnockbackDir.Normalize();
                            KnockbackDir.y = 0.7f;

                            HPComp.TakeDamageWithKnockback(KnockbackDir, _KnockbackValue, Dms);
                            DamagedEntities.Add(HPComp);
                            ColliderEntities.Add(col);
                        }
                        else if(HPComp != null && HPComp.Context == _Main_State.playerContext)
                        {
                            DamagedEntities.Add(HPComp);
                            ColliderEntities.Add(col);
                        }
                    }
                    #endregion
                }

            }
            else
            {
                print("no collider found");
            }

        }
        else
        {
            DrawWireframe = false;
            CanExit = true;

            stateMachine.SetGlobalCondition(TransitionKey, false);
            stateMachine.SetGlobalCondition(_Main_State.CheckDistanceForTransitions(), true);
        }
    }

    public void SetCost(float newCost)
    {
        EnergyCost = (int)newCost;
    }

    public int GetCost()
    {
        return EnergyCost;
    }

    public BaseState GetState()
    {
        return this;
    }

    public string GetTransitionKey()
    {
        return TransitionKey;
    }


    public void OnDrawGizmos()
    {
        if(DrawWireframe)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_Model.transform.position + (_Model.transform.forward + Vector3.forward), attackradius);
        }


    }
}

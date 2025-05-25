using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;

public class DumbChaserAI : DumbEnemy, ICharacterController
{

    [Header("References")]
    /// <summary> the context of the character, should have most useful information</summary>
    public PlayerContext context;
    /// <summary>
    /// The Kinematic Character motor 
    /// </summary>
    [SerializeField] KinematicCharacterMotor _KKC;
    [SerializeField] Rigidbody _rb;
    [SerializeField] Transform target;
    [SerializeField] Charge_Collider _Collider;
    [Space]


    [Header("Variables")]
    ///<summary> velocidad actual del chaser</summary>
    [SerializeField] float speed; //velocidad actual
    [SerializeField] float movementspeed; //velocidad normal de movimiento
    [SerializeField] float attackingspeed; // velocidad de carga de ataque, debería de ser mas lenta
    [SerializeField] float dashspeed; // burst de movimiento de dash, se aplica 1 sola vez
    [SerializeField] float DashRate; // tiempo necesario para dashear
    [SerializeField] float MovementAcceleration; // aceleracion de movimiento
    [SerializeField] float distanceToTarget; //distancia al objetivo
    [SerializeField] float rotationspeed; // velocidad de rotacion
    [SerializeField] bool SuccessfullAttack; // booleano de ataque realizado


    Vector3 RequestedAdditiveVelocity; // KKC, pedido de velocidad extra aditiva
    Vector3 RequestedForceVelocity; // kkc, pedido de fuerza al rigidbody

    private Vector3 targetdirection;


    private void Start()
    {
        _KKC= GetComponent<KinematicCharacterMotor>();
        _KKC.CharacterController = this;

        if(_rb == null) _rb = GetComponent<Rigidbody>();

        _KKC.AttachedRigidbodyOverride = _rb;
        if(target== null) target = PlayerTransform();

        context= GetComponent<PlayerContext>();
        if(context == null)
        {
            context = this.gameObject.AddComponent<PlayerContext>();
        }

        if(_Collider == null) _Collider = GetComponentInChildren<Charge_Collider>();
        _Collider.VS_InitializeWithParameters(context, this.gameObject);
    }


    private void Update()
    {
        if(target == null)
        {
            target = PlayerTransform();
            if (target == null)
            {
                Debug.LogError("Player transform is null");
                return;
            }
        }
        if (_KKC.AttachedRigidbody == null) _KKC.AttachedRigidbodyOverride = _rb;

        targetdirection = target.transform.position - _KKC.Capsule.transform.position;
        distanceToTarget = Vector3.Distance(_KKC.Capsule.transform.position, target.transform.position);

        if (distanceToTarget < 10)
        {
            if (!SuccessfullAttack)
            {
                StartCoroutine(dashwindup());
            }
        }
        else
        {
            SuccessfullAttack = false;
            StopAllCoroutines();
            speed = movementspeed;
        }
    }


    //time slicing?
    IEnumerator dashwindup()
    {
        speed = attackingspeed;
        SuccessfullAttack = true;
        yield return new WaitForSeconds(DashRate);
        Vector3 Dashattack = targetdirection.normalized * dashspeed;
        RequestForceVelocity(Dashattack);
        SuccessfullAttack = false;
    }

    private void RequestExtraVelocity(Vector3 ExtraVelocity)
    {
        _KKC.ForceUnground(0.1f);
        RequestedAdditiveVelocity += ExtraVelocity;
    }

    private void RequestForceVelocity(Vector3 ForceVelocity)
    {
        _KKC.ForceUnground(0.1f);
        RequestedForceVelocity = ForceVelocity;
    }

    public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
    {
        if (_KKC.GroundingStatus.IsStableOnGround)
        {


            var groundedMovement = _KKC.GetDirectionTangentToSurface(
                direction: targetdirection,
                surfaceNormal: _KKC.GroundingStatus.GroundNormal

                );


            var TargetVelocity = speed * groundedMovement;
            currentVelocity = Vector3.Slerp
                (
                    a: currentVelocity,
                    b: TargetVelocity,
                    t: 1f - Mathf.Exp(-MovementAcceleration * deltaTime)

                );

        }
        else
        {
            currentVelocity = new Vector3(0, -10, 0);
        }


        if(RequestedAdditiveVelocity.sqrMagnitude > 0)
        {

            currentVelocity += RequestedAdditiveVelocity;
            RequestedAdditiveVelocity = Vector3.zero;
        }

        if(RequestedForceVelocity.sqrMagnitude > 0)
        {
            _KKC.AttachedRigidbody.AddForce(RequestedForceVelocity, ForceMode.Impulse);
            RequestedForceVelocity= Vector3.zero;
        }


            
    }


    public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
    {
        var forward = Vector3.ProjectOnPlane(
                vector: targetdirection,
                _KKC.CharacterUp


            );

        currentRotation = Quaternion.LookRotation(forward, _KKC.CharacterUp);
    }

    private Transform PlayerTransform()
    {
        return playerContext.PlayerTransform;
    }



    public void AfterCharacterUpdate(float deltaTime)
    {
        context.KCCMotor = this._KKC;
        context.PlayerGameObject = this.gameObject;
        context.PlayerTransform = _KKC.Capsule.transform;

        if(_KKC.AttachedRigidbody.velocity.sqrMagnitude > 0 )
        {
            var Dir = Vector3.Distance(target.transform.position, _KKC.Capsule.transform.position);
            _Collider.ToggleAttack(true);
            if(Dir > 10 )
            {
                _KKC.AttachedRigidbody.velocity = Vector3.Slerp(_KKC.AttachedRigidbodyVelocity, Vector3.zero, 0.1f + Time.deltaTime);
                if (_KKC.AttachedRigidbodyVelocity.sqrMagnitude <= 0.1f)
                {
                    _KKC.AttachedRigidbody.velocity = Vector3.zero;
                    _Collider.ToggleAttack(false);
                }

            }
         
        }
    
    }

    public void BeforeCharacterUpdate(float deltaTime)
    {
       
    }

    public bool IsColliderValidForCollisions(Collider coll)
    {
        return true;
    }

    public void OnDiscreteCollisionDetected(Collider hitCollider)
    {

    }

    public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {

    }

    public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
    {

    }

    public void PostGroundingUpdate(float deltaTime)
    {

    }

    public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
    {

    }

    public override void SetTransformAndRotation(Transform newtransform, Quaternion newrotation)
    {
        _KKC.SetPositionAndRotation(newtransform.position,newrotation);
    }
}

//
// Script creado por patricio malvasio 2/5/2025
// este script es un prototipo de ia de enemigo.
// idealmente será reemplazado por otro más adelante.
//
//
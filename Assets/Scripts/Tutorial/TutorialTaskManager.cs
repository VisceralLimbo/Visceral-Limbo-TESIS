using UnityEngine;

public class TutorialTaskManager : MonoBehaviour
{
    private bool tutorialActive = false;

    public enum TutorialTaskType
    {
        Move,
        Jump,
        Attack,
        Crouch
    }

    private Player_InputHandler input;

    public bool moveDone;
    public bool jumpDone;
    public bool crouchDone;
    public bool attackDone;
    public bool AllCompleted => moveDone && jumpDone && crouchDone && attackDone;

    [SerializeField] private Player_MeleeComboComponent meleeAttack;

    void Start()
    {
        input = Player_InputHandler.instance;

        if (meleeAttack != null)
        {
            meleeAttack.OnMeleeAttackCompleted += OnAttackPerformed;
        }
    }

    void Update()
    {
        if (!tutorialActive) return;

        var data = input.CurrentMovementInput;

        //  MOVIMIENTO
        if (!moveDone && data.Movement.magnitude > 0.1f)
        {
            moveDone = true;
        }

        //  SALTO
        if (!jumpDone && data.Jumping)
        {
            jumpDone = true;
        }

        //  AGACHARSE
        if (!crouchDone && data.Crouch == CrouchEnum.Toggle)
        {
            crouchDone = true;
        }
    }

    void OnAttackPerformed()
    {
        if (!tutorialActive) return;

        if (!attackDone)
        {
            attackDone = true;
        }
    }

    void OnDestroy()
    {
        if (meleeAttack != null)
        {
            meleeAttack.OnMeleeAttackCompleted -= OnAttackPerformed;
        }
    }

    public void EnableTutorial()
    {
        tutorialActive = true;
    }
}

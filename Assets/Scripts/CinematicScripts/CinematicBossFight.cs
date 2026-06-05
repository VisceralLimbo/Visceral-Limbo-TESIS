using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class CinematicBossFight : MonoBehaviour
{
    [Header("Timeline")]
    public PlayableDirector playableDirector;

    [Header("Camera")]
    [SerializeField] private Transform cameraRig;

    private Transform rootCamera;
    private Transform originalParent;

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;

    private Player_Base player_Base;

    [SerializeField] private VisceralStateMachine visceralStateMachine;
    [SerializeField] private GameObject Boss;

    private bool cinematicPlayed = false;

    private GameObject gameplayUI;

    private void Awake()
    {
        if (playableDirector != null)
        {
            playableDirector.stopped += OnTimelineFinished;
        }
    }

    private void Start()
    {
        if (player_Base == null)
        {
            player_Base = FindObjectOfType<Player_Base>();
        }

        if (visceralStateMachine != null)
        {
            visceralStateMachine.enabled = false;
        }

        if (Boss != null)
        {
            Boss.SetActive(false);
        }

        gameplayUI = GameObject.FindGameObjectWithTag("UICombat");

        GameObject cam = GameObject.Find("RootCamera");

        if (cam != null)
        {
            rootCamera = cam.transform;

            originalParent = rootCamera.parent;
            originalLocalPosition = rootCamera.localPosition;
            originalLocalRotation = rootCamera.localRotation;
        }
        else
        {
            Debug.LogError("No se encontró un objeto llamado RootCamera.");
        }
    }

    private void Update()
    {
        if (!cinematicPlayed)
            return;

        if (playableDirector != null &&
            playableDirector.state == PlayState.Playing)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SkipCinematic();
            }
        }
    }

    private void OnDestroy()
    {
        if (playableDirector != null)
        {
            playableDirector.stopped -= OnTimelineFinished;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (cinematicPlayed)
            return;

        if (other.CompareTag("Player"))
        {
            cinematicPlayed = true;

            player_Base = other.GetComponentInParent<Player_Base>();

            SetPlayerCinematicLock(true);

            PlayCinematic();
        }
    }

    private void PlayCinematic()
    {
        AttachCameraToRig();

        if (gameplayUI != null)
        {
            gameplayUI.SetActive(false);
        }

        playableDirector.time = 0;
        playableDirector.Play();
    }

    private void SkipCinematic()
    {
        playableDirector.Stop();
    }

    private void OnTimelineFinished(PlayableDirector director)
    {
        DetachCameraFromRig();

        if (gameplayUI != null)
        {
            gameplayUI.SetActive(true);
        }

        if (player_Base != null)
        {
            SetPlayerCinematicLock(false);
            player_Base.SetPlayerActive();
        }

        if (visceralStateMachine != null)
        {
            visceralStateMachine.enabled = true;
        }

        if (Boss != null)
        {
            Boss.SetActive(true);
        }
    }

    private void AttachCameraToRig()
    {
        if (rootCamera == null || cameraRig == null)
        {
            Debug.LogWarning("RootCamera o CameraRig es null.");
            return;
        }

        rootCamera.SetParent(cameraRig);

        rootCamera.localPosition = Vector3.zero;
        rootCamera.localRotation = Quaternion.identity;
    }

    private void DetachCameraFromRig()
    {
        if (rootCamera == null)
            return;

        rootCamera.SetParent(originalParent);

        rootCamera.localPosition = originalLocalPosition;
        rootCamera.localRotation = originalLocalRotation;
    }

    private void SetPlayerCinematicLock(bool locked)
    {
        if (player_Base == null)
            return;

        Player_Movement movement = player_Base.GetComponentInChildren<Player_Movement>();

        if (movement != null)
        {
            if (locked)
                movement.LockMovementImmediate();
            else
                movement.UnlockMovement();
        }

        if (locked)
        {
            player_Base.StopWalkSound();
        }

        player_Base.enabled = !locked;
    }
}
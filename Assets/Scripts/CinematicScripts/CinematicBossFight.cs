using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class CinematicBossFight : MonoBehaviour
{
    [Header("Cameras")]
    public GameObject cinematicRoot;

    [Header("Timeline")]
    public PlayableDirector playableDirector;

    private Player_Base player_Base;

    [SerializeField] private VisceralStateMachine visceralStateMachine;

    [SerializeField] private GameObject Boss;

    private Camera mainCamera;

    private bool cinematicPlayed = false;

    private GameObject gameplayUI;


    private void Awake()
    {

        playableDirector.stopped += OnTimelineFinished;
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

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        gameplayUI = GameObject.FindGameObjectWithTag("UICombat");
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
        playableDirector.stopped -= OnTimelineFinished;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (cinematicPlayed) return;

        if (other.CompareTag("Player"))
        {
            cinematicPlayed = true;
            player_Base = other.GetComponentInParent<Player_Base>();
            SetPlayerCinematicLock(true);
            Camera.main.enabled = false;
            PlayCinematic();
        }
    }

    private void PlayCinematic()
    {

        cinematicRoot.SetActive(true);

        if (gameplayUI != null)
            gameplayUI.SetActive(false);

        playableDirector.time = 0;
        playableDirector.Play();
    }

    private void SkipCinematic()
    {
        playableDirector.Stop();
    }

    private void OnTimelineFinished(PlayableDirector director)
    {

        cinematicRoot.SetActive(false);

        if (gameplayUI != null)
            gameplayUI.SetActive(true);

        if (player_Base != null)
        {
            SetPlayerCinematicLock(false);
            player_Base.SetPlayerActive();
        }

        if (mainCamera != null)
        {
            mainCamera.enabled = true;
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

    private void SetPlayerCinematicLock(bool locked)
    {
        if (player_Base == null) return;

        Player_Movement movement = player_Base.GetComponentInChildren<Player_Movement>();

        if (movement != null)
        {
            if (locked) movement.LockMovementImmediate();
            else movement.UnlockMovement();
        }

        if (locked)
        {
            player_Base.StopWalkSound();
        }

        player_Base.enabled = !locked;
    }
}
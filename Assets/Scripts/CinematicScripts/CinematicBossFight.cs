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

    private void Awake()
    {
        
        playableDirector.stopped += OnTimelineFinished;
    }
    private void Start()
    {
        if(visceralStateMachine != null)
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
            player_Base.SetPlayerInactive();
            Camera.main.enabled = false;
            PlayCinematic();
        }
    }

    private void PlayCinematic()
    {
        
        cinematicRoot.SetActive(true);

        
        playableDirector.time = 0;
        playableDirector.Play();
    }

    private void OnTimelineFinished(PlayableDirector director)
    {
        
        cinematicRoot.SetActive(false);
        
        player_Base?.SetPlayerActive();

        if(mainCamera != null)
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
}
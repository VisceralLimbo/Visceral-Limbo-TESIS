using System.Collections;
using UnityEngine;

public class TutorialDummyShooter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    [SerializeField] private BulletDumb bulletPrefab;
    [SerializeField] private Transform shootPoint;

    [Header("Settings")]
    [SerializeField] private float shootDelay = 2f;

    [Header("Owner")]
    [SerializeField] private PlayerContext ownerContext;

    private void Start()
    {
        StartCoroutine(ShootLoop());
    }

    IEnumerator ShootLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(shootDelay);

            Shoot();
        }
    }

    void Shoot()
    {
        // a fixear tira error 
        // animacion 
        //if (animator != null)
        //{
        //    animator.SetTrigger("Attack");
        //}

        if (bulletPrefab != null && shootPoint != null)
        {
            BulletDumb bullet = Instantiate(
                bulletPrefab,
                shootPoint.position,
                shootPoint.rotation
            );

            bullet.SetOwner(gameObject, ownerContext);
        }
    }
}
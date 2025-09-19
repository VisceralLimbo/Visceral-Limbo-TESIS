using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class DebugCheats : MonoBehaviour
{
    
    [SerializeField]List<DumbEnemy> m_EnemyList;
    [SerializeField] SoundData TestSound;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            /*m_EnemyList.Clear();
            m_EnemyList = FindObjectsByType<DumbEnemy>(FindObjectsSortMode.None).ToList();

            foreach(var enemy in m_EnemyList)
            {
                var hpcomp = enemy.transform.root.GetComponentInChildren<Health_Component>();

                var Dmg = new DamageScore()
                {
                    DamageAmount = 1000f,
                    Attacker = null,

                };
                hpcomp?.TakeDamage(Dmg);
                print(enemy.name);
            }*/
        }

        if(Input.GetKeyDown(KeyCode.L))
        {
            SceneManager.LoadScene(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SoundManager.Instance.CreateSound()
                                 .WithSoundData(TestSound)
                                 .WithRandomPitch(default)
                                 .play();
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            SceneManager.LoadScene(1);
        }
        if (Input.GetKeyDown(KeyCode.K)) 
        {
            BloodEchoesManager.AddBloodEchoes(500);
            Debug.Log("blood echoes" + BloodEchoesManager.BloodEchoes);
        }
        
    }
}

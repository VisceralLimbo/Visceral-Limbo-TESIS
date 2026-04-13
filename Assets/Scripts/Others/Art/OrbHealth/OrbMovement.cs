using UnityEngine;

public class OrbMovement : MonoBehaviour
{
    private float speedY;
    private bool inFloor = false;

    [SerializeField] private float initialForceMin = 4f;
    [SerializeField] private float initialForceMax = 6f;
    [SerializeField] private float gravity = -9.8f;

    private float floorY;

    public void Init()
    {
        speedY = Random.Range(initialForceMin, initialForceMax);
        floorY = transform.position.y;
    }

    void Update()
    {
        if (inFloor) return;

        speedY += gravity * Time.deltaTime;

        transform.position += new Vector3(0, speedY * Time.deltaTime, 0);

        if (transform.position.y <= floorY)
        {
            transform.position = new Vector3(transform.position.x, floorY, transform.position.z);
            inFloor = true;
        }
    }
}

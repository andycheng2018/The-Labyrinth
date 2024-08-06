using UnityEngine;

namespace AC
{
    public class LavaProjectile : MonoBehaviour
    {
        public float speed = 10f;
        public float lifetime = 5f;
        public float gravityScale = 0.5f;

        private Rigidbody rb;

        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            Destroy(gameObject, lifetime);
            Launch();
        }

        public void Launch()
        {
             Vector3 randomDirection = Random.onUnitSphere;
            randomDirection.y = Mathf.Abs(randomDirection.y); // Ensure it goes upwards
            rb.AddForce(randomDirection * speed, ForceMode.Impulse);
        }

        private void Update()
        {
            rb.AddForce(Physics.gravity * gravityScale * Time.deltaTime, ForceMode.Acceleration);
        }
    }
}

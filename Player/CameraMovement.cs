using UnityEngine;

namespace AC
{
    public class CameraMovement : MonoBehaviour
    {
        public float speed = 1;
        public float rotAngle = 70;

        private void Update()
        {
            transform.rotation = Quaternion.Euler(0, Mathf.Sin(Time.realtimeSinceStartup * speed) * rotAngle, 0);
        }
    }
}


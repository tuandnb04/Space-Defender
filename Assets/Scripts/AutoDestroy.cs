using UnityEngine;

namespace SpaceDefender
{
    public class AutoDestroy : MonoBehaviour
    {
        public float lifetime = 1.5f;

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }
    }
}

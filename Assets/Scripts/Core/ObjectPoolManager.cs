using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public class ObjectPoolManager : MonoBehaviour
    {
        private static ObjectPoolManager _instance;
        private readonly Dictionary<GameObject, GameObject> _instanceToPrefabMap = new();

        private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new();

        private static ObjectPoolManager Instance
        {
            get
            {
                if (_instance) return _instance;
                _instance = FindAnyObjectByType<ObjectPoolManager>(FindObjectsInactive.Include);
                if (_instance) return _instance;
                var go = new GameObject("ObjectPoolManager");
                _instance = go.AddComponent<ObjectPoolManager>();
                return _instance;
            }
            set => _instance = value;
        }

        private void Awake()
        {
            if (_instance == null)
                Instance = this;
            else if (_instance != this)
                Destroy(gameObject);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            _instance = null;
        }

        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation,
            Transform parent = null)
        {
            return !prefab ? null : Instance.GetInternal(prefab, position, rotation, parent);
        }

        public static T Spawn<T>(T prefabComponent, Vector3 position, Quaternion rotation, Transform parent = null)
            where T : Component
        {
            if (prefabComponent == null) return null;
            var obj = Spawn(prefabComponent.gameObject, position, rotation, parent);
            return obj ? obj.GetComponent<T>() : null;
        }

        public static void Despawn(GameObject instance)
        {
            if (!instance) return;
            if (Instance)
                Instance.ReturnInternal(instance);
            else
                Destroy(instance);
        }

        private GameObject GetInternal(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            if (!_pools.TryGetValue(prefab, out var pool))
            {
                pool = new Queue<GameObject>();
                _pools[prefab] = pool;
            }

            GameObject obj = null;
            while (pool.Count > 0)
            {
                obj = pool.Dequeue();
                if (obj) break;
            }

            if (!obj)
            {
                obj = Instantiate(prefab, position, rotation, parent ? parent : transform);
            }
            else
            {
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                if (parent) obj.transform.SetParent(parent);
                obj.SetActive(true);
            }

            _instanceToPrefabMap[obj] = prefab;
            return obj;
        }

        private void ReturnInternal(GameObject instance)
        {
            if (!_instanceToPrefabMap.TryGetValue(instance, out var prefab) || prefab == null)
            {
                Destroy(instance);
                return;
            }

            instance.SetActive(false);
            instance.transform.SetParent(transform);

            if (!_pools.TryGetValue(prefab, out var pool))
            {
                pool = new Queue<GameObject>();
                _pools[prefab] = pool;
            }

            pool.Enqueue(instance);
        }

        public void ClearAllPools()
        {
            foreach (var kvp in _pools)
                while (kvp.Value.Count > 0)
                {
                    var obj = kvp.Value.Dequeue();
                    if (obj != null) Destroy(obj);
                }

            _pools.Clear();
            _instanceToPrefabMap.Clear();
        }
    }
}
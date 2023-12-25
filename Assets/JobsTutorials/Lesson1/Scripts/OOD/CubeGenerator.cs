using JobsTutorials.Lesson1.Common;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace JobsTutorials.Lesson1.Scripts.OOD
{
    [RequireComponent(typeof(BoxCollider))]
    public class CubeGenerator : MonoBehaviour
    {
        public GameObject cubeArchetype = null;
        public GameObject targetArea = null;
        [Range(1, 10000)] public int generationTotalNum = 2000;
        [Range(1, 60)] public int generationNumPerTickTime = 10;
        [Range(0.1f, 1.0f)] public float tickTime = 0.2f;
        [HideInInspector] public Vector3 generatorAreaSize;
        [HideInInspector] public Vector3 targetAreaSize;

        // 开启 collectionChecks 后，在外部销毁对象池内部物体将抛出异常
        public bool collectionChecks = true;
        private ObjectPool<GameObject> _pool = null;
        private float _timer = 0.0f;

        private void Start()
        {
            _pool = new ObjectPool<GameObject>(CreatePooledItem,
                OnTakeFromPool, 
                OnReturnedToPool, 
                OnDestroyPoolObject,
                collectionChecks,
                10,
                generationTotalNum);

            generatorAreaSize = GetComponent<BoxCollider>().size;
            if (targetArea != null)
            {
                targetAreaSize = targetArea.GetComponent<BoxCollider>().size;
            }
        }

        private void Update()
        {
            if (_timer >= tickTime)
            {
                GenerateCubes();
                _timer = 0.0f;
            }

            _timer += Time.deltaTime;
        }

        private void OnDestroy()
        {
            _pool.Dispose();
        }

        private void GenerateCubes()
        {
            if (cubeArchetype == null || _pool == null)
            {
                return;
            }

            for (int i = 0; i < generationNumPerTickTime; i++)
            {
                if (_pool.CountAll < generationTotalNum)
                {
                    var cube = _pool.Get();
                    if (cube)
                    {
                        var component = cube.GetComponent<ReturnToPool>();
                        component.pool = _pool;
                        cube.transform.position = GetRandomPos(transform.position, generatorAreaSize);
                        if (targetArea != null)
                        {
                            cube.GetComponent<AutoRotateAndMove>().targetPos =
                                GetRandomPos(targetArea.transform.position, targetAreaSize);
                        }
                    }
                }
                else
                {
                    _timer = 0.0f;
                    return;
                }
            }
        }

        private Vector3 GetRandomPos(Vector3 originPos, Vector3 areaSize)
        {
            return originPos + new Vector3(Random.Range(-areaSize.x * 0.5f, areaSize.x * 0.5f),
                0,
                Random.Range(-areaSize.z * 0.5f, areaSize.z * 0.5f));
        }

        private GameObject CreatePooledItem()
        {
            return Instantiate(cubeArchetype, transform);
        }

        private void OnReturnedToPool(GameObject gameObj)
        {
            gameObj.SetActive(false);
        }

        private void OnTakeFromPool(GameObject gameObj)
        {
            gameObj.SetActive(true);
        }

        private void OnDestroyPoolObject(GameObject gameObj)
        {
            Destroy(gameObj);
        }
    }
}
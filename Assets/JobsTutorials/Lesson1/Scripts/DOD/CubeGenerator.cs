using System;
using JobsTutorials.Lesson1.Scripts.Common;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace JobsTutorials.Lesson1.Scripts.DOD
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
        public float rotateSpeed = 180.0f;
        public float moveSpeed = 5.0f;

        // 开启 collectionChecks 后，在外部销毁对象池内部物体将抛出异常
        public bool collectionChecks = true;
        private ObjectPool<GameObject> _pool = null;
        private float _timer = 0.0f;
        
        private TransformAccessArray _transformAccessArray;
        private NativeArray<Vector3> _randomTargetPosArray;
        private Transform[] _transforms;

        private static readonly ProfilerMarker profilerMarker = new ProfilerMarker("CubesMarchWithJob");

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
            targetAreaSize = targetArea.GetComponent<BoxCollider>().size;

            // 先构建好所有的物体然后填充 Transform
            _randomTargetPosArray = new NativeArray<Vector3>(generationTotalNum, Allocator.Persistent);
            _transforms = new Transform[generationTotalNum];

            for (int i = 0; i < generationTotalNum; i++)
            {
                var cube = _pool.Get();
                var component = cube.AddComponent<AutoReturnToPool>();

                component.pool = _pool;
                var randomGenerationPos = GetRandomPos(transform.position, generatorAreaSize);
                cube.transform.position = randomGenerationPos;
                component.generationPos = randomGenerationPos;
                _transforms[i] = cube.transform;

                var randomTargetPos = GetRandomPos(targetArea.transform.position, targetAreaSize);
                _randomTargetPosArray[i] = randomTargetPos;
                component.targetPos = randomTargetPos;
            }

            _transformAccessArray = new TransformAccessArray(_transforms);
            // 将创建的物体又存入对象池
            for (int i = generationTotalNum - 1; i >= 0; i--)
            {
                _pool.Release(_transforms[i].gameObject);
            }
        }

        private void Update()
        {
            using (profilerMarker.Auto())
            {
                var autoRotateAndMoveJob = new AutoRotateAndMoveJob
                {
                    deltaTime = Time.deltaTime,
                    moveSpeed = moveSpeed,
                    rotateSpeed = rotateSpeed,
                    randomTargetPosArray = _randomTargetPosArray
                };

                var autoRotateAndMoveJobHandle = autoRotateAndMoveJob.Schedule(_transformAccessArray);
                autoRotateAndMoveJobHandle.Complete();

                if (_timer >= tickTime)
                {
                    GenerateCubes();
                    _timer = 0.0f;
                }

                _timer += Time.deltaTime;
            }
        }

        private void OnDestroy()
        {
            if (_transformAccessArray.isCreated)
            {
                _transformAccessArray.Dispose();
            }

            _randomTargetPosArray.Dispose();
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
                // 此处生成其实是拿出来，因此使用 CountActive
                if (_pool.CountActive < generationTotalNum)
                {
                    _pool.Get();
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
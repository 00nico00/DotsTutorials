using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Jobs;

namespace JobsTutorials.Lesson0.Scripts.DOD
{
    [BurstCompile]
    struct WaveJobs : IJobParallelForTransform
    {
        [ReadOnly] public float elapsedTime;
        
        public void Execute(int index, TransformAccess transform)
        {
            var distance = Vector3.Distance(transform.position, Vector3.zero);
            transform.position += Vector3.up * Mathf.Sin(elapsedTime * 3f + distance * 0.2f);
        }
    }
    
    
    public class WaveCubesWithJobs : MonoBehaviour
    {
        public GameObject cubeArchetype;
        [Range(1, 100)] public int xHalfCount = 40;
        [Range(1, 100)] public int zHalfCount = 40;
        private TransformAccessArray _transformAccessArray;
        
        private static readonly ProfilerMarker<int> profilerMarker =
            new ProfilerMarker<int>("WaveCubes UpdateTransform", "Objects Count");

        private void Start()
        {
            _transformAccessArray = new TransformAccessArray(4 * xHalfCount * zHalfCount);
            for (var x = -xHalfCount; x <= xHalfCount; x++)
            {
                for (var z = -zHalfCount; z <= zHalfCount; z++)
                {
                    var cube = Instantiate(cubeArchetype);
                    cube.transform.position = new Vector3(x * 1.1f, 0, z * 1.1f);
                    _transformAccessArray.Add(cube.transform);
                }
            }
        }

        private void Update()
        {
            using (profilerMarker.Auto(_transformAccessArray.length))
            {
                var job = new WaveJobs
                {
                    elapsedTime = Time.time
                };
                var waveCubesJobHandle = job.Schedule(_transformAccessArray);
                waveCubesJobHandle.Complete();
            }
        }

        private void OnDestroy()
        {
            _transformAccessArray.Dispose();
        }
    }
}

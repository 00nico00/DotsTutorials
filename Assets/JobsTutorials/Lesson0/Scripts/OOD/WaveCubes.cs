using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

namespace JobsTutorials.Lesson0.Scripts.OOD
{
    public class WaveCubes : MonoBehaviour
    {
        public GameObject cubeArchetype;
        [Range(1, 100)] public int xHalfCount = 40;
        [Range(1, 100)] public int zHalfCount = 40;
        private List<Transform> _cubesList;

        private static readonly ProfilerMarker<int> profilerMarker =
            new ProfilerMarker<int>("WaveCubes UpdateTransform", "Objects Count");

        private void Start()
        {
            _cubesList = new List<Transform>();
            for (var x = -xHalfCount; x <= xHalfCount; x++)
            {
                for (var z = -zHalfCount; z <= zHalfCount; z++)
                {
                    var cube = Instantiate(cubeArchetype);
                    cube.transform.position = new Vector3(x * 1.1f, 0, z * 1.1f);
                    _cubesList.Add(cube.transform);
                }
            }
        }

        private void Update()
        {
            using (profilerMarker.Auto(_cubesList.Count))
            {
                for (var i = 0; i < _cubesList.Count; i++)
                {
                    var distance = Vector3.Distance(_cubesList[i].position, Vector3.zero);
                    _cubesList[i].localPosition += Vector3.up * Mathf.Sin(Time.time * 3f + distance * 0.2f);
                }
            }
        }
    }
}

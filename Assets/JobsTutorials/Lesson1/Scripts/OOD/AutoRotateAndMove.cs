using System;
using JobsTutorials.Lesson1.Common;
using Unity.Profiling;
using UnityEngine;

namespace JobsTutorials.Lesson1.Scripts.OOD
{
    [RequireComponent(typeof(ReturnToPool))]
    public class AutoRotateAndMove : MonoBehaviour
    {
        private const float Epsilon = 0.05f;
        public float rotateSpeed = 180.0f;
        public float moveSpeed = 5.0f;
        public Vector3 targetPos;

        private static readonly ProfilerMarker profilerMarker = new ProfilerMarker("CubeMarch");

        private void Update()
        {
            using (profilerMarker.Auto())
            {
                transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
                var dist = targetPos - transform.position;
                if (dist.magnitude >= Epsilon)
                {
                    var moveDir = dist.normalized;
                    transform.position += moveDir * (moveSpeed * Time.deltaTime);
                }
                else
                {
                    var component = GetComponent<ReturnToPool>();
                    if (component)
                    {
                        component.OnDisappear();
                    }
                }
            }
        }
    }
}

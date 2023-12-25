using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace JobsTutorials.Lesson1.Scripts.DOD
{
    public struct AutoRotateAndMoveJob : IJobParallelForTransform
    {
        public float deltaTime;
        public float rotateSpeed;
        public float moveSpeed;
        public NativeArray<Vector3> randomTargetPosArray;
        
        public void Execute(int index, TransformAccess transform)
        {
            var moveDir = (randomTargetPosArray[index] - transform.position).normalized;
            transform.position += moveDir * moveSpeed * deltaTime;
            var localEulerAngles = transform.localRotation.eulerAngles;
            localEulerAngles.y += rotateSpeed * deltaTime;
            transform.localRotation = Quaternion.Euler(localEulerAngles);
        }
    }
}

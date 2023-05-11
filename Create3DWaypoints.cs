using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Jobs;

public class Create3DWaypoints : MonoBehaviour
{
    public Transform lookAtTarget;

    [SerializeField] private Waypoints wp;
    [SerializeField] private Transform arrowPrefab;
    [SerializeField] private float distanceBetweenArrow = 1;
    [SerializeField] private float arrowSpeed = 2;

    private List<Transform> arrowList = new List<Transform>();
    private List<int> pathIndexList = new List<int>();

    private TransformAccessArray accessArray;
    private NativeArray<int> pathIndexs;
    private NativeArray<Vector3> paths;

    public struct MoveArrowJob : IJobParallelForTransform
    {
        public float velocity;
        [NativeDisableParallelForRestriction]
        public NativeArray<Vector3> paths;
        public NativeArray<int> pathIndexs;
        public float deltaTime;

        public Vector3 lookedTargetPos;

        public void Execute(int index, TransformAccess transform)
        {
            if (Vector3.Dot(transform.position - paths[pathIndexs[index]], transform.position - paths[pathIndexs[index] + 1]) >= 0)
            {
                transform.position = paths[pathIndexs[index]];
            }
            Vector3 dir = (paths[pathIndexs[index]+1] - paths[pathIndexs[index]]).normalized;
            transform.position += dir * velocity * deltaTime;

            var up = transform.rotation * Vector3.up;
            var angle = Vector3.Angle(up, lookedTargetPos - transform.position);
            transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y,angle));
            //transform.rotation = RotateCustom(transform, Vector3.forward, angle);
        }

        public Quaternion RotateCustom(TransformAccess transform, Vector3 axis, float angle, Space relativeTo = Space.Self)
        {
            Quaternion rotation = Quaternion.identity;

            if (relativeTo == Space.Self)
            {
                rotation = Quaternion.AngleAxis(angle, axis);
            }
            else if (relativeTo == Space.World)
            {
                rotation = Quaternion.AngleAxis(angle, InverseTransformDirectionCustom(transform,axis));
            }

            return rotation * transform.rotation;
        }

        public Vector3 InverseTransformDirectionCustom(TransformAccess transform, Vector3 direction)
        {
            Quaternion inverseRotation = Quaternion.Inverse(transform.rotation);
            return inverseRotation * direction;
        }
    }
    private void Start()
    {
        for (int i = 0; i < wp.waypoints.Length - 1; i++)
        {
            Vector3 start = wp.waypoints[i];
            Vector3 end = wp.waypoints[i + 1];
            float dis = Vector3.Distance(start, end);
            int numPoints = Mathf.FloorToInt(dis / distanceBetweenArrow) + 1;
            for (int j = 0; j < numPoints; j++)
            {
                Transform t = Instantiate(arrowPrefab, transform);
                t.position = Vector3.Lerp(start, end, j * distanceBetweenArrow / dis);
                t.rotation = Quaternion.RotateTowards(t.rotation, Quaternion.LookRotation(end - start), 360);
                t.gameObject.SetActive(true);   
                arrowList.Add(t);
                pathIndexList.Add(i);
            }
        }
        accessArray = new TransformAccessArray(arrowList.ToArray());
        pathIndexs = new NativeArray<int>(pathIndexList.ToArray(), Allocator.Persistent);
        paths = new NativeArray<Vector3>(wp.waypoints, Allocator.Persistent);

    }

    private void Update()
    {
        var job = new MoveArrowJob()
        {
            velocity = arrowSpeed,
            paths = paths,
            pathIndexs = pathIndexs,
            deltaTime = Time.deltaTime,

            lookedTargetPos = lookAtTarget.position,
        };

        job.Schedule(accessArray).Complete();
    }
    private void OnDestroy()
    {
        accessArray.Dispose();
        pathIndexs.Dispose();
        paths.Dispose();
    }
}



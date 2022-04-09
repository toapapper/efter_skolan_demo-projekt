using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

public class AIManager : MonoBehaviour
{
    [SerializeField] private bool useJobs;
    private List<AI> aiList_1, aiList_2, aiList_3;
    private List<List<AI>> listCollection;
    [SerializeField] AI pfAI_1;

    private void Start()
    {
        aiList_1 = new List<AI>();
        aiList_2 = new List<AI>();
        aiList_3 = new List<AI>();
        listCollection = new List<List<AI>> { aiList_1, aiList_2, aiList_3 };

        for (int i = 0; i < 1000; i++)
        {
            AI aiGo = Instantiate(pfAI_1, new Vector3(UnityEngine.Random.Range(-8f, 8f), UnityEngine.Random.Range(-5f, 5f)), Quaternion.identity);
            aiGo.moveX = UnityEngine.Random.Range(1f, 2f);
            aiGo.moveY = UnityEngine.Random.Range(1f, 2f);
            //Transform aiTransform = Instantiate(pfAI, new Vector3(UnityEngine.Random.Range(-8f, 8f), UnityEngine.Random.Range(-5f, 5f)), Quaternion.identity);
            aiList_1.Add(aiGo);
        }
    }

    private void Update()
    {
        if (useJobs)
        {
            //NativeArray<float3> positionArray = new NativeArray<float3>(zombieList.Count, Allocator.TempJob);
            NativeArray<float> moveXArray = new NativeArray<float>(aiList_1.Count, Allocator.TempJob);
            NativeArray<float> moveYArray = new NativeArray<float>(aiList_1.Count, Allocator.TempJob);
            TransformAccessArray transformAccessArray = new TransformAccessArray(aiList_1.Count);

            foreach (List<AI> aiList in listCollection)
            {
                for (int i = 0; i < aiList.Count; i++)
                {
                    moveXArray[i] = aiList[i].moveX;
                    moveYArray[i] = aiList[i].moveY;
                    transformAccessArray.Add(aiList[i].transform);
                }
            }


            /*
            ReallyToughParallelJob reallyToughParallelJob = new ReallyToughParallelJob {
                deltaTime = Time.deltaTime,
                positionArray = positionArray,
                moveYArray = moveYArray,
            };

            JobHandle jobHandle = reallyToughParallelJob.Schedule(zombieList.Count, 100);
            jobHandle.Complete();
            */
            AIParallelMoveJob aIParallelMoveJob = new AIParallelMoveJob
            {
                deltaTime = Time.deltaTime,
                moveXArray = moveXArray,
                moveYArray = moveYArray,
            };

            JobHandle jobHandle = aIParallelMoveJob.Schedule(transformAccessArray);
            jobHandle.Complete();
            foreach (List<AI> aiList in listCollection)
            {
                for (int i = 0; i < aiList.Count; i++)
                {
                    //zombieList[i].transform.position = positionArray[i];
                    aiList[i].moveX = moveXArray[i];
                    aiList[i].moveY = moveYArray[i];
                }
            }


            //positionArray.Dispose();
            moveXArray.Dispose();
            moveYArray.Dispose();
            transformAccessArray.Dispose();
        }
        else
        {
            foreach (List<AI> aiList in listCollection)
            {
                foreach (AI ai_obj in aiList)
                {
                    ai_obj.transform.position += new Vector3(ai_obj.moveX * Time.deltaTime, ai_obj.moveY * Time.deltaTime);
                    if (ai_obj.transform.position.y > 5f)
                    {
                        ai_obj.moveY = -math.abs(ai_obj.moveY);
                    }
                    if (ai_obj.transform.position.y < -5f)
                    {
                        ai_obj.moveY = +math.abs(ai_obj.moveY);
                    }
                    if (ai_obj.transform.position.x < -10f)
                    {
                        ai_obj.moveX = -math.abs(ai_obj.moveX);
                    }
                    if (ai_obj.transform.position.x > 10f)
                    {
                        ai_obj.moveX = -math.abs(ai_obj.moveX);
                    }
                    float value = 0f;
                    for (int i = 0; i < 1000; i++)
                    {
                        value = math.exp10(math.sqrt(value));
                    }
                }
            }
        }
            
        /*
        if (useJobs) {
            NativeList<JobHandle> jobHandleList = new NativeList<JobHandle>(Allocator.Temp);
            for (int i = 0; i < 10; i++) {
                JobHandle jobHandle = ReallyToughTaskJob();
                jobHandleList.Add(jobHandle);
            }
            JobHandle.CompleteAll(jobHandleList);
            jobHandleList.Dispose();
        } else {
            for (int i = 0; i < 10; i++) {
                ReallyToughTask();
            }
        }
        */

        //Debug.Log(((Time.realtimeSinceStartup - startTime) * 1000f) + "ms");
    }
}

[BurstCompile]
public struct AIParallelMoveJob : IJobParallelForTransform
{

    public NativeArray<float> moveXArray;
    public NativeArray<float> moveYArray;
    [ReadOnly] public float deltaTime;

    public void Execute(int index, TransformAccess transform)
    {
        transform.position += new Vector3(moveXArray[index] * deltaTime, moveYArray[index] * deltaTime, 0f);
        if (transform.position.y > 5f)
        {
            moveYArray[index] = -math.abs(moveYArray[index]);
        }
        if (transform.position.y < -5f)
        {
            moveYArray[index] = +math.abs(moveYArray[index]);
        }
        if (transform.position.x > 10f)
        {
            moveXArray[index] = -math.abs(moveXArray[index]);
        }
        if (transform.position.x < -10f)
        {
            moveXArray[index] = +math.abs(moveXArray[index]);
        }

        //Just to consume time
        float value = 0f;
        for (int i = 0; i < 1000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }

}


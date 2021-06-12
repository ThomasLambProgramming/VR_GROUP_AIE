﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.Collections;

public class HammerCollisionEnemy : MonoBehaviour
{

    public NodeContainer pathData = null;
    public float yForceIncrease = 4f;
    public float meleeHitForce = 200f;
    public float throwHitForce = 10f;

    private Vector3 previousPosition;
    //give it a default so a null does not have to be done
    private Vector3 bufferPosition = new Vector3(0, 0, 0);
    public GameObject hammerPart;
    public int frameSkipsOnUpdate = 2;
    private int amountSkipped = 0;
    public GameObject playerRightHand = null;

    public float withInGrabDistance = 1f;
    public float goNextNodeDist = 2f;
    public float returnspeed = 10f;
    private int currentPathIndex = 0;
    private bool isDonePath = false;
    private bool isBeingSummoned = false;
    private Vector3[] path;
    public float maxReturnSpeed = 50f;
    XRGrabInteractable grabScript = null;
    private Rigidbody hammerRb = null;
    public void Start()
    {
        hammerRb = GetComponent<Rigidbody>();
        grabScript = GetComponent<XRGrabInteractable>();
    }
    public void FixedUpdate()
    {
        amountSkipped++;
        if (amountSkipped >= frameSkipsOnUpdate)
        {
            //this is done so there is a delay 
            previousPosition = bufferPosition;
            bufferPosition = hammerPart.transform.position;
            amountSkipped = 0;
        }

    }
    //recalling script
    public void IsSummoned()
    {
        isBeingSummoned = true;
        path = FindPath();
        currentPathIndex = 0;
        hammerRb.useGravity = false;
        isDonePath = false;
    }
    public void StopSummon()
    {
        isBeingSummoned = false;
        path = null;
        currentPathIndex = 0;
        hammerRb.useGravity = true;
        isDonePath = true;
    }
    
    public void Update()
    {
        //hammerRb.velocity = new Vector3(0,0,0);
        if (isBeingSummoned)
        {
            var directionToHand = (playerRightHand.transform.position - transform.position).normalized;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, directionToHand, out hit))
            {
                if (hit.transform.CompareTag("Obstacle"))
                {
                    path = FindPath();
                    currentPathIndex = 0;
                }
                else
                {
                    hammerRb.velocity += directionToHand * maxReturnSpeed;
                    isDonePath = true;
                    path = null;
                }
            }
            //the grab interactor will then be able to grab it so we can stop the summoning
            if (Vector3.Magnitude(playerRightHand.transform.position - transform.position) < withInGrabDistance)
            {
                StopSummon();
                hammerRb.velocity = Vector3.zero;
            }
            else 
            {
                if (path != null)
                {
                    Vector3 heading = transform.position + hammerRb.velocity;
                    float headingLength = heading.magnitude;
                    Vector3 directionToTarget = Vector3.Normalize(path[currentPathIndex] - transform.position);
                    Vector3 vector2Target = directionToTarget * headingLength;
                    Vector3 targetForcePos = vector2Target + transform.position;
                    Vector3 forceDirection = targetForcePos - heading;
                    forceDirection = forceDirection.normalized;
                    forceDirection = forceDirection * returnspeed * Time.deltaTime;
                    hammerRb.velocity += forceDirection;
                    
                    if ((path[currentPathIndex] - transform.position).magnitude < goNextNodeDist)
                    {
                        if (currentPathIndex < path.Length - 1)
                        {
                            currentPathIndex++;
                        }
                    }
                }
            }

            if (isDonePath)
            {
                //this is just to stop the error in unity im going to fix this but just annoying me when trying 
                //to set up the scene
            }
            if (hammerRb.velocity.magnitude > maxReturnSpeed)
            {
                hammerRb.velocity = hammerRb.velocity.normalized * maxReturnSpeed;
            }
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("Enemy"))
        {
            Vector3 hammerDifference = hammerPart.transform.position - previousPosition;
            float amountMoved = hammerRb.velocity.magnitude;
            Vector3 forceDirection = hammerDifference.normalized;
            forceDirection.y += yForceIncrease;

            Vector3 forceToAdd = forceDirection * amountMoved;


            if (grabScript.isSelected)
            {
                forceToAdd *= meleeHitForce;
                collision.transform.GetComponent<WalkerEnemy>().HasBeenHit(forceToAdd);
            }
            else
            {
                forceToAdd *= throwHitForce;
                collision.transform.GetComponent<WalkerEnemy>().HasBeenHit(forceToAdd);
            }
        }
        else if (collision.transform.CompareTag("Crate"))
        {
            collision.transform.GetComponent<CrateDestory>().HammerHit();
        }
        else if (collision.transform.CompareTag("BalloonBody"))
        {
            collision.transform.parent.GetComponent<BalloonEnemy>().MainBodyHit();
        }
        else if (collision.transform.CompareTag("Balloon"))
        {
            collision.transform.parent.GetComponent<BalloonEnemy>().BalloonHit();
        }
        else if (collision.transform.CompareTag("Prankster"))
        {
            Vector3 hammerDifference = hammerPart.transform.position - previousPosition;
            float amountMoved = hammerRb.velocity.magnitude;
            Vector3 forceDirection = hammerDifference.normalized;
            forceDirection.y += yForceIncrease;

            Vector3 forceToAdd = forceDirection * amountMoved;



            if (grabScript.isSelected)
            {
                forceToAdd *= meleeHitForce;
                collision.transform.GetComponent<Pranksters>().HasBeenHit(forceToAdd);
            }
            else
            {
                forceToAdd *= throwHitForce;
                collision.transform.GetComponent<Pranksters>().HasBeenHit(forceToAdd);
            }
        }
    }
    private Vector3[] FindPath()
    {
        if (pathData.NodeGraph == null)
        {
            Debug.LogWarning("There is no node graph! Please create one from the node window" +
                             " Window/NodeGraph.");
            return null;
        }

        Vector3[] path;
        PathFindJob pathfind = new PathFindJob();
        pathfind.NodeData = pathData.NodeGraph;

        int[] StartEndIndex = { 0, 0 };

        StartEndIndex[0] = FindClosestNode(transform.position);
        StartEndIndex[1] = FindClosestNode(playerRightHand.transform.position);

        NativeArray<int> startEndPos = new NativeArray<int>(StartEndIndex, Allocator.Temp);
        pathfind.startEndPos = startEndPos;

        //test for path finding time will need to improve the memory grabbing though its a bit slow and can be grouped
        //float time = Time.realtimeSinceStartup;
        pathfind.Execute();
        //Debug.Log(Time.realtimeSinceStartup - time);
        if (!pathfind.pathResult.IsCreated)
        {
            if (startEndPos.IsCreated)
                startEndPos.Dispose();
            Debug.Log("Pathresult was null");
            return null;
        }

        path = pathfind.pathResult.ToArray();
        pathfind.pathResult.Dispose();
        startEndPos.Dispose();

        //the path that the main loop was giving back was in the reverse order
        Vector3[] reversedPath = new Vector3[path.Length];
        for (int i = 0; i < path.Length; i++)
        {
            reversedPath[i] = path[path.Length - (1 + i)];
        }

        return reversedPath;
    }

    public int FindClosestNode(Vector3 a_position)
    {
        if (pathData.NodeGraph == null)
            return -1;

        int closestNode = -1;
        float distance = 0;
        bool firstNode = true;

        for (int i = 0; i < pathData.NodeGraph.Length; i++)
        {
            if (firstNode)
            {
                distance = Vector3.Magnitude(pathData.NodeGraph[i].m_position - a_position);
                firstNode = false;
                closestNode = 0;
                continue;
            }
            float nodeDist = Vector3.Magnitude(pathData.NodeGraph[i].m_position - a_position);
            if (nodeDist < distance)
            {
                distance = nodeDist;
                closestNode = i;
            }
        }

        return closestNode;
    }
}

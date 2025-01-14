﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
/*
* File: WalkerEnemy.cs
*
* Author: Thomas Lamb (s200498@students.aie.edu.au)
* Date Created: 4th June 2021
* Date Last Modified: 12th June 2021
*
* Walker ai class for pathing around with navmesh 
* and what it needs to do when it gets hit
*/
public class WalkerEnemy : MonoBehaviour
{
    public AudioManager audioSources = null;
    private Rigidbody rb = null;
    private NavMeshAgent navmesh = null;
    public bool makePath = true;
    private bool isDestroyed = false;
    public float distanceToNewPath = 2f;
    public GameObject headObject = null;
    private Rigidbody headRb = null;
    public bool kill = false;
    
    //seperate colliders so when the walker is hit it doesnt instantly explode
    private BoxCollider mainCollider = null;
    private BoxCollider headCollider = null;

    //public ParticleSystem onDeathParticle;

    public NodeContainer nodeData = null;
    // Start is called before the first frame update
    void Start()
    {
        mainCollider = GetComponent<BoxCollider>();
        headCollider = headObject.GetComponent<BoxCollider>();
        rb = GetComponent<Rigidbody>();
        navmesh = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        //if all this is true then we want the walker to move
        if (navmesh != null && navmesh.enabled && makePath)
        {
            //if the distance to the destination is lower then distanceToNewPath then we find a new random position
            //in the node container
            if (Vector3.Magnitude(navmesh.destination - transform.position) < distanceToNewPath * distanceToNewPath)
                navmesh.SetDestination(nodeData.NodeGraph[Random.Range(0, nodeData.NodeGraph.Length - 1)].m_position);
        }
        //For some unknown reason in level 4 and 5 the rigidbody was infinitely gaining gravity on the y
        //so when it was hit it would instantly fall through the ground
        rb.velocity = Vector3.zero;
        
        //run the onhit function
        if (kill)
        {
            HasBeenHit(new Vector3(0, 100, 0));
            kill = false;
        }

    }
    public void HasBeenHit(Vector3 forceToHit)
    {
        if (!isDestroyed)
        {
            FindObjectOfType<GameManager>().AddTime(gameObject, 3);
            isDestroyed = true;
        }

        //there isnt a hit sound soooo.....
        //audioSources.PlaySound("Hit", transform.position);
        
        //aaron edits
        //onDeathParticle.Play();
        //edits end
        
        //stop it from moving and enable the proper head collider
        navmesh.enabled = false;
        mainCollider.enabled = false;
        headCollider.enabled = true;
        
        //deparent it so it doesnt move with the object ontop of it flying in the air
        //headObject.transform.parent = null;

        //The reason this has to be done is because the hammer can sometimes hit twice on a fall or melee
        //so a check has to be done before adding just in case
        //(the head doesnt have a rigidbody because it will overlap with the rigidbody on the main body so adding on hit was easier because at most
        //it would be 2-3 enemies getting hit at once (and thats unlikely to begin with)
        headRb = headObject.GetComponent<Rigidbody>();
        if (headRb == null)
            headRb = headObject.AddComponent<Rigidbody>();
        headRb.velocity = Vector3.zero;
        headRb.AddForce(forceToHit);

        Destroy(gameObject, 4f);
        Destroy(headObject, 4f);
    }
}

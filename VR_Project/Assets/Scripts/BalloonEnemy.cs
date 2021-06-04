﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BalloonEnemy : MonoBehaviour
{
    public float bounceAmount = 0.6f;
    public float bounceSpeed = 0.2f;
    private float yOffset = 0f;
    bool move = true;
    private float delay = 0f;
    /*
     * TO ADD
     * Particle effect for string snap, balloon pop and enemy hit
     */

    void Start()
    {
        delay = Random.Range(-1.0f, 1.0f);
        yOffset = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (move)
        transform.position = new Vector3(
            transform.position.x,
            yOffset + (Mathf.Sin(Time.time * bounceSpeed) * delay * bounceAmount) ,
            transform.position.z);
    }

    public void OnHit()
    {
        move = false;
        transform.GetChild(0).parent = null;
        //pop particle effect 
        //then destroy this 
        Destroy(gameObject, 0.2f);
    }
}
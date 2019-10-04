﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaccoonController : MonoBehaviour
{
    public AudioClip itemCollectSound;
    [SerializeField] private float speed = 80;
    [SerializeField] private int food = 0;
    [SerializeField] private int maxFood = 10;

    private Rigidbody rb;
    public float jumpVelocity = 50f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Debug.Log(rb);
    }

    

    void FixedUpdate()
    {
        float moveHorizontal = speed * Input.GetAxis("Horizontal");
        float moveVertical = speed * Input.GetAxis("Vertical");
        Vector3 movePlayer = new Vector3(moveHorizontal, 0.0f, moveVertical);
        rb.velocity = movePlayer;
        
        
        // Code from jump script
        if (Input.GetButtonDown("Jump"))
        {
            Debug.Log("jump press");
            rb.velocity = Vector3.up * jumpVelocity;
        }
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.velocity += Vector3.up * Physics.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
        
        
    }

    public int GetFood()
    {
        return food; 
    }

    public int GetMaxFood()
    {
        return maxFood; 
    }

    public bool IncreaseFood()
    {
        if (food >= maxFood)
        {
            Debug.Log("At max food");
            return false;
        }

        Debug.Log("Got a food");
        food++;
        AudioManager.instance.Play("ItemCollect");
        return true;
    }

    public bool DecreaseFood()
    {
        if (food <= 0)
        {
            Debug.Log("Have no food");
            return false;
        }

        Debug.Log("Gave a food");
        food--;
        return true;
    }
}

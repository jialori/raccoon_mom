﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaccoonController : MonoBehaviour
{
  [SerializeField] private AudioClip itemCollectSound;
  [SerializeField] private bool useController = true;
  [SerializeField] private Transform cam;

  private Vector3 movementVector;

  private CharacterController characterController;

  [SerializeField] private float movementSpeed = 10;
  private float jumpPower = 15;
  private float gravity = 40;

  // First add a Layer "Breakable"/"Knockable" to all breakable and knockable objects in Unity Engine 
  private float raycastPaddedDist;
  private float raycastPadding = 0.2f;
  private int radiusStep = 36; // how many degree does each raycast check skips, dcrease if want more accuracy
  private string breakableMaskName = "Breakable";
  private string knockableMaskName = "Knockable";
  private int breakableMask;
  private int knockableMask;
  private float pushPower = 12;

  private bool isOnUpStair = false;
  private bool isOnDownStair = false;

  void Start()
  {
    // AudioManager.instance.Play("ThemeSong");
    characterController = GetComponent<CharacterController>();

    // Raycasy for breakable and knockable objects
    raycastPaddedDist = characterController.radius + raycastPadding;
    breakableMask = 1 << LayerMask.NameToLayer(breakableMaskName);
    knockableMask = 1 << LayerMask.NameToLayer(knockableMaskName);
  }

  void Update()
  {
    // Move up or down stairs
    if ((isOnUpStair || isOnDownStair) && Input.GetButtonDown("B")) 
    {
      characterController.enabled = false;
      if (isOnUpStair) characterController.transform.position += new Vector3(0, 8.5f, 0);
      if (isOnDownStair) characterController.transform.position -= new Vector3(0, 8, 0);
      characterController.enabled = true;
      return;
    }

    // Adjust movement for camera angle
    var camForward = cam.forward;
    var camRight = cam.right;
    camForward.y = 0;
    camRight.y = 0;
    camForward = camForward.normalized;
    camRight = camRight.normalized;
    var prevY = movementVector.y;

    // movement
    movementVector = (camForward * GetYAxis() + camRight * GetXAxis()) * movementSpeed;

    // Jump
    if (characterController.isGrounded)
    {
      movementVector.y = 0;

      if (GetJump())
      {
        movementVector.y = jumpPower;
      }

    }
    else
    {
      movementVector.y = prevY;
    }

    movementVector.y -= gravity * Time.deltaTime;
    // Debug.Log("movementVector = " + movementVector);
    characterController.Move(movementVector * Time.deltaTime);

    // Breakable objects
    RaycastHit hit;
    if (Input.GetButtonDown("B"))
    {
      // Bottom of controller. Slightly above ground so it doesn't bump into slanted platforms.
      Vector3 p1 = transform.position + Vector3.up * 0.01f;
      Vector3 p2 = p1 + Vector3.up * characterController.height;
      // Check around the character in 360 degree
      for (int i = 0; i < 360; i += radiusStep)
      {
        // Check if anything with the platform layer touches this object
        if (Physics.CapsuleCast(p1, p2, 0, new Vector3(Mathf.Cos(i), 0, Mathf.Sin(i)), out hit, raycastPaddedDist, breakableMask))
        {
          Breakable breakable = hit.collider.gameObject.GetComponent<Breakable>() as Breakable;
          if (breakable != null)
          {
            breakable.trigger();
          }
        }
      }
    }

    //[Optional] Check the players feet and push them up if something clips through their feet.
    //(Useful for vertical moving platforms)
    // if (Physics.Raycast(transform.position+Vector3.up, -Vector3.up, out hit, 1, knockableMask)){
    //     characterController.Move(Vector3.up * (1-hit.distance));
    // }

  }

  private float GetXAxis()
  {
    if (useController)
    {
      return Input.GetAxis("LeftJoystickX");
    }
    else
    {
      return Input.GetAxis("Horizontal");
    }
  }

  private float GetYAxis()
  {
    if (useController)
    {
      return -Input.GetAxis("LeftJoystickY");
    }
    else
    {
      return Input.GetAxis("Vertical");
    }
  }

  private bool GetJump()
  {
    if (useController)
    {
      return Input.GetButtonDown("A");
    }
    else
    {
      return Input.GetKeyDown(KeyCode.Space);
    }
  }

  // Hit an object over
  void OnControllerColliderHit(ControllerColliderHit hit)
  {
    this.isOnUpStair = false;
    this.isOnDownStair = false;
    Stair stair = hit.gameObject.GetComponent("Stair") as Stair;
    if (stair != null)
    {
      // Debug.Log("stair hit");
      if (hit.gameObject.tag == "UpStair") this.isOnUpStair = true;
      if (hit.gameObject.tag == "DownStair") this.isOnDownStair = true;
    }

    Rigidbody body = hit.collider.attachedRigidbody;

    // no rigidbody
    if (body == null || body.isKinematic)
    {
      return;
    }

    // Knock over a knock-over-able objects
    Knockable knockable = hit.gameObject.GetComponent("Knockable") as Knockable;
    if (knockable != null)
    {
      Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
      Vector3 pushForce = pushDir * pushPower;
      knockable.trigger(pushForce);
    }    
  }

}
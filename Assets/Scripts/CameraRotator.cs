﻿using UnityEngine;
using Util;
using System;

public class CameraRotator : MonoBehaviour
{

    // Constants for clamping camera angles and establishing boundaries for rotation
    private const float Y_ANGLE_MIN = 0.0f;
    private const float Y_ANGLE_MAX = 50.0f;

    // Initialization of variables
    // public Transform camTransform;

    [SerializeField] private float distance = 8.0f;
    private float currentX = 0.0f;
    private float currentY = 10f;

    [SerializeField] private Transform target = null;

    [System.Serializable]
    public class PosSettings
    {
        public Vector3 targetLookAtOffset = new Vector3(0, 2f, 0);
        public float lookSmooth = 50f;
        public float disFromTar = -8;
        public float smooth = 0.05f;
        [HideInInspector]
        public float adjustDis = -8;
    }

    public class OrbitSettings
    {
        public float xRotation = -20;
        public float yRotation = -180;
        public float maxXRotation = 30;
        public float minXRotation = -55;
        //public float maxYRotation = -90;
        //public float minYRotation = -270;
        // How fast the rotation can take place
        public float vOrbitSmooth = 50;
        public float hOrbitSmooth = 100;
    }

    public class InputSettings
    {
        public string ORBIT_HORIZONTAL_SNAP = "Cancel";
        public string ORBIT_HORIZONTAL = "Mouse X";
        public string ORBIT_VERTICAL = "Mouse Y";
        public string ZOOM = "Mouse ScrollWheel";

    }

    [System.Serializable]
    public class DebugSettings
    {
        public bool drawDesiredCollisionLines = true;
        public bool drawAdjustedCollisionLines = true;
    }

    public PosSettings position = new PosSettings();
    public OrbitSettings orbit = new OrbitSettings();
    public InputSettings input = new InputSettings();
    public DebugSettings debug = new DebugSettings();
    public CollisionHandler coll = new CollisionHandler();

    // target.position + targetLookAtOffset
    Vector3 lookAtPtPos = Vector3.zero;
    Vector3 des = Vector3.zero;
    Vector3 adjustedDes = Vector3.zero;
    Vector3 camVel = Vector3.zero;
    RaccoonController player;
    float vOrbitInp, hOrbitInp, hOrbitSnapInp;

    private void Start()
    {
        // target = GameManager.instance.Raccoon.gameObject.transform;
        MoveToTar();
        coll.Initialize(Camera.main);
        coll.UpdateCamClipPts(transform.position, transform.rotation, ref coll.adjustedCamClipPts);
        coll.UpdateCamClipPts(des, transform.rotation, ref coll.desiredCamClipPts);
    }

    private void Update()
    {
        if (!SceneTransitionManager.instance.isGameOn()) {return;}

        GetInput();
        MoveToTar();
        LookAtTar();
        OrbitTar();
        //RenderTransparent();

        coll.UpdateCamClipPts(transform.position, transform.rotation, ref coll.adjustedCamClipPts);
        coll.UpdateCamClipPts(des, transform.rotation, ref coll.desiredCamClipPts);

        // draw debug lines
        for (int i = 0; i < 5; i++)
        {
            if (debug.drawDesiredCollisionLines)
                Debug.DrawLine(lookAtPtPos, coll.desiredCamClipPts[i], Color.white);
            if (debug.drawAdjustedCollisionLines)
                Debug.DrawLine(lookAtPtPos, coll.adjustedCamClipPts[i], Color.green);
        }

        coll.CheckColliding(lookAtPtPos); // using raycasts
        position.adjustDis = coll.AdjustedDisWithRaycast(lookAtPtPos);
    }

    /*
    // Render objects between the raccoon and camera transparent
    void RenderTransparent()
    {
        // Retrieve all objects between raccoon and camera
        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, transform.forward, distance);
        
        // For each object hit by the raycast
        for (int i = 0; i < hits.Length; i++)
        {
            // Retrieve the object's renderer
            RaycastHit hit = hits[i];
            Renderer rend = hit.transform.GetComponent<Renderer>();
            Debug.Log("Object " + hit.transform.ToString() + " was hit by raycast between raccoon and camera");
            Debug.Log("Rend: " + Convert.ToBoolean(rend).ToString() + ", object is raccoon: " + (hit.transform != target).ToString());

            if (rend && hit.transform != target)
            {
                // Make object transparent here

                // Need some way to restore the object's shader when it is no longer blocking the camera's view
            }
        }
    }
    */

    void GetInput()
    {
        vOrbitInp = -Controller.GetCamYAxis();
        // hOrbitInp = Input.GetAxis(input.ORBIT_HORIZONTAL);
        hOrbitInp = -Controller.GetCamXAxis();
        // hOrbitSnapInp = Input.GetAxis(input.ORBIT_HORIZONTAL_SNAP);
    }

    void MoveToTar()
    {
        //lookAtPtPos = target.position + position.targetLookAtOffset;
        lookAtPtPos = target.position + coll.AdjustLookAtPos(target.position, position.targetLookAtOffset);
        // des = Quaternion.Euler(orbit.xRotation, orbit.yRotation + target.eulerAngles.y, 0) * -Vector3.forward * position.disFromTar;
        des = Quaternion.Euler(orbit.xRotation, orbit.yRotation, 0) * -Vector3.forward * position.disFromTar;
        des += lookAtPtPos;

        if (coll.isColliding)
        {
            // adjustedDes = Quaternion.Euler(orbit.xRotation, orbit.yRotation + target.eulerAngles.y, 0) * Vector3.forward * position.adjustDis;
            adjustedDes = Quaternion.Euler(orbit.xRotation, orbit.yRotation, 0) * Vector3.forward * position.adjustDis;
            adjustedDes += lookAtPtPos;

            // Smooth camera movement
            transform.position = Vector3.SmoothDamp(transform.position, adjustedDes, ref camVel, position.smooth);

        }
        else
            transform.position = Vector3.SmoothDamp(transform.position, des, ref camVel, position.smooth);
    }

    void LookAtTar()
    {
        Quaternion tarRotation = Quaternion.LookRotation(lookAtPtPos - transform.position);
        transform.rotation = Quaternion.Lerp(transform.rotation, tarRotation, position.lookSmooth * Time.deltaTime);
    }

    void OrbitTar()
    {
        if (hOrbitSnapInp > 0)
        {
            orbit.yRotation = -180;
        }
        //?
        orbit.xRotation += -vOrbitInp * orbit.vOrbitSmooth * Time.deltaTime;
        orbit.yRotation += -hOrbitInp * orbit.hOrbitSmooth * Time.deltaTime;

        orbit.xRotation = Mathf.Clamp(orbit.xRotation, orbit.minXRotation, orbit.maxXRotation);
        //orbit.yRotation = Mathf.Clamp(orbit.yRotation, orbit.minYRotation, orbit.maxYRotation);

    }

    [System.Serializable]
    public class CollisionHandler
    {
        public LayerMask collisionLayer;

        [HideInInspector]
        public bool isColliding = false;
        [HideInInspector]
        public Vector3[] adjustedCamClipPts;
        [HideInInspector]
        public Vector3[] desiredCamClipPts;

        private Camera cam;
        public void Initialize(Camera camera)
        {
            cam = camera;
            adjustedCamClipPts = new Vector3[5];
            desiredCamClipPts = new Vector3[5];
        }

        public Vector3 AdjustLookAtPos(Vector3 raccoonPos, Vector3 lookAtOffset)
        {
            Ray ray = new Ray(raccoonPos, Vector3.up);
            float orgDis = lookAtOffset.magnitude;
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, orgDis + .5f, collisionLayer))
            {
                if (hit.distance < orgDis)
                    return hit.distance * Vector3.up * 0.8f;
            }
            return lookAtOffset;
        }

        public void UpdateCamClipPts(Vector3 camPos, Quaternion atRotation, ref Vector3[] intoArray)
        {
            if (!cam)
                return;

            // clear the contents of intoArray
            intoArray = new Vector3[5];
            float z = cam.nearClipPlane;
            float x = Mathf.Tan(cam.fieldOfView / 3.41f) * z;
            float y = x / cam.aspect;

            // top left
            // added and rotated the point relative to the cam position
            intoArray[0] = (atRotation * new Vector3(-x, y, z)) + camPos;
            // top right
            intoArray[1] = (atRotation * new Vector3(x, y, z)) + camPos;
            // bottom left
            intoArray[2] = (atRotation * new Vector3(-x, -y, z)) + camPos;
            // bottom right
            intoArray[3] = (atRotation * new Vector3(x, -y, z)) + camPos;
            // cam pos
            intoArray[4] = camPos - cam.transform.forward;
        }

        bool CollisionDectectedAtClipPts(Vector3[] clipPts, Vector3 tarPos)
        {
            //Debug.Log("Now checking for collisions");
            for (int i = 0; i < clipPts.Length; i++)
            {
                Ray ray = new Ray(tarPos, clipPts[i] - tarPos);
                float distance = Vector3.Distance(clipPts[i], tarPos);
                if (Physics.Raycast(ray, distance, collisionLayer))
                {
                    //Debug.Log("Collision found");
                    return true;
                }
            }
            return false;
        }

        public float AdjustedDisWithRaycast(Vector3 tarPos)
        {
            float dis = -1;

            for (int i = 0; i < desiredCamClipPts.Length; i++)
            {
                Ray ray = new Ray(tarPos, desiredCamClipPts[i] - tarPos);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (dis == -1)
                        dis = hit.distance;
                    else
                    {
                        if (hit.distance < dis)
                            dis = hit.distance;
                    }
                }
            }

            if (dis == -1)
                return 0;
            else
                return dis;
        }

        public void CheckColliding(Vector3 tarPos)
        {
            isColliding = CollisionDectectedAtClipPts(desiredCamClipPts, tarPos);
        }
    }

}
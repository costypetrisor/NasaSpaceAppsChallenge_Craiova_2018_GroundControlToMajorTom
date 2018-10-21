using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hackathon
{
    public class PlayerController : MonoBehaviour
    {
        bool useRoverCamera = true;
        Camera roverCam = null;
        Camera fancyCam = null;

        MeshRenderer toRemoveFromRenderWhileRoverCamera = null;

        // Use this for initialization
        void Start()
        {
            roverCam = GameObject.Find("RoverCamera").GetComponent<Camera>();
            fancyCam = Camera.main;

            toRemoveFromRenderWhileRoverCamera = GameObject.Find("mast_01.001_camera").GetComponent<MeshRenderer>();

            SetupFancyCamera();
            SwitchToRoverCamera();
        }

        void SetupFancyCamera()
        {
            UnityStandardAssets.Utility.SmoothFollow smoothFollowComponent = fancyCam.GetComponent< UnityStandardAssets.Utility.SmoothFollow>();
            if (smoothFollowComponent)
            {
                smoothFollowComponent.SetTarget(gameObject.transform);
            }

            RoverFollow roverFollowComponent = fancyCam.GetComponent<RoverFollow>();
            if (roverFollowComponent)
            {
                roverFollowComponent.SetTarget(gameObject.transform);
            }
        }

        // Update is called once per frame
        void Update()
        {
            
            if (Input.GetKeyUp(KeyCode.C) || Input.GetButtonDown("Fire3"))
            {
                if (useRoverCamera)
                {
                    SwitchToFancyCamera();
                }
                else
                {
                    SwitchToRoverCamera();
                }
            }
        }

        void SwitchToFancyCamera()
        {
            useRoverCamera = false;
            fancyCam.enabled = !useRoverCamera;
            roverCam.enabled = useRoverCamera;

            if (toRemoveFromRenderWhileRoverCamera)
            {
                toRemoveFromRenderWhileRoverCamera.enabled = true;
            }
        }

        void SwitchToRoverCamera()
        {
            useRoverCamera = true;
            fancyCam.enabled = !useRoverCamera;
            roverCam.enabled = useRoverCamera;

            if (toRemoveFromRenderWhileRoverCamera)
            {
                toRemoveFromRenderWhileRoverCamera.enabled = false;
            }
        }
    }
}

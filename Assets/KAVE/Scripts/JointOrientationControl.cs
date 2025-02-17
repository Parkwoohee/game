﻿/*
If you use or adapt this software in your research please cite:

Afonso Gonçalves and Sergi Bermúdez i Badia. 2018.
KAVE: Building Kinect Based CAVE Automatic Virtual Environments, Methods for Surround-Screen Projection Management, Motion Parallax and Full-Body Interaction Support.
PACM on Human-Computer Interaction, Vol. 1, EICS, Article 10 (June 2018), 15 pages.
https://doi.org/10.1145/3229092

The author can be contacted at afonso.goncalves@m-iti.org
 */

using UnityEngine;
using Windows.Kinect;
using Vector4 = UnityEngine.Vector4;

namespace VRKave
{
    public class JointOrientationControl : MonoBehaviour
    {
        private GameObject _sourceManager;
        private VRKinectBodySource _bodySource;
        private VRWorldManager _vrWorldManager;
        private int _bodyIndex = 0;
        private Vector4 _fromKinecttoUnity;

        public GameObject[] AvatarBones;

        //SpineBase 0
        //SpineMid 1
        //Neck 2
        //Head 3
        //ShoulderLeft 4
        //ElbowLeft 5
        //WristLeft 6
        //HandLeft 7
        //ShoulderRight 8
        //ElbowRight 9
        //WristRight 10
        //HandRight 11
        //HipLeft 12
        //KneeLeft 13
        //AnkleLeft 14
        //FootLeft 15
        //HipRight 16
        //KneeRight 17
        //AnkleRight 18
        //FootRight 19
        //SpineShoulder 20
        //HandTipLeft 21
        //ThumbLeft 22
        //HandTipRight 23
        //ThumbRight 24

        void Start()
        {
            _sourceManager = gameObject.transform.parent.gameObject;

            if (_sourceManager != null)
                _bodySource = _sourceManager.GetComponent<VRKinectBodySource>();

            _vrWorldManager = gameObject.GetComponentInParent<VRWorldManager>();

            InitializeRotations();
        }

        void FixedUpdate()
        {
            var kinectBody = GetKinectBody();
            if (kinectBody == null)
                return;

            RotateBones(kinectBody);
            ResizeBones();
        }

        private void RotateBones(Body kinectBody)
        {
            int kinectJoint = -1;
            foreach (var bone in AvatarBones)
            {
                if (bone == null)
                    break;
                kinectJoint++;
                bone.transform.rotation = Quaternion.identity;

                if (kinectBody.IsTracked)
                {
                    var kinectRotation = GetVector4FromJoint(kinectBody.JointOrientations[(JointType) kinectJoint]);

                    var unityRotation = new Quaternion
                    {
                        x = _fromKinecttoUnity.x * kinectRotation.x,
                        y = _fromKinecttoUnity.y * kinectRotation.y,
                        z = _fromKinecttoUnity.z * kinectRotation.z,
                        w = _fromKinecttoUnity.w * kinectRotation.w
                    };

                    bone.transform.rotation = _sourceManager.transform.rotation * unityRotation;
                }
            }
        }

        private void ResizeBones()
        {
            var scale = new Vector3(1, 1, 1) * _vrWorldManager.KaveScale;
            var boneIndex = new[] {3, 7, 11, 15, 19};
            foreach (var index in boneIndex)
            {
                AvatarBones[index].gameObject.transform.localScale = scale;
            }
        }

        private Body GetKinectBody()
        {
            if (_sourceManager == null)
                return null;

            if (_bodySource == null)
                _bodySource = _sourceManager.GetComponent<VRKinectBodySource>();

            if (_bodySource == null)
                return null;

            Body[] kinectBodies = _bodySource.GetData();
            if (kinectBodies == null)
                return null;

            var kinectBody = kinectBodies[_bodyIndex];
            return kinectBody;
        }

        private void InitializeRotations()
        {
            _fromKinecttoUnity.x = -1;
            _fromKinecttoUnity.y = 1;
            _fromKinecttoUnity.z = 1;
            _fromKinecttoUnity.w = -1;
        }

        private static Vector4 GetVector4FromJoint(JointOrientation joint)
        {
            return new Vector4(joint.Orientation.X, joint.Orientation.Y, joint.Orientation.Z, joint.Orientation.W);
        }

        private static Vector4 GetVector4FromKinectVector4(Windows.Kinect.Vector4 face)
        {
            return new Vector4(face.X, face.Y, face.Z, face.W);
        }

        public void SetBodyIndex(int index)
        {
            _bodyIndex = index;
        }
    }
}
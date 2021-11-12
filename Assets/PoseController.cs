using CopyCat.Common.Events;
using CopyCat.Common.Models;
using CopyCat.Common.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CopyCatStore
{
    private static CopyCatStore _instance;
    public static CopyCatStore Instance => _instance ?? (_instance = new CopyCatStore());

    public Vector3 Forward { get; set; }
    public float LeftHandScore { get; set; }
    public float RightHandScore { get; set; }

    private CopyCatStore()
    {

    }
}

public static class LandmarkExtensions
{
    public static Vector3 ToVector3(this Landmark pose)
    {
        return new Vector3(pose.X, pose.Y, pose.Z);
    }
}

class Bone
{
    public HumanBodyBones Name { get; }
    public Transform Transform { get; set; }
    public Quaternion Inverse { get; set; }

    public Bone(HumanBodyBones name, Transform transform)
    {
        Name = name;
        Transform = transform;
    }
}


public class PoseController : MonoBehaviour
{
    object _lockObject;
    Dictionary<HumanBodyBones, Bone> _bones;
    PosePackage _posePackage;
    Bone _hips;
    Task _commTask;
    private void Start()
    {
        _lockObject = new object();
        Initialize();

        _commTask = Task.Run(() =>
        {
            var eventBusMediator = new EventBusMediator();
            eventBusMediator.Subscribe<PosePackage>(package =>
            {
                lock (_lockObject)
                {
                    _posePackage = package;
                    CopyCatStore.Instance.LeftHandScore = (_posePackage[EPoseLandmark.IndexLeft].Visibility + _posePackage[EPoseLandmark.PinkyLeft].Visibility) / 2.0f;
                    CopyCatStore.Instance.RightHandScore = (_posePackage[EPoseLandmark.IndexRight].Visibility + _posePackage[EPoseLandmark.PinkyRight].Visibility) / 2.0f;
                }
            });
        });
    }


    private void Initialize()
    {
        _bones = new Dictionary<HumanBodyBones, Bone>();

        var animator = GetComponent<Animator>();

        var bodyBones = new List<HumanBodyBones>();
        bodyBones.Add(HumanBodyBones.LeftUpperLeg);
        bodyBones.Add(HumanBodyBones.RightUpperLeg);
        bodyBones.Add(HumanBodyBones.LeftLowerLeg);
        bodyBones.Add(HumanBodyBones.RightLowerLeg);
        bodyBones.Add(HumanBodyBones.LeftFoot);
        bodyBones.Add(HumanBodyBones.RightFoot);
        bodyBones.Add(HumanBodyBones.LeftToes);
        bodyBones.Add(HumanBodyBones.RightToes);
        bodyBones.Add(HumanBodyBones.Spine);
        bodyBones.Add(HumanBodyBones.Chest);
        bodyBones.Add(HumanBodyBones.UpperChest);
        bodyBones.Add(HumanBodyBones.Neck);
        bodyBones.Add(HumanBodyBones.Head);
        bodyBones.Add(HumanBodyBones.LeftUpperArm);
        bodyBones.Add(HumanBodyBones.RightUpperArm);
        bodyBones.Add(HumanBodyBones.LeftLowerArm);
        bodyBones.Add(HumanBodyBones.RightLowerArm);
        bodyBones.Add(HumanBodyBones.LeftHand);
        bodyBones.Add(HumanBodyBones.RightHand);

        foreach (HumanBodyBones bone in bodyBones)
        {
            var transform = animator.GetBoneTransform(bone);

            if (transform == null)
                Debug.Log(bone);

            _bones[bone] = new Bone(bone, transform);
        }

        var forward = TriangleNormal(
            animator.GetBoneTransform(HumanBodyBones.Spine).transform.position,
            animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).transform.position,
            animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).transform.position);

        _hips = new Bone(HumanBodyBones.Hips, animator.GetBoneTransform(HumanBodyBones.Hips).transform);
        _hips.Inverse = Quaternion.Inverse(Quaternion.LookRotation(forward)) * _hips.Transform.rotation;

        foreach (var bone in _bones.Values)
        {
            var childName = HumanBodyBones.LastBone;
            switch (bone.Name)
            {
                case HumanBodyBones.LeftUpperLeg:
                    childName = HumanBodyBones.LeftLowerLeg;
                    break;
                case HumanBodyBones.LeftLowerLeg:
                    childName = HumanBodyBones.LeftFoot;
                    break;
                case HumanBodyBones.RightUpperLeg:
                    childName = HumanBodyBones.RightLowerLeg;
                    break;
                case HumanBodyBones.RightLowerLeg:
                    childName = HumanBodyBones.RightFoot;
                    break;
                case HumanBodyBones.LeftFoot:
                    childName = HumanBodyBones.LeftToes;
                    break;
                case HumanBodyBones.RightFoot:
                    childName = HumanBodyBones.RightToes;
                    break;
                case HumanBodyBones.Spine:
                    childName = HumanBodyBones.Chest;
                    break;
                case HumanBodyBones.Chest:
                    childName = HumanBodyBones.UpperChest;
                    break;
                case HumanBodyBones.UpperChest:
                    childName = HumanBodyBones.Neck;
                    break;
                case HumanBodyBones.Neck:
                    childName = HumanBodyBones.Head;
                    break;
                case HumanBodyBones.LeftUpperArm:
                    childName = HumanBodyBones.LeftLowerArm;
                    break;
                case HumanBodyBones.LeftLowerArm:
                    childName = HumanBodyBones.LeftHand;
                    break;
                case HumanBodyBones.RightUpperArm:
                    childName = HumanBodyBones.RightLowerArm;
                    break;
                case HumanBodyBones.RightLowerArm:
                    childName = HumanBodyBones.RightHand;
                    break;
                default:
                    continue;
            }

            bone.Inverse = Quaternion.Inverse(
                    Quaternion.LookRotation(
                        _bones[childName].Transform.position - bone.Transform.position, forward)) * bone.Transform.rotation;
        }
    }

    void OnApplicationQuit()
    {
        _commTask?.Dispose();
    }

    private void Update()
    {
        if (_posePackage == null)
            return;

        lock (_lockObject)
        {
            var leftShoulder = _posePackage[EPoseLandmark.ShoulderLeft].ToVector3();
            var rightShoulder = _posePackage[EPoseLandmark.ShoulderRight].ToVector3();

            var leftHip = _posePackage[EPoseLandmark.HipLeft].ToVector3();
            var rightHip = _posePackage[EPoseLandmark.HipRight].ToVector3();

            var neck = (leftShoulder + rightShoulder) / 2.0f;
            var chest = (neck + leftHip + rightHip) / 3.0f;
            var upperChest = (chest + neck) / 2.0f;
            var spline = (chest + leftHip + rightHip) / 3.0f;
            var forward = TriangleNormal(spline, leftHip, rightHip);
            CopyCatStore.Instance.Forward = forward;

            //var offset = landmarks[0];
            //for (int i = 1; i < landmarks.Count; i++)
            //{
            //    var x = landmarks[i].x - offset.x;
            //    var y = landmarks[i].y - offset.y;
            //    var z = landmarks[i].z - offset.z;

            //    LandmarkObjects[i].transform.localPosition = new Vector3(x, y, z);
            //}

            _hips.Transform.rotation = Quaternion.LookRotation(forward) * _hips.Inverse;
            _hips.Transform.position = (leftHip + rightHip) / 2.0f;


            var vector = _posePackage[EPoseLandmark.HipLeft].ToVector3()
                - _posePackage[EPoseLandmark.KneeLeft].ToVector3();
            _bones[HumanBodyBones.LeftUpperLeg].Transform.rotation = Quaternion.LookRotation(
                vector
                , forward) * _bones[HumanBodyBones.LeftUpperLeg].Inverse;
            //_bones[HumanBodyBones.LeftUpperLeg].Transform.localPosition = _hips.Transform.position - _posePackage[EPoseLandmark.HipLeft].ToVector3();
            //_bones[HumanBodyBones.LeftUpperLeg].Transform.localPosition = _hips.Transform.position - _posePackage[EPoseLandmark.HipLeft].ToVector3();
            //_bones[HumanBodyBones.LeftLowerLeg].Transform.localPosition = _posePackage[EPoseLandmark.KneeLeft].ToVector3() - _bones[HumanBodyBones.LeftUpperLeg].Transform.position;

            //_hips.Transform.rotation = Quaternion.LookRotation(forward) * _hips.Inverse;

            //foreach (var bone in _bones.Values)
            //{
            //    var vector = new Vector3();
            //    switch (bone.Name)
            //    {
            //        case HumanBodyBones.LeftUpperLeg:
            //            vector = _posePackage[EPoseLandmark.HipLeft].ToVector3() - _posePackage[EPoseLandmark.KneeLeft].ToVector3();
            //            break;
            //        case HumanBodyBones.LeftLowerLeg:
            //            vector = _posePackage[EPoseLandmark.KneeLeft].ToVector3() - _posePackage[EPoseLandmark.AnkleLeft].ToVector3();
            //            break;
            //        case HumanBodyBones.RightUpperLeg:
            //            vector = _posePackage[EPoseLandmark.HipRight].ToVector3() - _posePackage[EPoseLandmark.KneeRight].ToVector3();
            //            break;
            //        case HumanBodyBones.RightLowerLeg:
            //            vector = _posePackage[EPoseLandmark.KneeRight].ToVector3() - _posePackage[EPoseLandmark.AnkleRight].ToVector3();
            //            break;
            //        //case HumanBodyBones.LeftFoot:
            //        //    var leftToe = (_posePackage[EPoseLandmark.AnkleLeft].ToVector3()
            //        //        + _posePackage[EPoseLandmark.AnkleLeft].ToVector3()
            //        //        + _posePackage[EPoseLandmark.AnkleLeft].ToVector3()
            //        //        + _posePackage[EPoseLandmark.FootLeft].ToVector3()
            //        //        + _posePackage[EPoseLandmark.HeelLeft].ToVector3()) / 5.0f;
            //        //    vector = _posePackage[EPoseLandmark.AnkleLeft].ToVector3() - leftToe;

            //            //vector = _posePackage[EPoseLandmark.AnkleLeft].ToVector3() - _posePackage[EPoseLandmark.FootLeft].ToVector3();
            //            //break;
            //        //case HumanBodyBones.RightFoot:
            //        //    var rightToe = (_posePackage[EPoseLandmark.AnkleRight].ToVector3()
            //        //        + _posePackage[EPoseLandmark.AnkleRight].ToVector3()
            //        //        + _posePackage[EPoseLandmark.AnkleRight].ToVector3()
            //        //        + _posePackage[EPoseLandmark.FootRight].ToVector3() 
            //        //        + _posePackage[EPoseLandmark.HeelRight].ToVector3()) / 4.0f;
            //        //    vector = _posePackage[EPoseLandmark.AnkleRight].ToVector3() - rightToe;
            //            //vector = _posePackage[EPoseLandmark.AnkleRight].ToVector3() - _posePackage[EPoseLandmark.FootRight].ToVector3();
            //            //break;
            //        case HumanBodyBones.Spine:
            //            vector = spline - chest;
            //            break;
            //        case HumanBodyBones.Chest:
            //            vector = chest - upperChest;
            //            break;
            //        case HumanBodyBones.UpperChest:
            //            vector = upperChest - neck;
            //            break;
            //        case HumanBodyBones.LeftUpperArm:
            //            vector = _posePackage[EPoseLandmark.ShoulderLeft].ToVector3() - _posePackage[EPoseLandmark.ElbowLeft].ToVector3();
            //            break;
            //        case HumanBodyBones.LeftLowerArm:
            //            vector = _posePackage[EPoseLandmark.ElbowLeft].ToVector3() - _posePackage[EPoseLandmark.WristLeft].ToVector3();
            //            break;
            //        case HumanBodyBones.RightUpperArm:
            //            vector = _posePackage[EPoseLandmark.ShoulderRight].ToVector3() - _posePackage[EPoseLandmark.ElbowRight].ToVector3();
            //            break;
            //        case HumanBodyBones.RightLowerArm:
            //            vector = _posePackage[EPoseLandmark.ElbowRight].ToVector3() - _posePackage[EPoseLandmark.WristRight].ToVector3();
            //            break;
            //        default:
            //            continue;
            //    }

            //    //bone.Transform.rotation = Quaternion.LookRotation(vector, forward) * bone.Inverse;

            //    bone.Transform.rotation = Quaternion.Slerp(
            //        bone.Transform.rotation,
            //        Quaternion.LookRotation(vector, forward) * bone.Inverse,
            //        0.1f);
            //}
        }
        
    }

    private Vector3 TriangleNormal(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 d1 = a - b;
        Vector3 d2 = a - c;
        Vector3 dd = Vector3.Cross(d1, d2);
        dd.Normalize();

        return dd;
    }
}
using CopyCat.Common.Events;
using CopyCat.Common.Models;
using CopyCat.Common.Modules;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class LeftHandController : MonoBehaviour
{
    object _lockObject;
    Dictionary<HumanBodyBones, Bone> _bones;
    LeftHandPackage _handPackage;
    Task _commTask;

    private void Start()
    {
        _lockObject = new object();

        var rigidBodys = GetComponentsInChildren<Rigidbody>();
        foreach (var rigidBody in rigidBodys)
            rigidBody.constraints = RigidbodyConstraints.FreezeAll;

        _bones = new Dictionary<HumanBodyBones, Bone>();

        var animator = GetComponent<Animator>();

        var bones = new List<HumanBodyBones>();
        bones.Add(HumanBodyBones.LeftHand);
        bones.Add(HumanBodyBones.LeftThumbProximal);
        bones.Add(HumanBodyBones.LeftThumbIntermediate);
        bones.Add(HumanBodyBones.LeftThumbDistal);
        bones.Add(HumanBodyBones.LeftIndexProximal);
        bones.Add(HumanBodyBones.LeftIndexIntermediate);
        bones.Add(HumanBodyBones.LeftIndexDistal);
        bones.Add(HumanBodyBones.LeftMiddleProximal);
        bones.Add(HumanBodyBones.LeftMiddleIntermediate);
        bones.Add(HumanBodyBones.LeftMiddleDistal);
        bones.Add(HumanBodyBones.LeftRingProximal);
        bones.Add(HumanBodyBones.LeftRingIntermediate);
        bones.Add(HumanBodyBones.LeftRingDistal);
        bones.Add(HumanBodyBones.LeftLittleProximal);
        bones.Add(HumanBodyBones.LeftLittleIntermediate);
        bones.Add(HumanBodyBones.LeftLittleDistal);

        foreach (HumanBodyBones bone in bones)
        {
            var transform = animator.GetBoneTransform(bone);
            if (transform == null)
                continue;

            _bones[bone] = new Bone(bone, transform);
        }

        var forward = TriangleNormal(
           _bones[HumanBodyBones.LeftHand].Transform.position,
           _bones[HumanBodyBones.LeftIndexProximal].Transform.position,
           _bones[HumanBodyBones.LeftLittleProximal].Transform.position);

        foreach (var bone in _bones.Values)
        {
            var local = forward;
            var childName = HumanBodyBones.LastBone;
            switch (bone.Name)
            {
                case HumanBodyBones.LeftHand:
                    //local = CopyCatStore.Instance.Forward;
                    childName = HumanBodyBones.LeftMiddleProximal;
                    break;
                case HumanBodyBones.LeftThumbProximal:
                    childName = HumanBodyBones.LeftThumbIntermediate;
                    break;
                case HumanBodyBones.LeftThumbIntermediate:
                    childName = HumanBodyBones.LeftThumbDistal;
                    break;
                case HumanBodyBones.LeftIndexProximal:
                    childName = HumanBodyBones.LeftIndexIntermediate;
                    break;
                case HumanBodyBones.LeftIndexIntermediate:
                    childName = HumanBodyBones.LeftIndexDistal;
                    break;
                case HumanBodyBones.LeftMiddleProximal:
                    childName = HumanBodyBones.LeftMiddleIntermediate;
                    break;
                case HumanBodyBones.LeftMiddleIntermediate:
                    childName = HumanBodyBones.LeftMiddleDistal;
                    break;
                case HumanBodyBones.LeftRingProximal:
                    childName = HumanBodyBones.LeftRingIntermediate;
                    break;
                case HumanBodyBones.LeftRingIntermediate:
                    childName = HumanBodyBones.LeftRingDistal;
                    break;
                case HumanBodyBones.LeftLittleProximal:
                    childName = HumanBodyBones.LeftLittleIntermediate;
                    break;
                case HumanBodyBones.LeftLittleIntermediate:
                    childName = HumanBodyBones.LeftLittleDistal;
                    break;
            }

            if (childName != HumanBodyBones.LastBone)
            {
                bone.Inverse = Quaternion.Inverse(
                    Quaternion.LookRotation(
                        _bones[childName].Transform.position - bone.Transform.position, local)) * bone.Transform.rotation;
            }
            else
            {
                //var fv = jointPoint.Parent.Pos3D - jointPoint.Pos3D;
                //jointPoint.Transform.rotation = Quaternion.LookRotation(jointPoint.Pos3D - jointPoint.Child.Pos3D, fv) * jointPoint.InverseRotation;
            }
        }

        _commTask = Task.Run(() =>
        {
            var eventBusMediator = new EventBusMediator();
            eventBusMediator.Subscribe<LeftHandPackage>(package =>
            {
                lock (_lockObject)
                {
                    _handPackage = package;
                }
            });
        });
    }

    void OnApplicationQuit()
    {
        _commTask?.Dispose();
    }

    private void Update()
    {
        if (_handPackage == null)
            return;

        if (CopyCatStore.Instance.LeftHandScore < 0.9)
            return;

        lock (_lockObject)
        {
            var forward = TriangleNormal(
              _handPackage[EHandLandmark.Wrist].ToVector3(),
              _handPackage[EHandLandmark.IndexMcp].ToVector3(),
              _handPackage[EHandLandmark.PinkyMcp].ToVector3());

            foreach (var bone in _bones.Values)
            {
                var localForward = forward;
                Vector3 vector = new Vector3();
                switch (bone.Name)
                {
                    case HumanBodyBones.LeftHand:
                        //localForward = CopyCatStore.Instance.Forward;
                        vector = _handPackage[EHandLandmark.Wrist].ToVector3() - _handPackage[EHandLandmark.MiddleMcp].ToVector3();
                        break;
                    case HumanBodyBones.LeftThumbProximal:
                        vector = _handPackage[EHandLandmark.ThumbCmc].ToVector3() - _handPackage[EHandLandmark.ThumbMcp].ToVector3();
                        break;
                    case HumanBodyBones.LeftThumbIntermediate:
                        vector = _handPackage[EHandLandmark.ThumbMcp].ToVector3() - _handPackage[EHandLandmark.ThumbIp].ToVector3();
                        break;
                    case HumanBodyBones.LeftThumbDistal:
                        vector = _handPackage[EHandLandmark.ThumbIp].ToVector3() - _handPackage[EHandLandmark.ThumbTip].ToVector3();
                        break;
                    case HumanBodyBones.LeftIndexProximal:
                        vector = _handPackage[EHandLandmark.IndexMcp].ToVector3() - _handPackage[EHandLandmark.IndexPip].ToVector3();
                        break;
                    case HumanBodyBones.LeftIndexIntermediate:
                        vector = _handPackage[EHandLandmark.IndexPip].ToVector3() - _handPackage[EHandLandmark.IndexDip].ToVector3();
                        break;
                    case HumanBodyBones.LeftIndexDistal:
                        vector = _handPackage[EHandLandmark.IndexDip].ToVector3() - _handPackage[EHandLandmark.IndexTip].ToVector3();
                        break;
                    case HumanBodyBones.LeftMiddleProximal:
                        vector = _handPackage[EHandLandmark.MiddleMcp].ToVector3() - _handPackage[EHandLandmark.MiddlePip].ToVector3();
                        break;
                    case HumanBodyBones.LeftMiddleIntermediate:
                        vector = _handPackage[EHandLandmark.MiddlePip].ToVector3() - _handPackage[EHandLandmark.MiddleDip].ToVector3();
                        break;
                    case HumanBodyBones.LeftMiddleDistal:
                        vector = _handPackage[EHandLandmark.MiddleDip].ToVector3() - _handPackage[EHandLandmark.MiddleTip].ToVector3();
                        break;
                    case HumanBodyBones.LeftRingProximal:
                        vector = _handPackage[EHandLandmark.RingMcp].ToVector3() - _handPackage[EHandLandmark.RingPip].ToVector3();
                        break;
                    case HumanBodyBones.LeftRingIntermediate:
                        vector = _handPackage[EHandLandmark.RingPip].ToVector3() - _handPackage[EHandLandmark.RingDip].ToVector3();
                        break;
                    case HumanBodyBones.LeftRingDistal:
                        vector = _handPackage[EHandLandmark.RingDip].ToVector3() - _handPackage[EHandLandmark.RingTip].ToVector3();
                        break;
                    case HumanBodyBones.LeftLittleProximal:
                        vector = _handPackage[EHandLandmark.PinkyMcp].ToVector3() - _handPackage[EHandLandmark.PinkyPip].ToVector3();
                        break;
                    case HumanBodyBones.LeftLittleIntermediate:
                        vector = _handPackage[EHandLandmark.PinkyPip].ToVector3() - _handPackage[EHandLandmark.PinkyDip].ToVector3();
                        break;
                    case HumanBodyBones.LeftLittleDistal:
                        vector = _handPackage[EHandLandmark.PinkyDip].ToVector3() - _handPackage[EHandLandmark.PinkyTip].ToVector3();
                        break;
                    default:
                        continue;
                }

                bone.Transform.rotation = Quaternion.Slerp(
                    bone.Transform.rotation,
                    Quaternion.LookRotation(vector, localForward) * bone.Inverse,
                    0.1f);
            }
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

using CopyCat.Common.Events;
using CopyCat.Common.Models;
using CopyCat.Common.Modules;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RightHandController : MonoBehaviour
{
    object _lockObject;
    Dictionary<HumanBodyBones, Bone> _bones;
    RightHandPackage _handPackage;
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
        bones.Add(HumanBodyBones.RightHand);
        bones.Add(HumanBodyBones.RightThumbProximal);
        bones.Add(HumanBodyBones.RightThumbIntermediate);
        bones.Add(HumanBodyBones.RightThumbDistal);
        bones.Add(HumanBodyBones.RightIndexProximal);
        bones.Add(HumanBodyBones.RightIndexIntermediate);
        bones.Add(HumanBodyBones.RightIndexDistal);
        bones.Add(HumanBodyBones.RightMiddleProximal);
        bones.Add(HumanBodyBones.RightMiddleIntermediate);
        bones.Add(HumanBodyBones.RightMiddleDistal);
        bones.Add(HumanBodyBones.RightRingProximal);
        bones.Add(HumanBodyBones.RightRingIntermediate);
        bones.Add(HumanBodyBones.RightRingDistal);
        bones.Add(HumanBodyBones.RightLittleProximal);
        bones.Add(HumanBodyBones.RightLittleIntermediate);
        bones.Add(HumanBodyBones.RightLittleDistal);

        foreach (HumanBodyBones bone in bones)
        {
            var transform = animator.GetBoneTransform(bone);
            if (transform == null)
                continue;

            _bones[bone] = new Bone(bone, transform);
        }

        var forward = TriangleNormal(
           _bones[HumanBodyBones.RightHand].Transform.position,
           _bones[HumanBodyBones.RightIndexProximal].Transform.position,
           _bones[HumanBodyBones.RightLittleProximal].Transform.position);

        foreach (var bone in _bones.Values)
        {
            var local = forward;
            var childName = HumanBodyBones.LastBone;
            switch (bone.Name)
            {
                case HumanBodyBones.RightHand:
                    //local = CopyCatStore.Instance.Forward;
                    childName = HumanBodyBones.RightMiddleProximal;
                    break;
                case HumanBodyBones.RightThumbProximal:
                    childName = HumanBodyBones.RightThumbIntermediate;
                    break;
                case HumanBodyBones.RightThumbIntermediate:
                    childName = HumanBodyBones.RightThumbDistal;
                    break;
                case HumanBodyBones.RightIndexProximal:
                    childName = HumanBodyBones.RightIndexIntermediate;
                    break;
                case HumanBodyBones.RightIndexIntermediate:
                    childName = HumanBodyBones.RightIndexDistal;
                    break;
                case HumanBodyBones.RightMiddleProximal:
                    childName = HumanBodyBones.RightMiddleIntermediate;
                    break;
                case HumanBodyBones.RightMiddleIntermediate:
                    childName = HumanBodyBones.RightMiddleDistal;
                    break;
                case HumanBodyBones.RightRingProximal:
                    childName = HumanBodyBones.RightRingIntermediate;
                    break;
                case HumanBodyBones.RightRingIntermediate:
                    childName = HumanBodyBones.RightRingDistal;
                    break;
                case HumanBodyBones.RightLittleProximal:
                    childName = HumanBodyBones.RightLittleIntermediate;
                    break;
                case HumanBodyBones.RightLittleIntermediate:
                    childName = HumanBodyBones.RightLittleDistal;
                    break;
            }

            if (childName != HumanBodyBones.LastBone)
            {
                bone.Inverse = Quaternion.Inverse(
                    Quaternion.LookRotation(
                        _bones[childName].Transform.position = bone.Transform.position, local)) * bone.Transform.rotation;
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
            eventBusMediator.Subscribe<RightHandPackage>(package =>
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

        if (CopyCatStore.Instance.RightHandScore < 0.9)
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
                    case HumanBodyBones.RightHand:
                        //localForward = CopyCatStore.Instance.Forward;
                        vector = _handPackage[EHandLandmark.Wrist].ToVector3() - _handPackage[EHandLandmark.MiddleMcp].ToVector3();
                        break;
                    case HumanBodyBones.RightThumbProximal:
                        vector = _handPackage[EHandLandmark.ThumbCmc].ToVector3() - _handPackage[EHandLandmark.ThumbMcp].ToVector3();
                        break;
                    case HumanBodyBones.RightThumbIntermediate:
                        vector = _handPackage[EHandLandmark.ThumbMcp].ToVector3() - _handPackage[EHandLandmark.ThumbIp].ToVector3();
                        break;
                    case HumanBodyBones.RightThumbDistal:
                        vector = _handPackage[EHandLandmark.ThumbIp].ToVector3() - _handPackage[EHandLandmark.ThumbTip].ToVector3();
                        break;
                    case HumanBodyBones.RightIndexProximal:
                        vector = _handPackage[EHandLandmark.IndexMcp].ToVector3() - _handPackage[EHandLandmark.IndexPip].ToVector3();
                        break;
                    case HumanBodyBones.RightIndexIntermediate:
                        vector = _handPackage[EHandLandmark.IndexPip].ToVector3() - _handPackage[EHandLandmark.IndexDip].ToVector3();
                        break;
                    case HumanBodyBones.RightIndexDistal:
                        vector = _handPackage[EHandLandmark.IndexDip].ToVector3() - _handPackage[EHandLandmark.IndexTip].ToVector3();
                        break;
                    case HumanBodyBones.RightMiddleProximal:
                        vector = _handPackage[EHandLandmark.MiddleMcp].ToVector3() - _handPackage[EHandLandmark.MiddlePip].ToVector3();
                        break;
                    case HumanBodyBones.RightMiddleIntermediate:
                        vector = _handPackage[EHandLandmark.MiddlePip].ToVector3() - _handPackage[EHandLandmark.MiddleDip].ToVector3();
                        break;
                    case HumanBodyBones.RightMiddleDistal:
                        vector = _handPackage[EHandLandmark.MiddleDip].ToVector3() - _handPackage[EHandLandmark.MiddleTip].ToVector3();
                        break;
                    case HumanBodyBones.RightRingProximal:
                        vector = _handPackage[EHandLandmark.RingMcp].ToVector3() - _handPackage[EHandLandmark.RingPip].ToVector3();
                        break;
                    case HumanBodyBones.RightRingIntermediate:
                        vector = _handPackage[EHandLandmark.RingPip].ToVector3() - _handPackage[EHandLandmark.RingDip].ToVector3();
                        break;
                    case HumanBodyBones.RightRingDistal:
                        vector = _handPackage[EHandLandmark.RingDip].ToVector3() - _handPackage[EHandLandmark.RingTip].ToVector3();
                        break;
                    case HumanBodyBones.RightLittleProximal:
                        vector = _handPackage[EHandLandmark.PinkyMcp].ToVector3() - _handPackage[EHandLandmark.PinkyPip].ToVector3();
                        break;
                    case HumanBodyBones.RightLittleIntermediate:
                        vector = _handPackage[EHandLandmark.PinkyPip].ToVector3() - _handPackage[EHandLandmark.PinkyDip].ToVector3();
                        break;
                    case HumanBodyBones.RightLittleDistal:
                        vector = _handPackage[EHandLandmark.PinkyDip].ToVector3() - _handPackage[EHandLandmark.PinkyTip].ToVector3();
                        break;
                    default:
                        continue;
                }

                //bone.Transform.SetPositionAndRotation()
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

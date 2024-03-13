using System;
using System.Collections;
using System.Collections.Generic;
using PlayableTools;
using PlayableTools.Nodes;
// using Sirenix.OdinInspector;
using UnityEngine;

public class PlayableTest : MonoBehaviour
{
    private PlayableNodeGraphManager _graphManager;
    public NodeDataSO NodeDataSo;
    public Animator Animator;

    private void Awake()
    {
        Create();
        _graphManager.GeneratePlayableGraph(Animator);
    }

    private void OnDestroy()
    {
        Stop();
    }

    // [Button]
    public void Create()
    {
        _graphManager = new PlayableNodeGraphManager("1");
        _graphManager.Import(NodeDataSo);
        
    }

    // [Button]
    public void Stop()
    {
        _graphManager.StopPlay();
    }

    public List<AnimationClip> ClipToPlayInSlot;
    public string slotName;
    public int index;
    // [Button]
    public void PlayAniClip()
    {
        _graphManager.PlayAnimationClip(slotName,ClipToPlayInSlot[index]);
        index++;
        if (index == ClipToPlayInSlot.Count)
        {
            index = 0;
        }
    }
    private void Update()
    {
        _graphManager.RuntimeUpdate();
        
        _graphManager.SetFloat("VelocityY",Time.time%1);
    }
    

    public void OnFootStep()
    {
        
    }
}

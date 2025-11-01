using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState 
{
    protected Player player;
    protected PlayerStateMachine stateMachine;
    private Animator animationController;
    private string animationName;

protected bool isExitingState;
    protected float startTime;
    protected bool isAnimationFinished;
    public PlayerState(Player _player, PlayerStateMachine _stateMachine, Animator _animationController ,string _animationName)
    {
        player = _player;
        stateMachine = _stateMachine;
        animationController = _animationController;
      animationName = _animationName;
    }

    public virtual void Enter()
    {
        startTime = Time.time;
        isExitingState = false;
        isAnimationFinished = false;
        animationController.SetBool(animationName, true);
    }

    public virtual void Exit()
    {
        isExitingState = true;
        if(!isAnimationFinished) isAnimationFinished = true;
        animationController.SetBool(animationName, false);
    }
    public virtual void LogicUpdate()
    {
       TransitionChecks();
    }
    public virtual void PhysicsUpdate()
    {
    }
    public virtual void TransitionChecks()
    {
    }

    public virtual void AnimationTrigger()
    {
        isAnimationFinished = true;
    }
}

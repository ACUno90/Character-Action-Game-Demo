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
    //Intialize everything in here to set up for future use
    public PlayerState(Player _player, PlayerStateMachine _stateMachine, Animator _animationController ,string _animationName)
    {
        player = _player;
        stateMachine = _stateMachine;
     //   animationController = _animationController;
      animationName = _animationName;
    }

    public virtual void Enter()
    {
       //this function beings whatever time is in the game and any state or animation I'll add later would not end once we use enter fucn 
        startTime = Time.time;
        isExitingState = false;
        isAnimationFinished = false;
     //   animationController.SetBool(animationName, true);
    }

    public virtual void Exit()
    {
        //ends the func of whatever it is and stopping animation
        isExitingState = true;
        if(!isAnimationFinished) isAnimationFinished = true;
      //  animationController.SetBool(animationName, false);
    }
    public virtual void LogicUpdate()
    {
       TransitionChecks();
    }
    
    public virtual void PhysicsUpdate()
    {
    }
    // for checking transitions between states
    public virtual void TransitionChecks()
    {
    }
    // to trigger animation events when it finishes
    public virtual void AnimationTrigger()
    {
        isAnimationFinished = true;
    }
}

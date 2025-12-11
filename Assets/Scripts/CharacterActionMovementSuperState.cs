using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CharacterActionMovementSuperState : PlayerState
{
    // have the constructor like this so it knows what we are talking about once we use the states
    public CharacterActionMovementSuperState(Player PlayerCharacter, string animationName, PlayerStateMachine stateMachine) : base(PlayerCharacter, stateMachine, PlayerCharacter.animationController, animationName) { }
    protected Vector2 movementInput;
    protected bool isGrounded;
    float actionDuration =1f;
    float actionStartTime;
    public override void Enter()
    {
        base.Enter();
        actionStartTime = 0f;
        // play animation
    }

    public override void Exit()
    {
        base.Exit();
        // stop animation
    }

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        //assign action start time to world time
        actionStartTime += Time.deltaTime;

               if(player.MoveState == Player.MovementState.stinger)
               {
                 //player.animationController.SetBool("Stinger", true);
                  player.StingerMove();
                  return;
               }


               if(player.MoveState == Player.MovementState.laucher)
               {
                  player.AirLauncher();
                  return;
                  }
    

    }

    //For Idle use
    public override void TransitionChecks()
    {
        base.TransitionChecks();
        if (actionStartTime >= actionDuration) // assuming action lasts 1 second
        {
           

            stateMachine.ChangeState(new MovementSuperState(player, "isMoving", stateMachine));
        }
    }

}


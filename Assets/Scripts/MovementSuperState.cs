using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementSuperState : PlayerState
{
    // have the constructor like this so it knows what we are talking about once we use the states
    public MovementSuperState(Player pc, string animationName, PlayerStateMachine stateMachine) : base(pc, stateMachine, pc.animationController, animationName) { }
    protected Vector2 movementInput;
    protected bool isGrounded;

    public override void LogicUpdate()
    {
        base.LogicUpdate();
        player.Movement();
    }

    //For Move use
    public override void TransitionChecks()
    {
        base.TransitionChecks();

        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
          
            player.MoveState = Player.MovementState.stinger;
            stateMachine.ChangeState(new CharacterActionMovementSuperState(player, "isUsingActionMove", stateMachine));
       
            return;
        }
   
        if (Input.GetButtonDown("Air Launcher"))
        {
         player.MoveState = Player.MovementState.laucher;
            stateMachine.ChangeState(new CharacterActionMovementSuperState(player, "isUsingActionMove", stateMachine));
        
            return;
        }


    }



}

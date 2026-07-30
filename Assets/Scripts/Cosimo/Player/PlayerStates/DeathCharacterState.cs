using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Handles the Death State.
/// </summary>
public class DeathCharacterState : IStateCollision2D
{

    private Player _owner;
    private PlayerController _ownerController;
    private float _deathDuration = 1.5f;
    private float _timer;
    private Animator _animator;

    public DeathCharacterState(Player player, PlayerController controller, Animator animator)
    {
        _owner = player;
        _ownerController = controller;
        _animator = animator;
    }

    public void OnEnd()
    {
        _owner.PlayerController.Rb.simulated = true;
    }

    public void OnFixedUpdate()
    {

    }

    public void OnStart()
    {
        _ownerController.Rb.linearVelocity = Vector2.zero;
        _owner.PlayerController.Rb.simulated = false;

        // _ownerController.InputActions.Player.Disable();
        InputConfigManager.UnregisterConfig(_ownerController.PlayerConfig);

        _owner.Animator.SetBool("IsAlive", false);
        _owner.Animator.Play(_owner.DeathSettings.clipName);
        _timer = 0;

        // Debug.Log("Animazione di morte playata");
    }

    public void OnUpdate()
    {
        _timer += Time.deltaTime;

        if (_timer >= _deathDuration)
        {
            _owner.Respawn();
            _owner.SetState(ECharacterStates.Idle);
        }

    }

    public void OnCollisionEnter2D(Collision2D collision)
    {

    }

    public void OnCollisionExit2D(Collision2D collision)
    {

    }

    public void OnCollisionStay2D(Collision2D collision)
    {

    }

    public void OnTriggerEnter2D(Collider2D collider)
    {

    }

    public void OnTriggerStay2D(Collider2D collider)
    {

    }

    public void OnTriggerExit2D(Collider2D collider)
    {

    }
}
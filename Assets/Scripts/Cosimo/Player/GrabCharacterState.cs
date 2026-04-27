using UnityEngine;
using UnityEngine.Tilemaps;

internal class GrabCharacterState : IStateCollision2D
{
    private Player _owner;
    private PlayerController _ownerController;
    private GameObject _torch;
    private Tilemap _tilemap;

    public GrabCharacterState(Player player, PlayerController controller, GameObject torch, Tilemap tilemap)
    {
        
        _owner= player;
        _ownerController = controller;
        _torch = torch;
        _tilemap = tilemap;
        

    }
    public void OnCollisionEnter2D(Collision2D collision)
    {
        throw new System.NotImplementedException();
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        throw new System.NotImplementedException();
    }

    public void OnCollisionStay2D(Collision2D collision)
    {
        throw new System.NotImplementedException();
    }

    public void OnEnd()
    {
        throw new System.NotImplementedException();
    }

    public void OnFixedUpdate()
    {
        throw new System.NotImplementedException();
    }

    public void OnStart()
    {
        throw new System.NotImplementedException();
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        throw new System.NotImplementedException();
    }

    public void OnTriggerExit2D(Collider2D collider)
    {
        throw new System.NotImplementedException();
    }

    public void OnTriggerStay2D(Collider2D collider)
    {
        throw new System.NotImplementedException();
    }

    public void OnUpdate()
    {
        throw new System.NotImplementedException();
    }
}
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
        
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        
    }

    public void OnCollisionStay2D(Collision2D collision)
    {

    }

    public void OnEnd()
    {
    }

    public void OnFixedUpdate()
    {
    
    }

    public void OnStart()
    {
       
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        
    }

    public void OnTriggerExit2D(Collider2D collider)
    {
    }

    public void OnTriggerStay2D(Collider2D collider)
    {
        
    }

    public void OnUpdate()
    {
        
    }
}
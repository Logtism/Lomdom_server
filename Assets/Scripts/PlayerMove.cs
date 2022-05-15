using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RiptideNetworking;
using RiptideNetworking.Utils;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private CharacterController cc;
    [SerializeField] private Transform camProxy;
    [SerializeField] private float gravity;
    [SerializeField] private float movementspeed;
    [SerializeField] private float jumpheight;

    private float gravityAcceleration;
    private float movespeed;
    private float jumpspeed;

    private bool[] inputs;
    private float yvel;


    private void Start()
    {
        cc = GetComponent<CharacterController>();
        player = gameObject.GetComponent<Player>();

        inputs = new bool[6];

        gravityAcceleration = gravity * Time.fixedDeltaTime * Time.fixedDeltaTime;
        movespeed = movementspeed * Time.fixedDeltaTime;
        jumpspeed = Mathf.Sqrt(jumpheight * -2f * gravityAcceleration);
    }

    private void FixedUpdate()
    {
        Vector2 InputDirection = Vector2.zero;
        if (inputs[0])
        {
            InputDirection.y += 1;
        }
        if (inputs[1])
        {
            InputDirection.y -= 1;
        }
        if (inputs[2])
        {
            InputDirection.x -= 1;
        }
        if (inputs[3])
        {
            InputDirection.x += 1;
        }

        Move(InputDirection, inputs[4], inputs[5]);
    }

    private void Move(Vector2 InputDirection, bool jump, bool sprint)
    {
        Vector3 MoveDirection = Vector3.Normalize(camProxy.right * InputDirection.x + Vector3.Normalize(FlattenVector3(camProxy.forward)) * InputDirection.y);
        MoveDirection *= movespeed;

        if (sprint)
        {
            MoveDirection *= 2;
        }

        if (cc.isGrounded)
        {
            yvel = 0;
            if (jump)
            {
                yvel = jumpspeed;
            }
        }
        yvel += gravityAcceleration;

        MoveDirection.y = yvel;
        cc.Move(MoveDirection);

        SendMove();
    }

    private Vector3 FlattenVector3(Vector3 vector)
    {
        vector.y = 0;
        return vector;
    }

    public void SetInputs(bool[] inputs, Vector3 forward)
    {
        this.inputs = inputs;
        camProxy.forward = forward;
    }

    private void SendMove()
    {
        if (NetworkManager.Singleton.CurrentTick % 2 != 0)
        {
            return;
        }
        Message message = Message.Create(MessageSendMode.unreliable, Messages.STC.playermove);
        message.AddUShort(player.ClientID);
        message.AddUInt(NetworkManager.Singleton.CurrentTick);
        message.AddVector3(transform.position);
        message.AddVector3(camProxy.forward);
        NetworkManager.Singleton.Server.SendToAll(message);
    }
}

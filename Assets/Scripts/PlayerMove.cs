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

        if (player.InVehicle)
        {
            if (player.vehicle_driver)
            {
                VehicleMove(InputDirection, inputs[4], inputs[5]);
            }
            else
            {

            }
        }
        else
        {
            Move(InputDirection, inputs[4], inputs[5]);
        }
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

    private void VehicleMove(Vector2 InputDirection, bool jump, bool sprint)
    {
        float CurrentAcceleration;
        float CurrentBreakForce;
        float CurrentTurnAngle;
        CurrentAcceleration = player.vehicle.Vehicle_Data.Acceleration * InputDirection.y;

        if (jump)
        {
            CurrentBreakForce = player.vehicle.Vehicle_Data.BrakingForce;
        }
        else
        {
            CurrentBreakForce = 0f;
        }

        player.vehicle.BackLeft.motorTorque = CurrentAcceleration;
        player.vehicle.BackRight.motorTorque = CurrentAcceleration;

        player.vehicle.FrontLeft.brakeTorque = CurrentBreakForce;
        player.vehicle.FrontRight.brakeTorque = CurrentBreakForce;
        player.vehicle.BackLeft.brakeTorque = CurrentBreakForce;
        player.vehicle.BackRight.brakeTorque = CurrentBreakForce;

        CurrentTurnAngle = player.vehicle.Vehicle_Data.MaxTurnAngle * InputDirection.x;
        player.vehicle.FrontLeft.steerAngle = CurrentTurnAngle;
        player.vehicle.FrontRight.steerAngle = CurrentTurnAngle;

        SendMoveVehicle();
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

    private void SendMoveVehicle()
    {
        if (NetworkManager.Singleton.CurrentTick % 2 != 0)
        {
            return;
        }
        Message message = Message.Create(MessageSendMode.unreliable, Messages.STC.vehicle_move);
        message.AddUShort(player.ClientID);
        message.AddUInt(NetworkManager.Singleton.CurrentTick);
        message.AddVector3(player.vehicle.gameObject.transform.position);
        message.AddQuaternion(player.vehicle.transform.rotation);
        message.AddBool(true);
        NetworkManager.Singleton.Server.SendToAll(message);
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
        message.AddBool(false);
        NetworkManager.Singleton.Server.SendToAll(message);
    }
}

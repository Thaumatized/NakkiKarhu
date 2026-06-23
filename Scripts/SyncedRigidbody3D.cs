using Godot;
using System;

public partial class SyncedRigidbody3D : RigidBody3D
{
	[Rpc]
	public void networkedPosition(Vector3 position, Vector3 rotation)
	{
		Position = position;
		Rotation = rotation;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (Multiplayer.GetUniqueId() == 1)
		{
			if (Position.Y < -20)
			{
				Position = Vector3.Up * 20;
				LinearVelocity = Vector3.Zero;
				AngularVelocity = Vector3.Zero;
			}

			Rpc(MethodName.networkedPosition, Position, Rotation);
		}
	}
}

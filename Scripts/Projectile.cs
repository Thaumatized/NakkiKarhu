using System;
using Godot;

public partial class Projectile : RigidBody3D
{
	public float force = 10f;
	public float verticalForce = 10f;

	public Vector3 lastVelocity;

	public override void _Ready()
	{
		this.BodyEntered += onHit;
		this.ContactMonitor = true;
		this.MaxContactsReported = 1;
		lastVelocity = this.LinearVelocity;
	}

	public void onHit(Node body)
	{
		try
		{
			Player player = (Player)body;
			//player.Velocity += (player.Position - this.Position).Normalized() * force + Vector3.Up * verticalForce
			player.Velocity += this.lastVelocity.Normalized() * force + Vector3.Up * verticalForce;
		}
		catch { }
		this.BodyEntered -= onHit;
		this.SetCollisionMaskValue(2, false);
	}

	[Rpc]
	public void networkedPosition(Vector3 position, Vector3 rotation)
	{
		Position = position;
		Rotation = rotation;
	}

	public override void _PhysicsProcess(double delta)
	{
		lastVelocity = this.LinearVelocity;
		if (Multiplayer.GetUniqueId() == 1)
		{
			Rpc(MethodName.networkedPosition, Position, Rotation);
		}
	}
}

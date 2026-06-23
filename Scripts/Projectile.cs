using System;
using Godot;

public partial class Projectile : RigidBody3D
{
	public float force = 10f;
	public float verticalForce = 10f;

	public override void _Ready()
	{
		this.BodyEntered += onHit;
		this.ContactMonitor = true;
		this.MaxContactsReported = 1;
	}

	public void onHit(Node body)
	{
		GD.Print($"HIT {body.GetPath()}");
		try
		{
			Player player = (Player)body;
			GD.Print($"{body.GetPath()} was player");
			//player.Velocity += (player.Position - this.Position).Normalized() * force + Vector3.Up * verticalForce
			player.Velocity +=
				this.LinearVelocity.Normalized() * force + Vector3.Up * verticalForce;
		}
		catch { }
		this.BodyEntered -= onHit;
	}
}

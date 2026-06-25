using System;
using Godot;

public partial class Projectile : RigidBody3D
{
	public float force = 10f;
	public float verticalForce = 10f;

	[Export]
	public Vector3 lastVelocity;

	[Export]
	public long playerId;

	public double maxLife = 120;
	public double life = 0;
	public double shooterExceptionDuration = 0.5;

	public Player shooter;

	public override void _Ready()
	{
		GD.Print($"PROJECTILE SHOOTER ID {playerId}");

		shooter =  GameManager.instance.GetPlayer(playerId);
		AddCollisionExceptionWith(shooter.characterBody);
		this.BodyEntered += onHit;
		this.ContactMonitor = true;
		this.MaxContactsReported = 1;
		lastVelocity = this.LinearVelocity;
	}

	public void onHit(Node body)
	{
		GD.Print( $"PROJECTILE HIT {body.Name}");
		try
		{
			Player player = (Player)body.GetParent();
			//player.Velocity += (player.Position - this.Position).Normalized() * force + Vector3.Up * verticalForce
			player.characterBody.Velocity +=
				this.lastVelocity.Normalized() * force + Vector3.Up * verticalForce;
		}
		catch { }
		this.BodyEntered -= onHit;
		this.SetCollisionMaskValue(2, false);
	}

	public override void _PhysicsProcess(double delta)
	{
		lastVelocity = this.LinearVelocity;
		life += delta;

		if (life > shooterExceptionDuration)
		{
			RemoveCollisionExceptionWith(shooter.characterBody);
		}

		if (Multiplayer.GetUniqueId() == 1)
		{
			if (life > maxLife)
			{
				QueueFree();
			}
		}
	}
}

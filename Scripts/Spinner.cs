using System;
using Godot;

public partial class Spinner : AnimatableBody3D
{
	[Export]
	public float rpm = 6;

	[Export]
	MultiplayerSynchronizer synchronizer;

	public override void _Ready()
	{
		/*
		synchronizer.ReplicationConfig.AddProperty(synchronizer.GetPathTo(this) + ":rotation");
		synchronizer.ReplicationConfig.PropertySetReplicationMode(synchronizer.GetPathTo(this) + ":rotation", SceneReplicationConfig.ReplicationMode.Always);

		base._Ready();
		*/
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		if(Multiplayer.IsServer())
		{
		this.RotationDegrees += Vector3.Up * (float)(rpm / 60f * delta * 360f);
		}
	}
}

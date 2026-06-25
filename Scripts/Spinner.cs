using System;
using Godot;

public partial class Spinner : AnimatableBody3D
{
	[Export]
	public float rpm = 6;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _PhysicsProcess(double delta)
	{
		this.RotationDegrees += Vector3.Up * (float)(rpm / 60f * delta * 360f);
	}
}

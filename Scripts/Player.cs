using Godot;
using System;

public partial class Player : CharacterBody3D
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;

	public Camera3D camera;


	public override void _Ready()
	{
		camera = this.GetNode("Camera3D") as Camera3D;
		base._Ready();
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Input(InputEvent @event)
	{
		if(@event.IsActionPressed("click"))
			Input.MouseMode = Input.MouseModeEnum.Captured;
		if(@event.IsActionPressed("ui_cancel"))
			Input.MouseMode = Input.MouseModeEnum.Visible;
		if (Input.MouseMode != Input.MouseModeEnum.Captured)
			return;		

		if(@event.GetClass() == "InputEventMouseMotion")
		{
			InputEventMouseMotion mouseMotion = (InputEventMouseMotion)@event;

			Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
			Vector2 fov;
			if(camera.KeepAspect == Camera3D.KeepAspectEnum.Height)
			{
				fov = new Vector2(camera.Fov * viewportSize.X / viewportSize.Y, camera.Fov);
			}
			else
			{
				fov = new Vector2(camera.Fov, camera.Fov * viewportSize.Y / viewportSize.X);
			}
			
			camera.RotationDegrees += Vector3.Left * mouseMotion.Relative.Y / viewportSize.Y * fov.Y;
			this.RotationDegrees += Vector3.Down * mouseMotion.Relative.X / viewportSize.X * fov.X;
		}


		base._Input(@event);
	}

	public override void _PhysicsProcess(double delta)
	{
		Vector3 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		// Handle Jump.
		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		// Get the input direction and handle the movement/deceleration.
		// As good practice, you should replace UI actions with custom gameplay actions.
		Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
	}
}

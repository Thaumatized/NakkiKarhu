using Godot;
using System;

public class player : KinematicBody
{
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;

	public Camera camera;

	Vector3 velocity = Vector3.Zero;

	public override void _Ready()
	{
		camera = this.GetNode<Camera>("Camera");
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
			if(camera.KeepAspect == Camera.KeepAspectEnum.Height)
			{
				fov = new Vector2(camera.Fov * viewportSize.x / viewportSize.y, camera.Fov);
			}
			else
			{
				fov = new Vector2(camera.Fov, camera.Fov * viewportSize.y / viewportSize.x);
			}
			
			camera.RotationDegrees += Vector3.Left * mouseMotion.Relative.y / viewportSize.y * fov.y;
			this.RotationDegrees += Vector3.Down * mouseMotion.Relative.x / viewportSize.x * fov.x;
		}


		base._Input(@event);
	}

	public override void _PhysicsProcess(float delta)
	{
		// Jumping
		// IsOnFloor() appers to be broken on Godot3
		// if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		if (Input.IsActionJustPressed("jump") && velocity.y == 0)
		{
			velocity.y = JumpVelocity;
		}
		else
		{
			// Gravity
			velocity += Vector3.Down * (float)ProjectSettings.GetSetting("physics/3d/default_gravity") * delta;
		}

		//Vector2 inputDir = Input.GetVector("moveLeft", "moveRight", "moveForward", "moveBackward");
		Vector2 inputDir = new Vector2(
			(Input.IsActionPressed("moveLeft") ? -1 : 0) +
			(Input.IsActionPressed("moveRight") ? 1 : 0),
			(Input.IsActionPressed("moveForward") ? -1 : 0) + 
			(Input.IsActionPressed("moveBackward") ? 1 : 0)
		);
		Vector3 direction = Transform.basis * (new Vector3(inputDir.x, 0, inputDir.y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.x = direction.x * Speed;
			velocity.z = direction.z * Speed;
		}
		else
		{
			velocity.x = Mathf.MoveToward(velocity.x, 0, Speed);
			velocity.z = Mathf.MoveToward(velocity.z, 0, Speed);
		}

		velocity = MoveAndSlide(velocity);
	}
}

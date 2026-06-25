using System;
using System.ComponentModel;
using Godot;

public partial class Player : CharacterBody3D
{
	public PlayerInfo playerInfo;

	[EditorBrowsable]
	public const float Speed = 5.0f;
	public const float JumpVelocity = 7f;

	public Camera3D camera;

	public int shotsfired = 0;

	public override void _Ready()
	{
		GD.Print($"{playerInfo.id} {playerInfo.name}");

		camera = this.GetNode<Camera3D>("Camera3D");
		camera.Current = playerInfo.id == Multiplayer.GetUniqueId();
		if (playerInfo.id == Multiplayer.GetUniqueId())
		{
			this.GetNode<MeshInstance3D>("MeshInstance3D").CastShadow = MeshInstance3D
				.ShadowCastingSetting
				.ShadowsOnly;
			this.GetNode("MeshInstance3D").GetNode<MeshInstance3D>("MeshInstance3D").CastShadow =
				MeshInstance3D.ShadowCastingSetting.ShadowsOnly;
			this.GetNode<Label3D>("Label3D").Text = "";
			Input.MouseMode = Input.MouseModeEnum.Captured;
		}
		else
		{
			this.GetNode<Label3D>("Label3D").Text = playerInfo.name;
		}

		base._Ready();
	}

	public override void _Input(InputEvent @event)
	{
		if (playerInfo.id != Multiplayer.GetUniqueId())
			return;

		if (@event.IsActionPressed("click"))
			Input.MouseMode = Input.MouseModeEnum.Captured;
		if (@event.IsActionPressed("ui_cancel"))
			Input.MouseMode = Input.MouseModeEnum.Visible;
		if (Input.MouseMode != Input.MouseModeEnum.Captured)
			return;

		if (@event.GetClass() == "InputEventMouseMotion")
		{
			InputEventMouseMotion mouseMotion = (InputEventMouseMotion)@event;

			Vector2 viewportSize = GetViewport().GetVisibleRect().Size;
			Vector2 fov;
			if (camera.KeepAspect == Camera3D.KeepAspectEnum.Height)
			{
				fov = new Vector2(camera.Fov * viewportSize.X / viewportSize.Y, camera.Fov);
			}
			else
			{
				fov = new Vector2(camera.Fov, camera.Fov * viewportSize.Y / viewportSize.X);
			}

			camera.RotationDegrees = new Vector3(
				Math.Clamp(
					camera.RotationDegrees.X - (mouseMotion.Relative.Y / viewportSize.Y * fov.Y),
					-90,
					90
				),
				0,
				0
			);
			this.RotationDegrees += Vector3.Down * mouseMotion.Relative.X / viewportSize.X * fov.X;
		}

		base._Input(@event);
	}

	[Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
	public void shoot(Vector3 euler, Vector3 spin, int shotId)
	{
		RigidBody3D projectile = ResourceLoader
			.Load<PackedScene>("res://Scenes/projectile.tscn")
			.Instantiate<RigidBody3D>();
		GameManager.dynamicParent.AddChild(projectile);
		projectile.Name = $"projectile_{playerInfo.id}_{shotId}";

		Basis basis = Basis.FromEuler(euler);
		projectile.GlobalPosition = camera.GlobalPosition + (basis * Vector3.Forward);
		projectile.GlobalRotation = euler;
		projectile.LinearVelocity = projectile.Basis * Vector3.Forward * 25;
		projectile.AngularVelocity = spin;

		((Projectile)projectile).shooter = this;
		AddCollisionExceptionWith(projectile);
	}

	[Rpc]
	public void networkedPosition(Vector3 position, Vector3 rotation)
	{
		Position = position;
		Rotation = rotation;
	}

	public override void _PhysicsProcess(double delta)
	{

		if (playerInfo.id == Multiplayer.GetUniqueId())
		{
			if (Input.IsActionJustPressed("click"))
			{
				Random random = new Random();
				Vector3 spin = new Vector3(
					(((float)random.Next()) / Int32.MaxValue - 0.5f) * 20,
					(((float)random.Next()) / Int32.MaxValue - 0.5f) * 20,
					(((float)random.Next()) / Int32.MaxValue - 0.5f) * 20
				);
				Rpc(MethodName.shoot, camera.GlobalRotation, spin, shotsfired++);
			}

			Vector3 velocity = Velocity;

			velocity += GetGravity() * (float)delta;
			if (Input.IsActionJustPressed("jump") && IsOnFloor())
			{
				velocity.Y = JumpVelocity;
			}

			float acceleration = 1;
			if (!IsOnFloor())
			{
				acceleration *= 0.1f;
			}

			Vector2 inputDir = Input.GetVector(
				"moveLeft",
				"moveRight",
				"moveForward",
				"moveBackward"
			);
			Vector3 target =
				(Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized() * Speed;

			velocity.X = Mathf.MoveToward(Velocity.X, target.X, acceleration);
			velocity.Z = Mathf.MoveToward(Velocity.Z, target.Z, acceleration);

			Velocity = velocity;
			MoveAndSlide();

			this.Rotation += Vector3.Up * this.GetPlatformAngularVelocity().Y * (float)delta;

			if (Position.Y < -20)
			{
				Position = Vector3.Up * 20;
			}

			Rpc(MethodName.networkedPosition, Position, Rotation);
		}
	}
}

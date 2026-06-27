using System;
using Godot;

public partial class Player : Node
{
	[Export]
	public int playerId;

	public float Speed = 5.0f;
	public float projectileSpeed = 15f;

	public float JumpVelocity = 7f;
	public Camera3D camera;

	public int shotsfired = 0;
	public CharacterBody3D characterBody;

	[Export]
	public Vector3 networkedVelocity;

	public override void _EnterTree()
	{
		playerId = int.Parse(Name);
		GD.Print($"Player _EnterTree {playerId.ToString()}");
		SetMultiplayerAuthority(playerId);

		characterBody = GetNode<CharacterBody3D>("CharacterBody3D");
	}

	public override void _Ready()
	{
		GD.Print(
			$"Player _Ready {playerId} == {Multiplayer.GetUniqueId()} => {playerId == Multiplayer.GetUniqueId()}"
		);
		camera = characterBody.GetNode<Camera3D>("Camera3D");
		camera.Current = playerId == Multiplayer.GetUniqueId();
		GD.Print(camera.Current);
		if (playerId == Multiplayer.GetUniqueId())
		{
			characterBody.GetNode<MeshInstance3D>("MeshInstance3D").CastShadow = MeshInstance3D
				.ShadowCastingSetting
				.ShadowsOnly;
			characterBody
				.GetNode("MeshInstance3D")
				.GetNode<MeshInstance3D>("MeshInstance3D")
				.CastShadow = MeshInstance3D.ShadowCastingSetting.ShadowsOnly;
			characterBody.GetNode<Label3D>("Label3D").Text = "";
		}
		else
		{
			characterBody.GetNode<Label3D>("Label3D").Text = "";
		}
	}

	public void setPlayerName(string name)
	{
		characterBody.GetNode<Label3D>("Label3D").Text = name;
	}

	public override void _Input(InputEvent @event)
	{
		if (playerId != Multiplayer.GetUniqueId())
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
			characterBody.RotationDegrees +=
				Vector3.Down * mouseMotion.Relative.X / viewportSize.X * fov.X;
		}

		base._Input(@event);
	}

	[Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true)]
	public void shoot(Vector3 euler, int shotId)
	{
		if (Multiplayer.IsServer())
		{
			GD.Print($"SHOOT {playerId}");

			Random random = new Random();
			Vector3 spin = new Vector3(
				(((float)random.Next()) / Int32.MaxValue - 0.5f) * 20,
				(((float)random.Next()) / Int32.MaxValue - 0.5f) * 20,
				(((float)random.Next()) / Int32.MaxValue - 0.5f) * 20
			);

			RigidBody3D projectile = ResourceLoader
				.Load<PackedScene>("res://Scenes/projectile.tscn")
				.Instantiate<RigidBody3D>();

			projectile.Name = $"projectile_{playerId}_{shotId}";

			((Projectile)projectile).playerId = playerId;

			GameManager.instance.projectileContainer.AddChild(projectile);

			Basis basis = Basis.FromEuler(euler);
			projectile.GlobalPosition = camera.GlobalPosition + (basis * Vector3.Forward);
			projectile.GlobalRotation = euler;
			projectile.LinearVelocity = projectile.Basis * Vector3.Forward * projectileSpeed;
			projectile.AngularVelocity = spin;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (playerId == Multiplayer.GetUniqueId())
		{
			if (Input.IsActionJustPressed("click"))
			{
				RpcId(1, MethodName.shoot, camera.GlobalRotation, shotsfired++);
			}

			Vector3 velocity = characterBody.Velocity;

			velocity += characterBody.GetGravity() * (float)delta;
			if (Input.IsActionJustPressed("jump") && characterBody.IsOnFloor())
			{
				velocity.Y = JumpVelocity;
			}

			float acceleration = 1;
			if (!characterBody.IsOnFloor())
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
				(
					characterBody.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)
				).Normalized() * Speed;

			velocity.X = Mathf.MoveToward(characterBody.Velocity.X, target.X, acceleration);
			velocity.Z = Mathf.MoveToward(characterBody.Velocity.Z, target.Z, acceleration);

			characterBody.Velocity = velocity;
			characterBody.MoveAndSlide();
			networkedVelocity = velocity;

			characterBody.Rotation +=
				Vector3.Up * characterBody.GetPlatformAngularVelocity().Y * (float)delta;

			if (characterBody.Position.Y < -20)
			{
				Random random = new Random();
				characterBody.Position = new Vector3(
					random.Next() % 10,
					random.Next() % 10 + 20,
					random.Next() % 10
				);
			}
		}
		else
		{
			characterBody.Velocity = networkedVelocity;
			characterBody.MoveAndSlide();
		}
	}
}

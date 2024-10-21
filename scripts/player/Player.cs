using Godot;

public partial class Player : CharacterBody3D
{
	private const float Speed = 5.0f;
	private const float JumpVelocity = 4.5f;

	[Export] private Node3D head;
	[Export] private Camera3D camera;
	[Export] private float sensitivity = 0.03f;

    public override void _Ready()
    {
		Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
		if (@event is InputEventMouseMotion eventMouseMotion)
		{
			head.RotateY(-eventMouseMotion.Relative.X * sensitivity * 0.01f);
			camera.RotateX(-eventMouseMotion.Relative.Y * sensitivity * 0.01f);
			camera.Rotation = new Vector3(Mathf.Clamp(camera.Rotation.X, Mathf.DegToRad(-90), Mathf.DegToRad(90)), camera.Rotation.Y, camera.Rotation.Z);
		}
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
		Vector3 direction = (head.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
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
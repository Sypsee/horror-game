using Godot;

public partial class Player : CharacterBody3D
{
	[Export] private float BaseSpeed = 5.0f;
	[Export] private float SprintSpeed = 8.0f;
	[Export] private float JumpVelocity = 5.5f;
	[Export] private float MaxStamina = 50.0f;
	[Export] private float StaminaReducePerSecond = 30.0f;
	[Export] private float StaminaIncreasePerSecond = 1.5f;

	private float _speed = 0.0f;
	private float _stamina = 0.0f;

	private const float bob_freq = 2.0f;
	private const float bob_ampl = 0.08f;
	private double t_bob = 0.0f;

	[Export] private Node3D head;
	[Export] private Camera3D camera;
	[Export] private float sensitivity = 0.03f;

    public override void _Ready()
    {
		_speed = BaseSpeed;
		_stamina = MaxStamina;

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

		float staminaIncrease = _stamina - StaminaReducePerSecond * (float)delta;
		if (Input.IsActionPressed("Sprint") && staminaIncrease > 0.0f)
		{
			_speed = SprintSpeed;
			_stamina -= staminaIncrease;
		}
		else
		{
			_speed = BaseSpeed;
			if (_stamina < MaxStamina)
			{
				_stamina += StaminaIncreasePerSecond * (float)delta;
			}
		}

		_stamina = Mathf.Clamp(_stamina, 0.0f, MaxStamina);

		GD.Print("stamina -> " + _stamina);

		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}

		if (Input.IsActionJustPressed("ui_accept") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}

		Vector2 inputDir = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		Vector3 direction = (head.Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		if (IsOnFloor())
		{
			if (direction != Vector3.Zero)
			{
				velocity.X = direction.X * _speed;
				velocity.Z = direction.Z * _speed;
			}
			else
			{
				velocity.X = Mathf.Lerp(velocity.X, direction.X * _speed, (float)delta * 8.0f);
				velocity.Z = Mathf.Lerp(velocity.Z, direction.Z * _speed, (float)delta * 8.0f);
			}
		}
		else
		{
			velocity.X = Mathf.Lerp(velocity.X, direction.X * _speed, (float)delta * 3.0f);
			velocity.Z = Mathf.Lerp(velocity.Z, direction.Z * _speed, (float)delta * 3.0f);
		}

		t_bob += delta * velocity.Length() * (IsOnFloor() ? 1.0 : 0.0);
		camera.Transform = HeadBob(t_bob);

		Velocity = velocity;
		MoveAndSlide();
	}

    private Transform3D HeadBob(double time)
	{
		Transform3D transform = camera.Transform;
		Vector3 origin = Vector3.Zero;
		origin.Y = Mathf.Sin((float)time * bob_freq) * bob_ampl;
		origin.X = Mathf.Cos((float)time * bob_freq / 2) * bob_ampl;

		transform.Origin = origin;
		return transform;
	}
}
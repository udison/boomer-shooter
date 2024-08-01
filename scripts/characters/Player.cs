using Godot;
using System;

public partial class Player : Entity
{

	public override void _PhysicsProcess(double delta)
	{
		HandleInput();

		base._PhysicsProcess(delta);
	}

	private void HandleInput() {
		Vector2 inputDir = Input.GetVector("left", "right", "forward", "backward");
		motion = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
		GD.Print(motion);

		// Handle Jump
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			motion.Y = jumpVelocity;
		}
	}
}

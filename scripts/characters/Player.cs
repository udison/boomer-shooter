using Godot;
using System;

public partial class Player : Entity
{
	private static float MAX_HEAD_ROTATION = 89.0f;

	[ExportGroup("Mouse and Keyboard")]
	[Export(PropertyHint.Range, "0.01, 5.0, 0.1")] private float sensitivity = .1f;

	#region Nodes
	private PlayerHead head;
    #endregion

	private Vector2 lookInput = Vector2.Zero;
	private Weapon activeWeapon;

    public override void _Ready()
    {
        base._Ready();

		head = GetNode<PlayerHead>("Head");
    }

    public override void _Input(InputEvent @event)
    {
		if (@event is InputEventMouseMotion mouseEvent) {
        	Look(mouseEvent.Relative);
		}
    }

    public override void _PhysicsProcess(double delta)
	{
		HandleInput();

		base._PhysicsProcess(delta);
	}

	public Vector2 GetLookInput() {
		return lookInput;
	}

	private void HandleInput() {
		Vector2 inputDir = Input.GetVector("left", "right", "forward", "backward");
		motion = new Vector3(inputDir.X, 0, inputDir.Y);

		// Handle Jump
		if (Input.IsActionJustPressed("jump"))
		{
			Jump();
		}
	}

	private void Look(Vector2 direction) {
		lookInput = direction;

		// Rotates player
		RotateY(-Mathf.DegToRad(direction.X * sensitivity));

		// Rotates head
		head.RotateX(-Mathf.DegToRad(direction.Y * sensitivity));
		head.RotationDegrees = new Vector3(
			Mathf.Clamp(head.RotationDegrees.X, -MAX_HEAD_ROTATION, MAX_HEAD_ROTATION),
			head.Rotation.Y,
			head.Rotation.Z
		);
	}
}

using Godot;
using System;

public partial class PlayerHead : Node3D
{
	#region Nodes
	private Player player;
	private Camera3D camera;
	#endregion


	/// <summary>Enable or disable head bob</summary>
	[ExportCategory("Head Bob")]
	[Export] public bool enableHeadBob = true;
	private float time = 0;
	private float amplitude = 0.05f;
	private float frequency = 15;

	/// <summary>Enable or disable head bob</summary>
	[ExportCategory("Head Tilt")]
	[Export] public bool enableHeadTilt = true;
	private float headTiltAngle = 2.0f;

	public override void _Ready() {
		player = GetParent<Player>();
		camera = GetNode<Camera3D>("Camera");
	}

	public override void _PhysicsProcess(double delta)
	{
		HeadBob(delta);
		HeadTilt(delta);
	}

	private void HeadBob(double delta) {
		if (!enableHeadBob) return;

		float planarSpeed = player.GetPlanarMotion().Length();

		if (planarSpeed > 0) {
			time += (float)delta;

			float targetFrequency = frequency * (planarSpeed / player.GetMaxSpeed());

			camera.Position = new Vector3(
				Position.X,
				MathF.Sin(time * targetFrequency) * amplitude,
				Position.Z
			);
		}
		else {
			time = 0;
			camera.Position = camera.Position.Lerp(Vector3.Zero, (float)delta * 3);
			
			if (camera.Position.DistanceTo(Vector3.Zero) <= .001f) {
				camera.Position = Vector3.Zero;
			}
		}
	}

	private void HeadTilt(double delta) {
		if (!enableHeadTilt) return;

		camera.Rotation = new Vector3(
			camera.Rotation.X,
			camera.Rotation.Y,
			Mathf.Lerp(camera.Rotation.Z, -player.GetPlanarMotion().X * Mathf.DegToRad(headTiltAngle), (float)delta * 4.0f)
		);
	}
}

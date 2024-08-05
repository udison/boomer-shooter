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

	public override void _Ready() {
		player = GetParent<Player>();
		camera = GetNode<Camera3D>("Camera");
	}

	public override void _PhysicsProcess(double delta)
	{
		HeadBob(delta);
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
}

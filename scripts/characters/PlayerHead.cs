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
	private float amplitude = 0.1f;
	private float frequency = 5;

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

		if (player.GetPlanarMotion().Length() > 0) {
			time += (float)delta;

			Vector3 target = new Vector3(
				Position.X,
				MathF.Sin(time * frequency) * amplitude,
				Position.Z
			);

			camera.Position = camera.Position.Lerp(target, (float)delta);
		}
		else {
			time = 0;
			camera.Position = camera.Position.Lerp(Vector3.Zero, (float)delta);
		}
	}
}

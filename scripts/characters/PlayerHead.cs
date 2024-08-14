using Godot;
using System;

public partial class PlayerHead : Node3D
{
	#region Nodes
	private Player player;
	private Camera3D camera;
	private WeaponHolder weaponHolder;
	#endregion


	/// <summary>Enable or disable head bob</summary>
	[ExportCategory("Head Bob")]
	[Export] public bool enableHeadBob = true;
	private float time = 0;
	private float amplitude = 0.05f;
	private float frequency = 50;

	/// <summary>Enable or disable head bob</summary>
	[ExportCategory("Head Tilt")]
	[Export] public bool enableHeadTilt = true;
	private float headTiltAngle = 4.0f;

	[ExportCategory("Weapon Tilt")]
	[Export] public float weaponTiltMultiplier = .2f;
	[Export] public float weaponTiltVelocity = 7f;

	[ExportCategory("Weapon Sway")]
	[Export] public float swayRotation = -.005f;

	[ExportCategory("Weapon Bob")]
	[Export] public float weaponBobAmplitude = .01f;
	[Export] public float weaponBobFrequency = 15f;

	public override void _Ready() {
		player = GetParent<Player>();
		camera = GetNode<Camera3D>("Camera");
		weaponHolder = camera.GetNode<WeaponHolder>("WeaponHolder");
	}

	public override void _PhysicsProcess(double delta)
	{
		HeadBob(delta);
		HeadTilt(delta);
		WeaponTilt(delta);
		WeaponSway(delta);
		WeaponBob(delta);
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

	private void WeaponTilt(double delta) {
		Vector3 target = new Vector3(
			weaponHolder.Rotation.X,
			weaponHolder.Rotation.Y,
			Mathf.Lerp(
				weaponHolder.Rotation.Z,
				-player.GetPlanarMotion().Normalized().X * weaponTiltMultiplier,
				(float)delta * weaponTiltVelocity
			)
		);

		weaponHolder.Rotation = target;
	}

	private void WeaponSway(double delta) {
		Vector2 lookInput = player.GetLookInput().Lerp(Vector2.Zero, 10 * (float)delta);

		Vector3 target = new Vector3(
			Mathf.Lerp(weaponHolder.Rotation.X, lookInput.Y * swayRotation, (float)delta * 5.0f),
			Mathf.Lerp(weaponHolder.Rotation.Y, lookInput.X * swayRotation, (float)delta * 5.0f),
			weaponHolder.Rotation.Z
		);

		weaponHolder.Rotation = target;
	}

	private void WeaponBob(double delta) {
		// TODO: [OPTIMIZATION] calling this every physics frame is not good
		Weapon weapon = weaponHolder.GetActiveWeapon();

		if (weapon == null) return;

		float planarSpeed = player.GetPlanarMotion().Length();

		if (planarSpeed > 0) {
			float targetFrequency = weaponBobFrequency * (planarSpeed / player.GetMaxSpeed());

			weapon.Position = new Vector3(
				Position.X,
				MathF.Sin(time * targetFrequency) * weaponBobAmplitude,
				Position.Z
			);
		}
		else {
			weapon.Position = weapon.Position.Lerp(Vector3.Zero, (float)delta * 3);
			
			if (weapon.Position.DistanceTo(Vector3.Zero) <= .001f) {
				weapon.Position = Vector3.Zero;
			}
		}
	}

}

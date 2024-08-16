using Godot;
using System;

public partial class PlayerHead : Node3D
{
	#region Nodes
	private Player player;
	private Camera3D camera;
	private Node3D stancePosition;
	private WeaponHolder weaponHolder;
	private Node3D readyPosition;
	private Node3D safePosition;
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

	[ExportCategory("Weapon Bob")]
	[Export] public float aimVelocity = 5f;
	private bool isAiming = false;
	private Vector3 currentAimOffset = Vector3.Zero;

	public override void _Ready() {
		player = GetParent<Player>();
		camera = GetNode<Camera3D>("Camera");
		stancePosition = camera.GetNode<Node3D>("StancePosition");
		weaponHolder = stancePosition.GetNode<WeaponHolder>("WeaponHolder");
		readyPosition = camera.GetNode<Node3D>("ReadyPosition");
		safePosition = camera.GetNode<Node3D>("SafePosition");
	}

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("secondary")) {
			ToggleAim();
		}
    }

    public override void _PhysicsProcess(double delta)
	{
		HeadBob(delta);
		HeadTilt(delta);
		WeaponTilt(delta);
		WeaponSway(delta);
		WeaponBob(delta);
		HandleAiming(delta);
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

	private void ToggleAim() {
		// isAiming = !isAiming;

		if (!isAiming) {
			// Vector3 offset = ToLocal(weaponHolder.GetActiveWeapon().GetNode<Marker3D>("SightPosition").GlobalPosition);

			// isAiming = true;
			// currentAimOffset = offset;
			// weaponHolder.GlobalPosition -= offset;

			// 1. Set weapon position back to the original pos (weaponHolder.pos = Vec3.Zero)
			Weapon weapon = weaponHolder.GetActiveWeapon();
			weaponHolder.Position = Vector3.Zero;
			weaponHolder.Rotation = Vector3.Zero;
			weapon.Position = Vector3.Zero;
			weapon.Rotation = Vector3.Zero;
			
			// 2. Get the weapon aim position offset to camera (camera.ToLocal(something?))
			Vector3 offset = camera.ToLocal(weapon.GetNode<Marker3D>("SightPosition").GlobalTransform.Origin);

			// 3. Set the position
			isAiming = true;
			currentAimOffset = offset;
			stancePosition.Position -= offset;
		}
		else {
			isAiming = false;
			stancePosition.GlobalPosition = readyPosition.GlobalPosition;
			currentAimOffset = Vector3.Zero;
		}
		GD.Print(isAiming);
	}

	private void HandleAiming(double delta) {
		// if 
	}

}

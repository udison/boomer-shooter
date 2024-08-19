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
	private Node3D alternateReadyPosition;
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
	[Export] public float weaponTiltAimMultiplier = .2f;

	[ExportCategory("Weapon Sway")]
	[Export] public float swayRotation = -.005f;
	[Export] public float swayAimMultiplier = .2f;

	[ExportCategory("Weapon Bob")]
	[Export] public float weaponBobAmplitude = .01f;
	[Export] public float weaponBobFrequency = 30f;
	[Export] public float weaponBobAimMultiplier = .2f;

	/// <summary>Duration of the aiming transition in seconds</summary>
	[ExportCategory("Aiming")]
	[Export] public float aimDuration = .4f;
	private bool isAiming = false;
	private Tween activeStancePositionTween = null;
	private Tween activeStanceRotationTween = null;
	private Node3D activeFireStance;

	public override void _Ready() {
		player = GetParent<Player>();
		camera = GetNode<Camera3D>("Camera");
		stancePosition = camera.GetNode<Node3D>("StancePosition");
		weaponHolder = stancePosition.GetNode<WeaponHolder>("WeaponHolder");
		readyPosition = camera.GetNode<Node3D>("ReadyPosition");
		alternateReadyPosition = camera.GetNode<Node3D>("AlternateReadyPosition");
		safePosition = camera.GetNode<Node3D>("SafePosition");
		activeFireStance = readyPosition;
	}

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("secondary")) {
			ToggleAim();
		}

		if (Input.IsActionJustPressed("change_fire_stance")) {
			ChangeFireStance();
		}
    }

    public override void _PhysicsProcess(double delta)
	{
		HeadBob(delta);
		HeadTilt(delta);
		WeaponTilt(delta);
		WeaponSway(delta);
		WeaponBob(delta);
	}

	public WeaponHolder GetWeaponHolder() {
		return weaponHolder;
	}

	#region Head juice
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
			if (time > 0) time -= (float)delta;
			else time = 0;

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
	#endregion

	#region Weapon juice
	private void WeaponTilt(double delta) {
		Vector3 target = new Vector3(
			weaponHolder.Rotation.X,
			weaponHolder.Rotation.Y,
			Mathf.Lerp(
				weaponHolder.Rotation.Z,
				-player.GetPlanarMotion().Normalized().X * weaponTiltMultiplier * (isAiming ? weaponTiltAimMultiplier : 1),
				(float)delta * weaponTiltVelocity
			)
		);

		weaponHolder.Rotation = target;
	}

	private void WeaponSway(double delta) {
		Vector2 lookInput = player.GetLookInput().Lerp(Vector2.Zero, 10 * (float)delta);

		Vector3 target = new Vector3(
			Mathf.Lerp(weaponHolder.Rotation.X, lookInput.Y * swayRotation * (isAiming ? swayAimMultiplier : 1), (float)delta * 5.0f),
			Mathf.Lerp(weaponHolder.Rotation.Y, lookInput.X * swayRotation * (isAiming ? swayAimMultiplier : 1), (float)delta * 5.0f),
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
				MathF.Sin(time * targetFrequency) * weaponBobAmplitude * (isAiming ? weaponBobAimMultiplier : 1),
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
	#endregion

	#region Aiming
	private void ToggleAim() {
		// Set weapon position and rotation back to the original pos (so weapon sway, tilt and bob dont mess with aiming position)
		// TODO: #11 [IMPROVEMENT] This causes a very subtle flick, barely visible but it is there. 
		//       Would be better if some vector magic was done on offset calculation below instead.
		Weapon weapon = weaponHolder.GetActiveWeapon();
		weaponHolder.Position = Vector3.Zero;
		weaponHolder.Rotation = Vector3.Zero;
		weapon.Position = Vector3.Zero;
		weapon.Rotation = Vector3.Zero;

		if (!isAiming) {
			isAiming = true;

			// Offset from weapon aim position to camera center
			Vector3 offset = camera.ToLocal(weapon.GetNode<Marker3D>("SightPosition").GlobalTransform.Origin);
			TweenStance(stancePosition.Position - offset, Vector3.Zero, aimDuration);
		}
		else {
			isAiming = false;
			
			// Offset from weapon position to ready stance position
			TweenStance(activeFireStance.Position, activeFireStance.Rotation, aimDuration);
		}
	}

	private void ChangeFireStance() {
		if (activeFireStance == readyPosition) activeFireStance = alternateReadyPosition;
		else if (activeFireStance == alternateReadyPosition) activeFireStance = safePosition;

		// TODO: Make safePosition active by holding B instead of cycling to it
		else if (activeFireStance == safePosition) activeFireStance = readyPosition;

		TweenStance(activeFireStance.Position, activeFireStance.Rotation, aimDuration);
	}

	private void TweenStance(
		Vector3 targetPosition,
		Vector3 targetRotation,
		float duration,
		Tween.TransitionType transition = Tween.TransitionType.Circ,
		Tween.EaseType easing = Tween.EaseType.Out
	) {
		if (activeStancePositionTween != null && activeStancePositionTween.IsRunning()) {
			activeStancePositionTween.Kill();
			activeStanceRotationTween.Kill();
		}

		// Important note: stance tweening MUST NOT use global position, always local
		activeStancePositionTween = GetTree().CreateTween();
		activeStancePositionTween
			.TweenProperty(stancePosition, "position", targetPosition, duration)
			.SetTrans(transition)
			.SetEase(easing);
		
		activeStanceRotationTween = GetTree().CreateTween();
		activeStanceRotationTween
			.TweenProperty(stancePosition, "rotation", targetRotation, duration)
			.SetTrans(transition)
			.SetEase(easing);
	}
	#endregion

}

using Godot;
using System;
using System.Collections.Generic;

public partial class Weapon : Node3D
{
	[Export] protected string displayName = "";
	[Export] protected float damage = 10.0f;
	[Export(PropertyHint.Range, "0,2000,1")] protected float fireRate = 550.0f; // rounds/attacks per minute
	[Export(PropertyHint.Range, "0,2000,1")] protected float range = 1000.0f; // rounds/attacks per minute
	[Export] protected int clipSize = 16;
	[Export] protected int remainingAmmo = 320;
	[Export] protected PackedScene casingParticle;
	[Export] protected PackedScene magazineScene;

	protected EWeaponState state = EWeaponState.DRAWING;
	protected int rounds;
	protected static PackedScene DEBUG_HIT_MARKER = GD.Load<PackedScene>("res://scenes/debug/debug_hit_marker.tscn");
	protected static int DEBUG_HIT_MARKER_LIFETIME = 5; // seconds

	#region Nodes
	protected AnimationPlayer animationPlayer;
	protected Timer fireRateTimer;
	protected Node3D casingShute;
	protected Node3D attackPoint;
	protected Node3D magazinePosition;
	protected Magazine currentMag;
    #endregion

	#region Lifecycle
    public override void _Ready() {
		rounds = clipSize;
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        casingShute = GetNode<Node3D>("CasingShute");
		attackPoint = GetNode<Node3D>("AttackPoint");
		magazinePosition = GetNode<Node3D>("MagazinePosition");
		InstantiateMagazine();
        
		fireRateTimer = GetNode<Timer>("FireRateTimer");
		fireRateTimer.WaitTime = 60 / fireRate;
		fireRateTimer.Timeout += OnAttackEnd;

		PlayAnimation(EWeaponAnimation.READY);
    }

    public override void _Input(InputEvent @event) {
		// TODO: Move this input checks to player and comunicate by signals
        if (Input.IsActionJustPressed("primary")) {
			Attack();
		}
		
        if (Input.IsActionJustPressed("reload")) {
			StartReload();
		}
    }
    #endregion

	#region Animation
    protected void SetState(EWeaponState newState) {
		state = newState;
	}

    protected void PlayAnimation(EWeaponAnimation anim) {
		if (animationPlayer == null) {
			GD.PrintErr("[Weapon] AnimationPlayer does not exists! (" + displayName + ")");
			return;
		}

		string animName = anim.ToString();

		if (!animationPlayer.HasAnimation(animName)) {
			GD.PrintErr("[Weapon] Trying to play animation that does not exists! (" + anim + ")");
			return;
		}

		animationPlayer.Play("RESET");
		animationPlayer.Play(animName);
	}
	#endregion

	#region Attack
	public virtual void Attack() {
		if (state == EWeaponState.EMPTY) {
			PlayAnimation(EWeaponAnimation.EMPTY);
		}

		if (state != EWeaponState.READY) return;

		SetState(EWeaponState.ATTACKING);
		fireRateTimer.Start();
		CastAttackRay();
		CheckRounds();
	}

	protected virtual void CheckRounds() {
		rounds--;

		if (rounds <= 0) {
			SetState(EWeaponState.EMPTY);
			PlayAnimation(EWeaponAnimation.ATTACK_TO_EMPTY);
			GD.Print("State set to " + state);
		}
		else {
			PlayAnimation(EWeaponAnimation.ATTACK);
		}
	}

	protected virtual void CastAttackRay() {
		Vector3 from = attackPoint.GlobalPosition;
		Vector3 to = from - GlobalTransform.Basis.Z * range; // this is minus because "forward" in Godot is -Z

		// TODO #17: Would be nice to have a helper function that casts this ray and returns a result
		//             as a "PhysicsRayResult" class. Working with Godot dictionary doesnt make sense here
		PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(from, to);
		query.CollideWithBodies = true;
		Godot.Collections.Dictionary result = GetWorld3D().DirectSpaceState.IntersectRay(query);

        if (result.ContainsKey("collider")) {
			// TODO #17: These would be much better if it was "result.collider" or "result.position" without these odd casts
			//             BTW why the fuck is IntersectRay() result a fucking dict ???? 
            Vector3 position = (Vector3)result["position"];
            Vector3 normal = (Vector3)result["normal"];
            Node3D collider = (Node3D)result["collider"];

			if (collider is RigidBody3D rigidBody3D) {
				rigidBody3D.ApplyForce(-normal * 100 * damage, position);
			}

			// TODO #18: Add option to turn this on/off in debug console
        	// SpawnDebugHitMarker(position, GlobalRotation);
        }
	}

	protected void SpawnDebugHitMarker(Vector3 position, Vector3 rotation) {
		Node3D instance = DEBUG_HIT_MARKER.Instantiate<Node3D>();
		GetTree().Root.AddChild(instance);
		instance.GlobalPosition = position;
		instance.GlobalRotation = rotation;

		GetTree().CreateTimer(DEBUG_HIT_MARKER_LIFETIME).Timeout += () => instance.QueueFree();
	}

	public void EmitCasingParticles() {
		if (casingParticle != null) {
			Casing casing = casingParticle.Instantiate<Casing>();
			GetTree().Root.AddChild(casing);

			casing.GlobalPosition = casingShute.GlobalPosition;
			casing.GlobalRotation = casingShute.GlobalRotation;
			casing.ApplyForce(casingShute.GlobalBasis.X * GD.RandRange(30, 100));
			casing.ApplyTorque(Vector3.Up * GD.RandRange(500, 1500));
		}
	}
	#endregion

	#region Reload
	protected void StartReload() {
		if (rounds == clipSize || remainingAmmo <= 0) return;

		SetState(EWeaponState.RELOADING);
		PlayAnimation(EWeaponAnimation.RELOAD);
	}

	protected void ReleaseMagazine() {
		if (currentMag == null) return;

		magazinePosition.RemoveChild(currentMag);
		GetTree().Root.AddChild(currentMag);
		currentMag.GlobalPosition = magazinePosition.GlobalPosition;
		currentMag.GlobalRotation = magazinePosition.GlobalRotation;
		currentMag.Release();
		currentMag = null;
	}
	#endregion

	#region Magazine
	protected void InstantiateMagazine() {
		if (magazineScene == null) {
			GD.PushWarning("[Weapon] Magazine scene is null for " + Name);
		}

		currentMag = magazineScene.Instantiate<Magazine>();
		currentMag.weapon = this;
		magazinePosition.AddChild(currentMag);
		currentMag.Position = Vector3.Zero;
		currentMag.Rotation = Vector3.Zero;
		currentMag.Freeze = true;
	}
	#endregion

	#region Callbacks
	protected void OnDrawEnd() {
		SetState(EWeaponState.READY);
	}

	protected void OnAttackEnd() {
		if (state != EWeaponState.ATTACKING) return;

		SetState(EWeaponState.READY);
	}

	protected void OnReloadEnd() {
		// max 15 | curr 10 | remaining 100
		// max - curr = delta
		// remaining - delta

		int delta = clipSize - rounds;

		if (delta <= remainingAmmo) {
			rounds += delta;
			remainingAmmo -= delta;
		}
		else {
			rounds = remainingAmmo;
			remainingAmmo = 0;
		}

		PlayAnimation(EWeaponAnimation.RESET);
		SetState(EWeaponState.READY);
	}
	#endregion
}

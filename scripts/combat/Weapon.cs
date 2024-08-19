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

	protected EWeaponState state = EWeaponState.DRAWING;
	protected static PackedScene DEBUG_HIT_MARKER = GD.Load<PackedScene>("res://scenes/debug/debug_hit_marker.tscn");

	#region Nodes
	protected AnimationPlayer animationPlayer;
	protected Timer fireRateTimer;
	protected Node3D casingShute;
	protected Node3D attackPoint;
    #endregion

	#region Lifecycle
    public override void _Ready() {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        casingShute = GetNode<Node3D>("CasingShute");
		attackPoint = GetNode<Node3D>("AttackPoint");
        
		fireRateTimer = GetNode<Timer>("FireRateTimer");
		fireRateTimer.WaitTime = 60 / fireRate;
		fireRateTimer.Timeout += OnAttackEnd;

		PlayAnimation(EWeaponAnimation.READY);
    }

    public override void _Input(InputEvent @event) {
        if (Input.IsActionJustPressed("primary")) {
			Attack();
		}
    }
    #endregion

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

	public virtual void Attack() {
		if (state != EWeaponState.READY) return;

		SetState(EWeaponState.ATTACKING);
		PlayAnimation(EWeaponAnimation.ATTACK);
		fireRateTimer.Start();
		CastAttackRay();
	}

	protected virtual void CastAttackRay() {
		Vector3 from = attackPoint.GlobalPosition;
		Vector3 to = from - GlobalTransform.Basis.Z * range; // this is minus because "forward" in Godot is -Z

		PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(from, to);
		query.CollideWithBodies = true;
		Godot.Collections.Dictionary result = GetWorld3D().DirectSpaceState.IntersectRay(query);
		GD.Print(result.ToString());

        if (result.ContainsKey("collider")) {
            Vector3 position = (Vector3)result["position"];

        	SpawnDebugHitMarker(position, GlobalRotation);
        }
	}

	protected void SpawnDebugHitMarker(Vector3 position, Vector3 rotation) {
		Node3D instance = DEBUG_HIT_MARKER.Instantiate<Node3D>();
		GetTree().Root.AddChild(instance);
		instance.GlobalPosition = position;
		instance.GlobalRotation = rotation;
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

	#region Callbacks
	protected void OnDrawEnd() {
		SetState(EWeaponState.READY);
	}

	protected void OnAttackEnd() {
		SetState(EWeaponState.READY);
	}
	#endregion
}

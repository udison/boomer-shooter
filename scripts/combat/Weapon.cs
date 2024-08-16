using Godot;
using System;
using System.Collections.Generic;

public partial class Weapon : Node3D
{
	[Export] protected string displayName = "";
	[Export] protected float damage = 10.0f;
	[Export(PropertyHint.Range, "0,2000,1")] protected float fireRate = 550.0f; // rounds/attacks per minute
	[Export] protected int clipSize = 16;
	[Export] protected int remainingAmmo = 320;
	[Export] protected Vector3 aimingPosition = new Vector3(0.003f, -.054f, -.226f);
	[Export] protected Godot.Collections.Array<GpuParticles3D> particles = new Godot.Collections.Array<GpuParticles3D>();

	protected EWeaponState state = EWeaponState.DRAWING;

	#region Nodes
	protected AnimationPlayer animationPlayer;
	protected Timer fireRateTimer;
    #endregion

	#region Lifecycle
    public override void _Ready() {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        
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

    public override void _PhysicsProcess(double delta) {
        HandleWeaponSway(delta);
    }
    #endregion

	private void HandleWeaponSway(double delta) {
		// TODO: Implement this
	}

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

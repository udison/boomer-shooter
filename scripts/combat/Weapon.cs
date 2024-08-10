using Godot;
using System;

public partial class Weapon : Node3D
{
	[Export] protected string displayName = "";
	[Export] protected float damage = 10.0f;

	protected EWeaponState state = EWeaponState.DRAWING;

	#region Nodes
	protected AnimationPlayer animationPlayer;
    #endregion

    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

		PlayAnimation(EWeaponAnimation.READY);
    }

    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionJustPressed("primary")) {
			Attack();
		}
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

	protected void OnDrawEnd() {
		GD.Print("draw! now bang pow pow");
		state = EWeaponState.READY;
	}

	public void Attack() {
		if (state != EWeaponState.READY) return;

		PlayAnimation(EWeaponAnimation.ATTACK);
	}
}

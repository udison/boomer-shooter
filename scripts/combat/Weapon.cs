using Godot;
using System;

public partial class Weapon : Node3D
{
	[Export] protected string displayName = "";
	[Export] protected float damage = 10.0f;

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
        base._Input(@event);
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

		animationPlayer.Play(animName);
	}

	public void Attack() {
		PlayAnimation(EWeaponAnimation.ATTACK);
	}
}

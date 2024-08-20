using Godot;
using System;

public partial class Magazine : RigidBody3D
{
	public Weapon weapon;

	public void Release() {
		Freeze = false;
		ApplyForce(-GlobalTransform.Basis.Y * 100);
	}
}

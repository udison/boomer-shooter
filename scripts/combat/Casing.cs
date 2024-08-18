using Godot;
using System;

public partial class Casing : RigidBody3D
{
	private static int LIFETIME = 5; // seconds

	SceneTreeTimer timer;
	
	public override void _Ready() {
		timer = GetTree().CreateTimer(LIFETIME);
		timer.Timeout += OnLifetimeEnd;
	}

	public void OnLifetimeEnd() {
		timer = null;
		Sleeping = true;
	}
}

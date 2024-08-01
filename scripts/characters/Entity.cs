using Godot;
using System;

public partial class Entity : CharacterBody3D
{

    /// <summary>The speed in which this entity moves</summary>
    [Export] protected float speed = 5.0f;

    /// <summary>How tall this entity jumps</summary>
	[Export] protected float jumpVelocity = 4.5f;

    protected Vector3 motion = Vector3.Zero;

    public override void _PhysicsProcess(double delta)
    {
        ApplyGravity(delta);
        Move(delta);
    }

    protected void ApplyGravity(double delta) {
		if (!IsOnFloor())
		{
			Velocity += GetGravity() * (float)delta;
		}
    }

    protected void Move(double delta) {
		Vector3 velocity = Velocity;

        if (motion != Vector3.Zero)
		{
			velocity.X = motion.X * speed;
			velocity.Z = motion.Z * speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, speed);
			velocity.Z = Mathf.MoveToward(Velocity.Z, 0, speed);
		}

		Velocity = velocity;
        MoveAndSlide();
    }
}
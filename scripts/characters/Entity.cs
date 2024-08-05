using Godot;
using System;

public partial class Entity : CharacterBody3D
{

    /// <summary>The speed in which this entity moves</summary>
	[ExportCategory("Movement")]
    [Export] protected float speed = 5.0f;

    /// <summary>How tall this entity jumps</summary>
	[Export] protected float jumpVelocity = 4.5f;

    protected Vector3 motion = Vector3.Zero;

	#region Lifecycle Hooks

    public override void _PhysicsProcess(double delta)
    {
        ApplyGravity(delta);
        Move(delta);
    }

	#endregion


	#region Public Methods

	/// <summary>Returns the max speed (this.speed) of this entity</summary>
	public float GetMaxSpeed() {
		return speed;
	}

	/// <summary>Returns the planar speed the entity is moving</summary>
	public Vector2 GetPlanarMotion() {
		return new Vector2(motion.X, motion.Z);
	}

	#endregion


	#region Protected Methods

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

	#endregion
}
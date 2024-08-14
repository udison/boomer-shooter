using Godot;
using System;

[Tool]
public partial class AutoStartIK : SkeletonIK3D
{
	public override void _Ready()
	{
		Start();
	}
}

using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class WeaponHolder : Node3D
{
	/// <summary>Returns the current active weapon the player is holding. If player is unarmed, returns null</summary>
	public Weapon GetActiveWeapon() {
		Godot.Collections.Array<Node> children = GetChildren();

		foreach (Node child in children) {
			if (child is Weapon weapon && weapon.Visible) {
				return weapon;
			}
		}

		return null;
	}
}

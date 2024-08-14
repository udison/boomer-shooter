using Godot;
using System;

public partial class Game : Node3D
{

    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionJustPressed("fullscreen")) {
			DisplayServer.WindowSetMode(
				DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen ?
					DisplayServer.WindowMode.Windowed : DisplayServer.WindowMode.Fullscreen
			);
		}
    }

    public override void _Ready() {
		Input.MouseMode = Input.MouseModeEnum.Captured;

	}
}

using Godot;
using System;

public partial class animationRun : Sprite2D
{
	private AnimationPlayer aniPlayer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Get the reference to the AnimationPlayer node
		aniPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.

	public override void _Input(InputEvent @event)
	{
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		if (direction != Vector2.Zero)
		{
			if (direction == Vector2.Left)
			{
				FlipH = true;
				Visible = true;
				aniPlayer.Play("run");
			}
			else if (direction == Vector2.Right)
			{
				FlipH = false;
				Visible = true;
				aniPlayer.Play("run");

			}
		}
		else
		{
			Visible = false;
		}

	}
}

using Godot;
using System;

public partial class animationJump : Sprite2D
{
	private AnimationPlayer aniPlayer;

	// Called when the node enters the scene tree for the first time.
	public override void _PhysicsProcess(double delta)
	{
		// Get the reference to the AnimationPlayer node
		aniPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.

	public override void _Input(InputEvent @event)
	{
		if(Input.IsActionPressed("ui_accept")) {
				Visible = true;
				aniPlayer.Play("jump");
		}
		else
		{
			Visible = false;
		}
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		if (direction != Vector2.Zero)
		{
			if (direction == Vector2.Left)
			{
				FlipH = true;
			}
			else if (direction == Vector2.Right)
			{
				FlipH = false;
			}
		}
		
	}
}

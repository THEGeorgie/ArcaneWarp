using Godot;
using System;

public partial class animationIdle : Sprite2D
{
	private AnimationPlayer aniPlayer;

	// Called when the node enters the scene tree for the first time.
	private bool isVisibale = true;
public override void _Ready()
	{
		// Get the reference to the AnimationPlayer node
		aniPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
	}
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	
	public override void _Input(InputEvent @event)
	{
		Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
		if(direction != Vector2.Zero){
			Visible = false;
			if (direction == Vector2.Left)
			{
				FlipH = true;
			}
			else if (direction == Vector2.Right)
			{
				FlipH = false;
			}
		}else{
			Visible = true;
			aniPlayer.Play("idle");
		}
		if(Input.IsActionPressed("ui_accept")) {
				Visible = false;
		}
	}
}

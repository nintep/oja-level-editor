extends AnimatedSprite2D


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	position = position.snapped(Vector2.ONE * $GridMovement.TILE_SIZE)
	position -= Vector2.ONE * ($GridMovement.TILE_SIZE / 2)
	position += Vector2.UP * ($GridMovement.TILE_SIZE / 6)
	set_moving_animation()

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta):
	var input_direction = Input.get_vector("move_left", "move_right", "move_up", "move_down")
	$GridMovement.move(input_direction)
	set_moving_animation()

func set_moving_animation() -> void:
	var facing_direction: Vector2 = $GridMovement.facing_direction
	if (facing_direction == Vector2.ZERO):
		return
		
	var dir_string: String = $GridMovement.vector2Direction(facing_direction)
	var animation_state: StringName = "idle_" + dir_string
	#var vectorDirection = vector2Direction(moving_direction)
			
	play(animation_state)

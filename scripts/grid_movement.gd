extends Node2D

@export var self_node: Node2D
@export var speed: float = 1
@export var move_delay: float = 0.2

const TILE_SIZE: int = 32

var moving_direction: Vector2 = Vector2.ZERO
var facing_direction: Vector2 = Vector2.RIGHT
var raycast: RayCast2D = null

var move_delay_remaining: float = 0

func move(direction: Vector2) -> void:
	if (!raycast):
		printerr("Raycast 2D missing")
		return
		
	if moving_direction.length() == 0 && direction.length() > 0:	
		var facing = facing_direction
		if direction.y > 0: facing = Vector2.DOWN
		elif direction.y < 0: facing = Vector2.UP
		elif direction.x > 0: facing = Vector2.RIGHT
		elif direction.x < 0: facing = Vector2.LEFT
		
		if (facing == facing_direction && move_delay_remaining > 0):
			return
			
		facing_direction = facing
		
		raycast.target_position = facing * TILE_SIZE
		raycast.force_raycast_update() # Update the `target_position` immediately
		
		# Allow movement only if no collision in next tile
		if !raycast.is_colliding():
		
			moving_direction = facing			
			var new_position = self_node.global_position + (moving_direction * TILE_SIZE)
			
			var tween = create_tween()
			tween.tween_property(self_node, "position", new_position, 1/speed).set_trans(Tween.TRANS_LINEAR)
			tween.tween_callback(func(): moving_direction = Vector2.ZERO)
			
			move_delay_remaining = move_delay

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	# Set movement direction as DOWN by default
	raycast = get_node_or_null("RayCast2D")
	if (!raycast):
		printerr("Raycast 2D missing")
	else:
		raycast.target_position = Vector2.DOWN * TILE_SIZE


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	if (move_delay_remaining > 0):
		move_delay_remaining -= delta
	
func vector2Direction(vec: Vector2) -> String:
	var direction = "down"
	if vec.y > 0: direction = "down"
	elif vec.y < 0: direction = "up"
	elif vec.x > 0:
		direction = "right"
	elif vec.x < 0:
		direction = "left"
		
	return direction

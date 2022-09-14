
public enum Objective {

	Running = 0,
	Jumping = 1,
	ObstacleJump = 2,
	Climbing = 3,
	Flying = 4
}

static class ObjectiveUtil {

	public static Objective ObjectiveForNumber(int n) {
		switch(n) {
		case 0: return Objective.Running; 
		case 1: return Objective.Jumping; 
		case 2: return Objective.ObstacleJump; 
		case 3: return Objective.Climbing;
		case 4: return Objective.Flying;
		}

		return Objective.Running;
	}

	public static string StringRepresentation(this Objective objective) {
		switch(objective) {
		case Objective.Running: return "Running";
		case Objective.Jumping: return "Jumping"; 
		case Objective.ObstacleJump: return "Obstacle Jump";
		case Objective.Climbing: return "Climbing";
		case Objective.Flying: return "Flying";
		}

		//return "Running";
		throw new System.Exception("The objective does not have a string representation.");
	}

	public static Objective ObjectiveFromString(string objective) {

		switch(objective.ToUpper()) {

		case "Running": 
		case "RUNNING":
			return Objective.Running; 
		case "Jumping":
		case "JUMPING": 
			return Objective.Jumping; 
		case "Obstacle Jump":
		case "OBSTACLE JUMP": 
			return Objective.ObstacleJump;
		case "Climbing":
		case "CLIMBING": 
			return Objective.Climbing; 
		case "Flying":
		case "FLYING":
			return Objective.Flying;

		default: throw new System.Exception("The string cannot be converted to an Objective");
		}
	}
}

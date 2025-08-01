
public enum Objective: byte {

	Running = 0,
	Jumping = 1,
	ObstacleJump = 2,
	Climbing = 3,
	Flying = 4
}

static class ObjectiveUtil {

	public const Objective LAST_OBJECTIVE = Objective.Flying;

	public static Objective[] ALL_OBJECTIVES = new Objective[] {
		Objective.Running,
		Objective.Jumping,
		Objective.ObstacleJump,
		Objective.Climbing,
		Objective.Flying
	};

	private static string[] ALL_OBJECTIVE_NAMES;
	public static string[] GetAllObjectiveNames() {
		if (ALL_OBJECTIVE_NAMES == null) {
			ALL_OBJECTIVE_NAMES = new string[ALL_OBJECTIVES.Length];
			for (int i = 0; i < ALL_OBJECTIVES.Length; i++) {
				ALL_OBJECTIVE_NAMES[i] = StringRepresentation(ALL_OBJECTIVES[i]);
			}
		}
		return ALL_OBJECTIVE_NAMES;
	}

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

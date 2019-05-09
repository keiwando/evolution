
public enum EvolutionTask {

	Running = 0,
	Jumping = 1,
	ObstacleJump = 2,
	Climbing = 3
}

static class EvolutionTaskUtil {

	public static EvolutionTask TaskForNumber(int n) {
		switch(n) {
		case 0: return EvolutionTask.Running; 
		case 1: return EvolutionTask.Jumping; 
		case 2: return EvolutionTask.ObstacleJump; 
		case 3: return EvolutionTask.Climbing;
		}

		return EvolutionTask.Running;
	}

	public static string StringRepresentation(this EvolutionTask task) {
		switch(task) {
		case EvolutionTask.Running: return "Running";
		case EvolutionTask.Jumping: return "Jumping"; 
		case EvolutionTask.ObstacleJump: return "Obstacle Jump";
		case EvolutionTask.Climbing: return "Climbing";
		}

		//return "Running";
		throw new System.Exception("The task does not have a string representation.");
	}

	public static EvolutionTask TaskFromString(string task) {

		switch(task.ToUpper()) {

		case "Running": 
		case "RUNNING": 
			return EvolutionTask.Running; 
		case "Jumping":
		case "JUMPING": 
			return EvolutionTask.Jumping; 
		case "Obstacle Jump":
		case "OBSTACLE JUMP": 
			return EvolutionTask.ObstacleJump;
		case "Climbing":
		case "CLIMBING": 
			return EvolutionTask.Climbing; 

		default: throw new System.Exception("The string cannot be converted to an EvolutionTask");
		}
	}
}

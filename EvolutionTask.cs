
public enum EvolutionTask {

	RUNNING = 0,
	JUMPING = 1,
	OBSTACLE_JUMP = 2,
	CLIMBING = 3
}

static class EvolutionTaskUtil {

	public static EvolutionTask TaskForNumber(int n) {
		switch(n) {
		case 0: return EvolutionTask.RUNNING; 
		case 1: return EvolutionTask.JUMPING; 
		case 2: return EvolutionTask.OBSTACLE_JUMP; 
		case 3: return EvolutionTask.CLIMBING;
		}

		return EvolutionTask.RUNNING;
	}

	public static string StringRepresentation(this EvolutionTask task) {
		switch(task) {
		case EvolutionTask.RUNNING: return "Running";
		case EvolutionTask.JUMPING: return "Jumping"; 
		case EvolutionTask.OBSTACLE_JUMP: return "Obstacle Jump";
		case EvolutionTask.CLIMBING: return "Climbing";
		}

		//return "Running";
		throw new System.Exception("The task does not have a string representation.");
	}

	public static EvolutionTask TaskFromString(string task) {

		switch(task.ToUpper()) {

		case "RUNNING": return EvolutionTask.RUNNING; 
		case "JUMPING": return EvolutionTask.JUMPING; 
		case "OBSTACLE JUMP": return EvolutionTask.OBSTACLE_JUMP;
		case "CLIMBING": return EvolutionTask.CLIMBING; 

		default: throw new System.Exception("The string cannot be converted to an EvolutionTask");
		}
	}
}

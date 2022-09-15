
namespace Keiwando.Evolution {

    public class BodyComponentSettingsManager {

        public event System.Action dataWillChange;

        protected void DataWillChange() {
            if (dataWillChange != null) {
                dataWillChange();
            }
        }

        public virtual void Refresh() {}
    }
}
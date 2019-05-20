using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    public abstract class BaseStructureBuilder<T>: IStructureBuilder where T: BaseStructure {

        protected abstract string prefabPath { get; }

        protected T structure;

        public BaseStructureBuilder(T structure) {
            this.structure = structure;
        }

        public virtual GameObject Build() {
            return StructureBuilderUtils.Build(prefabPath, structure.Transform);
        }
    }
}
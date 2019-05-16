namespace Keiwando.Evolution.Scenes {

    public abstract class BaseStructureBuilder: IStructureBuilder {

        protected abstract string prefabPath { get; }

        private BaseStructure structure;

        public BaseStructureBuilder(BaseStructure structure) {
            this.structure = structure;
        }

        public void Build() {
            StructureBuilderUtils.Build(prefabPath, structure.Transform);
        }
    }
}
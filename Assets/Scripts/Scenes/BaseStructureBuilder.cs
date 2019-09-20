using UnityEngine;

namespace Keiwando.Evolution.Scenes {

    public abstract class BaseStructureBuilder<T>: IStructureBuilder where T: BaseStructure {

        protected abstract string prefabPath { get; }
        protected abstract CollisionLayer collisionLayer { get; }

        protected T structure;

        public BaseStructureBuilder(T structure) {
            this.structure = structure;
        }

        public virtual GameObject Build(ISceneContext context) {
         
            var obj = StructureBuilderUtils.Build(prefabPath, structure.Transform);
            obj.layer = LayerMaskForCollisionLayer(collisionLayer, context);
            return obj;
        }

        protected LayerMask LayerMaskForCollisionLayer(CollisionLayer layer, ISceneContext context) {

            switch (layer) {
            case CollisionLayer.Background: return context.GetBackgroundLayer();
            case CollisionLayer.StaticForeground: return context.GetStaticForegroundLayer();
            case CollisionLayer.DynamicForeground: return context.GetDynamicForegroundLayer();
            case CollisionLayer.Wall: return LayerMask.NameToLayer("Wall");

            default: throw new System.ArgumentException("Unknown collision layer type");
            }
        }
    }
}
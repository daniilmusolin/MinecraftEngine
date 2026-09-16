using MinecraftEngine.Engine.Core;

namespace MinecraftEngine.Engine.World {
    public struct BlockData {
        public BlockType Type;
        public string Name;
        public bool IsSolid;
        public bool IsFullBlock;
        public bool IsTransparent;
        public int Hardness;
        public int Resistance;
        public float LightLevel;
        public string TextureTop;
        public string TextureBottom;
        public string TextureSide;
        public string TextureFront;
        public string TextureBack;
        public string TextureLeft;
        public string TextureRight;
    }
}

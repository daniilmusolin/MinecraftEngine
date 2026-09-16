namespace MinecraftEngine.Engine.Core;

public class ItemStack {
    public BlockType Type { get; set; }
    public int Count { get; set; }
    public int MaxStackSize { get; set; } = 64;

    public ItemStack(BlockType type, int count = 1) {
        Type = type;
        Count = Math.Min(count, MaxStackSize);
    }

    public bool IsEmpty => Count <= 0 || Type == BlockType.Air;
    public bool IsFull => Count >= MaxStackSize;

    public bool CanAdd(int amount = 1) {
        return Count + amount <= MaxStackSize;
    }

    public int Add(int amount = 1) {
        int canAdd = Math.Min(amount, MaxStackSize - Count);
        Count += canAdd;
        return amount - canAdd;
    }

    public int Remove(int amount = 1) {
        int removed = Math.Min(amount, Count);
        Count -= removed;
        if (Count <= 0) {
            Type = BlockType.Air;
        }
        return removed;
    }

    public ItemStack Clone() {
        return new ItemStack(Type, Count);
    }
}
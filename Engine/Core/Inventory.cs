namespace MinecraftEngine.Engine.Core;

public class Inventory {
    public const int HOTBAR_SIZE = 9;
    public const int INVENTORY_SIZE = 36;

    private ItemStack[] _items;
    private int _selectedSlot = 0;

    public event Action<int>? OnSlotChanged;

    public Inventory() {
        _items = new ItemStack[INVENTORY_SIZE];
        for (int i = 0; i < INVENTORY_SIZE; i++) {
            _items[i] = new ItemStack(BlockType.Air, 0);
        }

        // Даем стартовые блоки
        _items[0] = new ItemStack(BlockType.Grass, 64);
        _items[1] = new ItemStack(BlockType.Dirt, 64);
        _items[2] = new ItemStack(BlockType.Stone, 64);
        _items[3] = new ItemStack(BlockType.Wood, 64);
        _items[4] = new ItemStack(BlockType.Planks, 64);
        _items[5] = new ItemStack(BlockType.Cobblestone, 64);
        _items[6] = new ItemStack(BlockType.Sand, 64);
    }

    public ItemStack GetSelectedItem() {
        return _items[_selectedSlot];
    }

    public BlockType GetSelectedBlockType() {
        return _items[_selectedSlot].Type;
    }

    public int SelectedSlot {
        get => _selectedSlot;
        set {
            int newSlot = Math.Clamp(value, 0, HOTBAR_SIZE - 1);
            if (_selectedSlot != newSlot) {
                _selectedSlot = newSlot;
                OnSlotChanged?.Invoke(_selectedSlot);
            }
        }
    }

    public ItemStack this[int index] {
        get => _items[index];
        set => _items[index] = value;
    }

    public bool AddItem(BlockType type, int count = 1) {
        for (int i = 0; i < INVENTORY_SIZE; i++) {
            if (_items[i].Type == type && _items[i].CanAdd(count)) {
                _items[i].Add(count);
                return true;
            }
        }

        // Ищем пустой слот
        for (int i = 0; i < INVENTORY_SIZE; i++) {
            if (_items[i].IsEmpty) {
                _items[i] = new ItemStack(type, count);
                return true;
            }
        }

        return false;
    }

    public bool RemoveSelectedItem(int count = 1) {
        var item = GetSelectedItem();
        if (item.IsEmpty) return false;

        int removed = item.Remove(count);
        return removed > 0;
    }
}
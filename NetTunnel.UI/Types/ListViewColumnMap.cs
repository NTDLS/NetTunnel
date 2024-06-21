namespace NetTunnel.UI.Types
{
    public class ListViewColumnMap
    {
        private Dictionary<string, int> _nameToIndex = new();
        private Dictionary<int, string> _indexToName = new();

        public int this[string name]
            => _nameToIndex.TryGetValue(name, out var index) ? index : throw new Exception("The given name was not found in the collection.");

        public string? this[int index]
            => _indexToName.TryGetValue(index, out var name) ? name : throw new Exception("The given index was not found in the collection.");

        public ListViewItem.ListViewSubItem SubItem(ListViewItem item, int columnIndex)
            => item.SubItems[columnIndex] as ListViewItem.ListViewSubItem;

        public ListViewItem.ListViewSubItem SubItem(ListViewItem item, string columnName)
            => item.SubItems[this[columnName]] as ListViewItem.ListViewSubItem;
        public ListViewColumnMap(ListView? listView)
        {
            if (listView != null)
            {
                foreach (ColumnHeader column in listView.Columns)
                {
                    _nameToIndex[column.Name ?? throw new Exception($"The column with text '{column.Name}' does not have a name.")] = column.Index;
                    _indexToName[column.Index] = column.Name ?? throw new Exception($"The column with text '{column.Name}' does not have a name.");
                }
            }
        }
    }
}

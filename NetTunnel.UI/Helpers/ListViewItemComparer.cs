using System.Collections;

namespace NetTunnel.UI.Helpers
{
    internal class ListViewItemComparer : IComparer
    {
        private int column;
        private SortOrder sortOrder;

        public ListViewItemComparer()
        {
            column = 0;
            sortOrder = SortOrder.Ascending;
        }

        public int Compare(object? x, object? y)
        {
            var item1 = (ListViewItem?)x;
            var item2 = (ListViewItem?)y;

            // Use the appropriate column's text for comparison
            string text1 = item1?.SubItems[column]?.Text ?? string.Empty;
            string text2 = item2?.SubItems[column]?.Text ?? string.Empty;

            // Compare the two items based on the text and sorting order
            int result = String.Compare(text1, text2, StringComparison.OrdinalIgnoreCase);

            if (sortOrder == SortOrder.Descending)
                result = -result;

            return result;
        }

        public int SortColumn
        {
            get { return column; }
            set { column = value; }
        }

        public SortOrder SortOrder
        {
            get { return sortOrder; }
            set { sortOrder = value; }
        }
    }
}

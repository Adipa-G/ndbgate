using System;

namespace DbGate.ErManagement.DbAbstractionLayer.MetaManipulate.Support
{
    public class MetaQueryHolder : IComparable<MetaQueryHolder>
    {
        public const int OBJECT_TYPE_TABLE = 1;
        public const int OBJECT_TYPE_COLUMN = 2;
        public const int OBJECT_TYPE_PRIMARY_KEY = 3;
        public const int OBJECT_TYPE_FOREIGN_KEY = 4;

        public const int OPERATION_TYPE_ADD = 1;
        public const int OPERATION_TYPE_ALTER = 2;
        public const int OPERATION_TYPE_DELETE = 3;

        public MetaQueryHolder()
        {
        }

        public MetaQueryHolder(int itemType, int queryType, string queryString)
        {
            ItemType = itemType;
            QueryType = queryType;
            QueryString = queryString;
        }

        public int ItemType { get; set; }
        public int QueryType { get; set; }
        public string QueryString { get; set; }

        #region IComparable<MetaQueryHolder> Members

        
        public int CompareTo(MetaQueryHolder metaQueryHolder)
        {
            if (metaQueryHolder == null)
                return 1;
            if (ItemType != metaQueryHolder.ItemType)
            {
                return ItemType.CompareTo(metaQueryHolder.ItemType);
            }
            else
            {
                if (QueryType == OPERATION_TYPE_ADD)
                    return 1;
                if (QueryType == OPERATION_TYPE_DELETE)
                    return -1;
                return 0;
            }
        }

        #endregion
    }
}
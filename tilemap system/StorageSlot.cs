using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tilemap_system
{
    internal struct StorageSlot
    {
        int _index;
        StorageInfo _storageContainer;

        public static bool operator ==(StorageSlot a, StorageSlot b) => a.GetIndex() == b.GetIndex() && a.GetStorageContainer() == b.GetStorageContainer();
        public static bool operator !=(StorageSlot a, StorageSlot b) => !(a == b);

        public StorageSlot(StorageInfo storageInfo, int index)
        {
            _index = index;
            _storageContainer = storageInfo;
        }
        public int GetIndex() => _index;
        public StorageInfo GetStorageContainer() => _storageContainer;
    }
}

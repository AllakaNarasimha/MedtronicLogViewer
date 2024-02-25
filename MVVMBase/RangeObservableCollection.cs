using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MVVMBase
{
    public class RangeObservableCollection<T> : ObservableCollection<T>
    {
        public RangeObservableCollection() { }

        public RangeObservableCollection(IEnumerable<T> collection) : base(collection) { }

        public void AddRange(IEnumerable<T> collection)
        {
            foreach (var item in collection)
            {
                Items.Add(item);
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace VideoConversionApp.Utils;

public class SortableObservableCollection<T> : ObservableCollection<T>
{
    
    // This is a bit ridiculous, how we do not have sort available in ObservableCollection...
    public void Sort(Comparison<T> comparison)
    {
        ((List<T>)Items).Sort(comparison);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
}

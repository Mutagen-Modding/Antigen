using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using DynamicData;
using DynamicData.Binding;

namespace Antigen.Extensions;

public static class ObservableCollectionExtension
{
    extension<T>(IObservableCollection<T> collection)
    {
        public void LoadOptimized(IEnumerable<T> source)
        {
            var newItems = source.ToArray();

            if (ObservableCollectionHelper<T>.ItemsField.GetValue(collection) is IList items)
            {
                items.Clear();
                foreach (var newItem in newItems)
                {
                    items.Add(newItem);
                }

                ObservableCollectionHelper<T>.NotifyResetChanges(collection);
            }
            else
            {
                collection.Load(newItems);
            }
        }

        public void AddRangeOptimized(IEnumerable<T> source)
        {
            var newItems = source.ToArray();

            if (ObservableCollectionHelper<T>.ItemsField.GetValue(collection) is IList items)
            {
                var itemsCount = items.Count;
                foreach (var newItem in newItems)
                {
                    items.Add(newItem);
                }

                ObservableCollectionHelper<T>.NotifyAppendChanges(collection, newItems, itemsCount);
            }
            else
            {
                collection.AddRange(newItems);
            }
        }

        public void InsertRangeOptimized(IEnumerable<T> source, int index)
        {
            var newItems = source.ToArray();

            if (ObservableCollectionHelper<T>.ItemsField.GetValue(collection) is IList items)
            {
                var insertIndex = index;
                foreach (var newItem in newItems)
                {
                    items.Insert(index, newItem);
                    index++;
                }

                ObservableCollectionHelper<T>.NotifyAppendChanges(collection, newItems, insertIndex);
            }
            else
            {
                collection.AddRange(newItems);
            }
        }
    }

    private static class ObservableCollectionHelper<T>
    {
        public static readonly object[] CountPropertyChanged = [new PropertyChangedEventArgs(nameof(ObservableCollection<>.Count))];
        public static readonly object[] IndexerPropertyChanged = [new PropertyChangedEventArgs("Item[]")];
        public static readonly object[] ResetCollectionChanged = [new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)];

        public static readonly MethodInfo OnPropertyChanged = typeof(ObservableCollection<T>).GetMethod(
                "OnPropertyChanged",
                BindingFlags.NonPublic | BindingFlags.Instance)
         ?? throw new InvalidOperationException("Could not find OnPropertyChanged method on ObservableCollection<T>.");

        public static readonly MethodInfo OnCollectionChanged = typeof(ObservableCollection<T>).GetMethod(
                "OnCollectionChanged",
                BindingFlags.NonPublic | BindingFlags.Instance,
                [typeof(NotifyCollectionChangedEventArgs)])
         ?? throw new InvalidOperationException("Could not find OnCollectionChanged method on ObservableCollection<T>.");

        public static readonly FieldInfo ItemsField = typeof(Collection<T>).GetField(
                "items",
                BindingFlags.NonPublic | BindingFlags.Instance)
         ?? throw new InvalidOperationException("Could not find items field on Collection<T>.");

        public static void NotifyResetChanges(IObservableCollection<T> collection)
        {
            OnPropertyChanged.Invoke(collection, CountPropertyChanged);
            OnPropertyChanged.Invoke(collection, IndexerPropertyChanged);
            OnCollectionChanged.Invoke(collection, ResetCollectionChanged);
        }

        public static void NotifyAppendChanges(IObservableCollection<T> collection, IList newItems, int previousItemsCount)
        {
            OnPropertyChanged.Invoke(collection, CountPropertyChanged);
            OnPropertyChanged.Invoke(collection, IndexerPropertyChanged);
            OnCollectionChanged.Invoke(collection, [new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItems, previousItemsCount)]);
        }
    }
}
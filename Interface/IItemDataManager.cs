using System.Collections;
using System.Collections.Generic;

namespace PangyaAPI.SuperSocket.Interface
{
    public interface IItemDataManager : ICollection, IEnumerable
    {

        object this[int index]
        {

            get;

            set;
        }


        bool IsReadOnly
        {

            get;
        }


        bool IsFixedSize
        {

            get;
        }


        int Add(object value);


        bool Contains(object value);


        void Clear();


        int IndexOf(object value);


        void Insert(int index, object value);


        void Remove(object value);


        void RemoveAt(int index);
    }
    public interface IItemDataManager<T> : ICollection<T>, IEnumerable<T>, IEnumerable
    {

        object this[int index]
        {

            get;

            set;
        }


        bool IsReadOnly
        {

            get;
        }


        bool IsFixedSize
        {

            get;
        }


        int Add(object value);


        bool Contains(object value);


        void Clear();


        int IndexOf(object value);


        void Insert(int index, object value);


        void Remove(object value);


        void RemoveAt(int index);
    }
}
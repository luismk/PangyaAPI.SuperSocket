namespace PangyaAPI.SuperSocket.Interface
{
    /// <summary>
    /// ISmartPoolSourceCreator
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISmartPoolSourceCreator<T>
    {
        /// <summary>
        /// Creates the specified size.
        /// </summary>
        /// <param name="size">The size.</param>
        /// <param name="poolItems">The pool items.</param>
        /// <returns></returns>
        ISmartPoolSource Create(int size, out T[] poolItems);
    }
    /// <summary>
    /// The basic interface of smart pool
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ISmartPool<T> : IPoolInfo
    {
        /// <summary>
        /// Initializes the specified min pool size.
        /// </summary>
        /// <param name="minPoolSize">The min size of the pool.</param>
        /// <param name="maxPoolSize">The max size of the pool.</param>
        /// <param name="sourceCreator">The source creator.</param>
        /// <returns></returns>
        void Initialize(int minPoolSize, int maxPoolSize, ISmartPoolSourceCreator<T> sourceCreator);

        /// <summary>
        /// Pushes the specified item into the pool.
        /// </summary>
        /// <param name="item">The item.</param>
        void Push(T item);

        /// <summary>
        /// Tries to get one item from the pool.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        bool TryGet(out T item);
    }

    /// <summary>
    /// ISmartPoolSource
    /// </summary>
    public interface ISmartPoolSource
    {
        /// <summary>
        /// Gets the count.
        /// </summary>
        /// <value>
        /// The count.
        /// </value>
        int Count { get; }
    }
}
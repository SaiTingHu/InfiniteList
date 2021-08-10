using System;

namespace HT.InfiniteList
{
    /// <summary>
    /// 无限列表异常
    /// </summary>
    public class InfiniteListException : Exception
    {
        public InfiniteListException(string message) : base(message)
        {

        }
    }
}
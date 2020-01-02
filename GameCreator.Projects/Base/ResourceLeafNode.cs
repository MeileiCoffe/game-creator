﻿using System.Collections.Generic;
using GameCreator.Resources.Api;

namespace GameCreator.Projects
{
    public class ResourceLeafNode<T> : ResourceNode<T> where T : IIndexedResource
    {
        public T Value { get; }
        
        internal ResourceLeafNode(T value)
        {
            Value = value;
        }
        
        public override IEnumerator<T> GetEnumerator()
        {
            yield return Value;
        }
    }
}
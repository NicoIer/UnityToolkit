// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.

using System.Collections.Generic;
using System.Numerics;

namespace UnityToolkit
{
    public class QuadTreeNode<T>
    {
        public float xMin;
        public float yMin;
        public float xMax;
        public float yMax;

        public Vector2 center { get; private set; }
        public T data;
        
        public QuadTreeNode<T> topRight;
        public QuadTreeNode<T> topLeft;
        public QuadTreeNode<T> bottomLeft;
        public QuadTreeNode<T> bottomRight;
        
        public QuadTreeNode(in float xMin, in float xMax, in float yMin, in float yMax, T data)
        {
            this.xMin = xMin;
            this.xMax = xMax;
            this.yMin = yMin;
            this.yMax = yMax;
            this.data = data;
            center = new Vector2((xMin + xMax) / 2, (yMin + yMax) / 2);
        }
        
        public bool Contains(in float x, float y)
        {
            return x >= xMin && x < xMax && y >= yMin && y < yMax;
        }
    }
}
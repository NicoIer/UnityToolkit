// Copyright (c) 2023 NicoIer and Contributors.
// Licensed under the MIT License (MIT). See LICENSE in the repository root for more information.
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityToolkit
{
    [Serializable]
    public class Path
    {
        private IReadOnlyList<int> internalList;

        public Path(IReadOnlyList<int> content)
        {
            internalList = content;
        }

        public int this[int index] => internalList[index];

        public int Count
        {
            get
            {
                if(internalList == null)
                {
                    return 0;
                }
                return internalList.Count;
            }
        }
    }
}
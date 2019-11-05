// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using ItemsRepeaterTestApp.Utils;
using System.Collections.Generic;

namespace ItemsRepeaterTestApp.Samples.Model
{
    public class RecipeGroup : ResetableCollection<Recipe>
    {
        public string Name { get; set; }

        public RecipeGroup(IEnumerable<Recipe> recipes)
            : base(recipes)
        { }

        public override string ToString()
        {
            return Name;
        }
    }
}

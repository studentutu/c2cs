// Copyright (c) Lucas Girouard-Stranks (https://github.com/lithiumtoast). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory (https://github.com/lithiumtoast/c2cs) for full license information.

namespace C2CS.CSharp
{
    public record CSharpCommon
    {
        public readonly string Name;
        public readonly string CodeLocationComment;

        public CSharpCommon(
            string name,
            string locationComment)
        {
            Name = name;
            CodeLocationComment = locationComment;
        }

        // Required for debugger string with records
        // ReSharper disable once RedundantOverriddenMember
        public override string ToString()
        {
            return $"{Name} {CodeLocationComment}";
        }
    }
}

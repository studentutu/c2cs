// Copyright (c) Lucas Girouard-Stranks (https://github.com/lithiumtoast). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory (https://github.com/lithiumtoast/c2cs) for full license information.

namespace C2CS.Languages.C
{
    public record ClangOpaquePointer : ClangCommon
    {
        public readonly ClangType PointerType;

        internal ClangOpaquePointer(
            string name,
            ClangCodeLocation codeLocation,
            ClangType pointerType)
            : base(name, codeLocation)
        {
            PointerType = pointerType;
        }

        // Required for debugger string with records
        // ReSharper disable once RedundantOverriddenMember
        public override string ToString()
        {
            return base.ToString();
        }
    }
}

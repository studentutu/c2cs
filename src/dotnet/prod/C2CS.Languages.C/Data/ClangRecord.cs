// Copyright (c) Lucas Girouard-Stranks (https://github.com/lithiumtoast). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory (https://github.com/lithiumtoast/c2cs) for full license information.

using System.Collections.Immutable;

namespace C2CS.Languages.C
{
    public record ClangRecord : ClangCommon
    {
        public readonly ClangType Type;
        public readonly ImmutableArray<ClangRecordField> Fields;
        public readonly ImmutableArray<ClangRecord> NestedRecords;

        internal ClangRecord(
            string name,
            ClangCodeLocation codeLocation,
            ClangType type,
            ImmutableArray<ClangRecordField> fields,
            ImmutableArray<ClangRecord> nestedRecords)
            : base(name, codeLocation)
        {
            Type = type;
            Fields = fields;
            NestedRecords = nestedRecords;
        }

        // Required for debugger string with records
        // ReSharper disable once RedundantOverriddenMember
        public override string ToString()
        {
            return base.ToString();
        }
    }
}

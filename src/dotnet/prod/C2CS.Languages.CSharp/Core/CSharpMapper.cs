// Copyright (c) Lucas Girouard-Stranks (https://github.com/lithiumtoast). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory (https://github.com/lithiumtoast/c2cs) for full license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Text.RegularExpressions;
using C2CS.Languages.C;

namespace C2CS.CSharp
{
    public static class CSharpMapper
    {
        public static CSharpAbstractSyntaxTree GetAbstractSyntaxTree(
            ClangAbstractSyntaxTree clangAbstractSyntaxTree)
        {
            var functionExterns = MapFunctionExterns(
                clangAbstractSyntaxTree.FunctionExterns);
            var functionPointers = MapFunctionPointers(
                clangAbstractSyntaxTree.FunctionPointers);
            var structs = MapStructs(
                clangAbstractSyntaxTree.Records,
                clangAbstractSyntaxTree.AliasDataTypes,
                clangAbstractSyntaxTree.OpaquePointers);
            var opaqueDataTypes = MapOpaqueDataTypes(
                clangAbstractSyntaxTree.OpaqueDataTypes);
            var enums = MapEnums(
                clangAbstractSyntaxTree.Enums);

            var result = new CSharpAbstractSyntaxTree(
                functionExterns,
                functionPointers,
                structs,
                opaqueDataTypes,
                enums);

            return result;
        }

        private static ImmutableArray<CSharpFunctionExtern> MapFunctionExterns(ImmutableArray<ClangFunctionExtern> clangFunctionExterns)
        {
            var builder = ImmutableArray.CreateBuilder<CSharpFunctionExtern>(clangFunctionExterns.Length);

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var clangFunctionExtern in clangFunctionExterns)
            {
                var functionExtern = MapFunctionExtern(clangFunctionExtern);
                builder.Add(functionExtern);
            }

            var result = builder.ToImmutable();
            return result;
        }

        private static CSharpFunctionExtern MapFunctionExtern(ClangFunctionExtern clangFunctionExtern)
        {
            var name = clangFunctionExtern.Name;
            var originalCodeLocationComment = MapOriginalCodeLocationComment(clangFunctionExtern.CodeLocation);
            var returnType = MapType(clangFunctionExtern.ReturnType);
            var callingConvention = MapFunctionCallingConvention(clangFunctionExtern.CallingConvention);
            var parameters = MapFunctionExternParameters(clangFunctionExtern.Parameters);

            var result = new CSharpFunctionExtern(
                name,
                originalCodeLocationComment,
                callingConvention,
                returnType,
                parameters);

            return result;
        }

        private static CSharpFunctionExternCallingConvention MapFunctionCallingConvention(
            ClangFunctionExternCallingConvention clangFunctionCallingConvention)
        {
            var result = clangFunctionCallingConvention switch
            {
                ClangFunctionExternCallingConvention.C => CSharpFunctionExternCallingConvention.C,
                ClangFunctionExternCallingConvention.Unknown => CSharpFunctionExternCallingConvention.Unknown,
                _ => throw new ArgumentOutOfRangeException(nameof(clangFunctionCallingConvention), clangFunctionCallingConvention, null)
            };

            return result;
        }

        private static ImmutableArray<CSharpFunctionExternParameter> MapFunctionExternParameters(
            ImmutableArray<ClangFunctionExternParameter> clangFunctionExternParameters)
        {
            var builder = ImmutableArray.CreateBuilder<CSharpFunctionExternParameter>(clangFunctionExternParameters.Length);
            var parameterNames = new List<string>();

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var clangFunctionExternParameter in clangFunctionExternParameters)
            {
                var parameterName = MapUniqueParameterName(clangFunctionExternParameter.Name, parameterNames);
                parameterNames.Add(parameterName);
                var functionExternParameter = MapFunctionExternParameter(clangFunctionExternParameter, parameterName);
                builder.Add(functionExternParameter);
            }

            var result = builder.ToImmutable();
            return result;
        }

        private static string MapUniqueParameterName(string parameterName, List<string> parameterNames)
        {
            if (string.IsNullOrEmpty(parameterName))
            {
                parameterName = "param";
            }

            while (parameterNames.Contains(parameterName))
            {
                var numberSuffixMatch = Regex.Match(parameterName, "\\d$");
                if (numberSuffixMatch.Success)
                {
                    var parameterNameWithoutSuffix = parameterName.Substring(0, numberSuffixMatch.Index);
                    parameterName = ParameterNameUniqueSuffix(parameterNameWithoutSuffix, numberSuffixMatch.Value);
                }
                else
                {
                    parameterName = ParameterNameUniqueSuffix(parameterName, string.Empty);
                }
            }

            return parameterName;

            static string ParameterNameUniqueSuffix(string parameterNameWithoutSuffix, string parameterSuffix)
            {
                if (parameterSuffix == string.Empty)
                {
                    return parameterNameWithoutSuffix + "2";
                }

                var parameterSuffixNumber = int.Parse(parameterSuffix, NumberStyles.Integer, CultureInfo.InvariantCulture);
                parameterSuffixNumber += 1;
                var parameterName = parameterNameWithoutSuffix + parameterSuffixNumber;
                return parameterName;
            }
        }

        private static CSharpFunctionExternParameter MapFunctionExternParameter(
            ClangFunctionExternParameter clangFunctionExternParameter, string parameterName)
        {
            var name = SanitizeIdentifierName(parameterName);
            var originalCodeLocationComment = MapOriginalCodeLocationComment(clangFunctionExternParameter.CodeLocation);
            var type = MapType(clangFunctionExternParameter.Type);
            var isReadOnly = clangFunctionExternParameter.IsReadOnly;

            var result = new CSharpFunctionExternParameter(
                name,
                originalCodeLocationComment,
                type,
                isReadOnly);

            return result;
        }

        private static ImmutableArray<CSharpFunctionPointer> MapFunctionPointers(ImmutableArray<ClangFunctionPointer> clangFunctionPointers)
        {
            var builder = ImmutableArray.CreateBuilder<CSharpFunctionPointer>(clangFunctionPointers.Length);

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var clangFunctionPointer in clangFunctionPointers)
            {
                var functionPointer = MapFunctionPointer(clangFunctionPointer);
                builder.Add(functionPointer);
            }

            var result = builder.ToImmutable();
            return result;
        }

        private static CSharpFunctionPointer MapFunctionPointer(ClangFunctionPointer clangFunctionPointer)
        {
            var name = clangFunctionPointer.Name;
            var originalCodeLocationComment = MapOriginalCodeLocationComment(clangFunctionPointer.CodeLocation);
            var type = MapType(clangFunctionPointer.Type);

            var result = new CSharpFunctionPointer(
                name,
                originalCodeLocationComment,
                type);

            return result;
        }

        private static ImmutableArray<CSharpStruct> MapStructs(
            ImmutableArray<ClangRecord> records,
            ImmutableArray<ClangAliasType> aliasDataTypes,
            ImmutableArray<ClangOpaquePointer> opaquePointers)
        {
            var builder = ImmutableArray.CreateBuilder<CSharpStruct>(
                records.Length + aliasDataTypes.Length);

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var clangRecord in records)
            {
                var @struct = MapStruct(clangRecord);
                builder.Add(@struct);
            }

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var clangAliasDataType in aliasDataTypes)
            {
                var @struct = MapAliasDataType(clangAliasDataType);
                builder.Add(@struct);
            }

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var clangOpaquePointer in opaquePointers)
            {
                var @struct = MapOpaquePointer(clangOpaquePointer);
                builder.Add(@struct);
            }

            var result = builder.ToImmutable();
            return result;
        }

        private static CSharpStruct MapStruct(ClangRecord clangRecord)
        {
            var name = clangRecord.Name;
            var originalCodeLocationComment = MapOriginalCodeLocationComment(clangRecord.CodeLocation);
            var type = MapType(clangRecord.Type);
            var fields = MapStructFields(clangRecord.Fields);
            var nestedStructs = MapNestedStructs(clangRecord.NestedRecords);

            var result = new CSharpStruct(
                name,
                originalCodeLocationComment,
                type,
                fields,
                nestedStructs);

            return result;
        }

        private static ImmutableArray<CSharpStructField> MapStructFields(ImmutableArray<ClangRecordField> clangRecordFields)
        {
            var builder = ImmutableArray.CreateBuilder<CSharpStructField>(clangRecordFields.Length);

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var clangRecordField in clangRecordFields)
            {
                var structField = MapStructField(clangRecordField);
                builder.Add(structField);
            }

            var result = builder.ToImmutable();
            return result;
        }

        private static CSharpStructField MapStructField(ClangRecordField clangRecordField)
        {
            var name = SanitizeIdentifierName(clangRecordField.Name);
            var originalName = clangRecordField.Name;
            var originalCodeLocationComment = MapOriginalCodeLocationComment(clangRecordField.CodeLocation);
            var type = MapType(clangRecordField.Type);
            var offset = clangRecordField.Offset;
            var padding = clangRecordField.Padding;
            var isWrapped = type.IsArray && !IsValidFixedBufferType(type.Name);

            var result = new CSharpStructField(
                name,
                originalName,
                originalCodeLocationComment,
                type,
                offset,
                padding,
                isWrapped);

            return result;
        }

        private static ImmutableArray<CSharpStruct> MapNestedStructs(ImmutableArray<ClangRecord> clangNestedRecords)
        {
            var builder = ImmutableArray.CreateBuilder<CSharpStruct>(clangNestedRecords.Length);

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var clangRecordNestedRecord in clangNestedRecords)
            {
                var nestedRecord = MapStruct(clangRecordNestedRecord);
                builder.Add(nestedRecord);
            }

            var result = builder.ToImmutable();
            return result;
        }

        private static ImmutableArray<CSharpOpaqueDataType> MapOpaqueDataTypes(ImmutableArray<ClangOpaqueDataType> opaqueDataTypes)
        {
            var builder = ImmutableArray.CreateBuilder<CSharpOpaqueDataType>(opaqueDataTypes.Length);

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var clangOpaqueDataType in opaqueDataTypes)
            {
                var opaqueDataType = MapOpaqueDataType(clangOpaqueDataType);
                builder.Add(opaqueDataType);
            }

            var result = builder.ToImmutable();
            return result;
        }

        private static CSharpOpaqueDataType MapOpaqueDataType(ClangOpaqueDataType clangOpaqueDataType)
        {
            var name = clangOpaqueDataType.Name;
            var originalCodeLocationComment = MapOriginalCodeLocationComment(clangOpaqueDataType.CodeLocation);

            var result = new CSharpOpaqueDataType(
                name,
                originalCodeLocationComment);

            return result;
        }

        private static CSharpStruct MapOpaquePointer(ClangOpaquePointer clangOpaquePointer)
        {
            var name = clangOpaquePointer.Name;
            var originalCodeLocationComment = MapOriginalCodeLocationComment(clangOpaquePointer.CodeLocation);
            var type = MapType(clangOpaquePointer.PointerType);
            var fields = MapOpaquePointerFields(clangOpaquePointer.PointerType, originalCodeLocationComment);

            var result = new CSharpStruct(
                name,
                originalCodeLocationComment,
                type,
                fields);

            return result;
        }

        private static ImmutableArray<CSharpStructField> MapOpaquePointerFields(ClangType clangType, string originalCodeLocationComment)
        {
            var type = MapType(clangType);
            var structField = new CSharpStructField(
                "Pointer",
                string.Empty,
                originalCodeLocationComment,
                type,
                0,
                0,
                false);

            var result = ImmutableArray.Create(structField);
            return result;
        }

        private static CSharpStruct MapAliasDataType(ClangAliasType clangAliasType)
        {
            var name = clangAliasType.Name;
            var originalCodeLocationComment = MapOriginalCodeLocationComment(clangAliasType.CodeLocation);
            var type = MapType(clangAliasType.UnderlyingType);
            var fields = MapAliasDataTypeFields(clangAliasType.UnderlyingType, originalCodeLocationComment);

            var result = new CSharpStruct(
                name,
                originalCodeLocationComment,
                type,
                fields);

            return result;
        }

        private static ImmutableArray<CSharpStructField> MapAliasDataTypeFields(ClangType clangType, string originalCodeLocationComment)
        {
            var type = MapType(clangType);
            var structField = new CSharpStructField(
                "Data",
                string.Empty,
                originalCodeLocationComment,
                type,
                0,
                0,
                false);

            var result = ImmutableArray.Create(structField);
            return result;
        }

        public static ImmutableArray<CSharpEnum> MapEnums(ImmutableArray<ClangEnum> clangEnums)
        {
            var builder = ImmutableArray.CreateBuilder<CSharpEnum>(clangEnums.Length);

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var clangEnum in clangEnums)
            {
                var @enum = MapEnum(clangEnum);
                builder.Add(@enum);
            }

            var result = builder.ToImmutable();
            return result;
        }

        private static CSharpEnum MapEnum(ClangEnum clangEnum)
        {
            var name = clangEnum.Name;
            var originalCodeLocationComment = MapOriginalCodeLocationComment(clangEnum.CodeLocation);
            var type = MapType(clangEnum.IntegerType);
            var values = MapEnumValues(clangEnum.Values);

            var result = new CSharpEnum(
                name,
                originalCodeLocationComment,
                type,
                values);
            return result;
        }

        private static ImmutableArray<CSharpEnumValue> MapEnumValues(ImmutableArray<ClangEnumValue> clangEnumValues)
        {
            var builder = ImmutableArray.CreateBuilder<CSharpEnumValue>(clangEnumValues.Length);

            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (var clangEnumValue in clangEnumValues)
            {
                var @enum = MapEnumValue(clangEnumValue);
                builder.Add(@enum);
            }

            var result = builder.ToImmutable();
            return result;
        }

        private static CSharpEnumValue MapEnumValue(ClangEnumValue clangEnumValue)
        {
            var name = clangEnumValue.Name;
            var originalCodeLocationComment = MapOriginalCodeLocationComment(clangEnumValue.CodeLocation);
            var value = clangEnumValue.Value;

            var result = new CSharpEnumValue(
                name,
                originalCodeLocationComment,
                value);

            return result;
        }

        private static CSharpType MapType(ClangType clangType)
        {
            var name = MapTypeName(clangType);
            var originalName = clangType.OriginalName;
            var sizeOf = clangType.SizeOf;
            var alignOf = clangType.AlignOf;
            var fixedBufferSize = clangType.ArraySize;

            var result = new CSharpType(
                name,
                originalName,
                sizeOf,
                alignOf,
                fixedBufferSize);

            return result;
        }

        private static string MapTypeName(ClangType clangType)
        {
            string result = clangType.Name;

            if (clangType.IsSystemType && clangType.Name == "bool")
            {
                result = "CBool";
            }

            return result;
        }

        private static string MapOriginalCodeLocationComment(ClangCodeLocation codeLocation)
        {
            var kind = codeLocation.Kind;
            var fileName = codeLocation.FileName;
            var fileLineNumber = codeLocation.FileLineNumber;
            var dateTime = codeLocation.DateTime;

            var result = $"// {kind} @ {fileName}:{fileLineNumber} {dateTime}";

            return result;
        }

        private static string SanitizeIdentifierName(string name)
        {
            var result = name;

            switch (name)
            {
                case "abstract":
                case "as":
                case "base":
                case "bool":
                case "break":
                case "byte":
                case "case":
                case "catch":
                case "char":
                case "checked":
                case "class":
                case "const":
                case "continue":
                case "decimal":
                case "default":
                case "delegate":
                case "do":
                case "double":
                case "else":
                case "enum":
                case "event":
                case "explicit":
                case "extern":
                case "false":
                case "finally":
                case "fixed":
                case "float":
                case "for":
                case "foreach":
                case "goto":
                case "if":
                case "implicit":
                case "in":
                case "int":
                case "interface":
                case "internal":
                case "is":
                case "lock":
                case "long":
                case "namespace":
                case "new":
                case "null":
                case "object":
                case "operator":
                case "out":
                case "override":
                case "params":
                case "private":
                case "protected":
                case "public":
                case "readonly":
                case "record":
                case "ref":
                case "return":
                case "sbyte":
                case "sealed":
                case "short":
                case "sizeof":
                case "stackalloc":
                case "static":
                case "string":
                case "struct":
                case "switch":
                case "this":
                case "throw":
                case "true":
                case "try":
                case "typeof":
                case "uint":
                case "ulong":
                case "unchecked":
                case "unsafe":
                case "ushort":
                case "using":
                case "virtual":
                case "void":
                case "volatile":
                case "while":
                    result = $"@{name}";
                    break;
            }

            return result;
        }

        private static bool IsValidFixedBufferType(string typeString)
        {
            return typeString switch
            {
                "bool" => true,
                "byte" => true,
                "char" => true,
                "short" => true,
                "int" => true,
                "long" => true,
                "sbyte" => true,
                "ushort" => true,
                "uint" => true,
                "ulong" => true,
                "float" => true,
                "double" => true,
                _ => false
            };
        }
    }
}

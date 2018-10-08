﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;
using Microsoft.CodeAnalysis.Operations;

namespace Analyzer.Utilities.FlowAnalysis.Analysis.PropertySetAnalysis
{
    /// <summary>
    /// Analysis result from execution of <see cref="PropertySetAnalysis"/> on a control flow graph.
    /// </summary>
    internal sealed class PropertySetAnalysisResult : DataFlowAnalysisResult<PropertySetBlockAnalysisResult, PropertySetAbstractValue>
    {
        public PropertySetAnalysisResult(
            DataFlowAnalysisResult<PropertySetBlockAnalysisResult, PropertySetAbstractValue> parameterValidationAnalysisResult,
            ImmutableDictionary<OperationMethodKey, PropertySetAbstractValue> hazardousUsages)
            : base(parameterValidationAnalysisResult)
        {
            this.HazardousUsages = hazardousUsages;
        }

        public ImmutableDictionary<OperationMethodKey, PropertySetAbstractValue> HazardousUsages { get; }
    }
}

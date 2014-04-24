﻿/* ****************************************************************************
 *
 * Copyright (c) Microsoft Corporation. 
 *
 * This source code is subject to terms and conditions of the Apache License, Version 2.0. A 
 * copy of the license can be found in the License.html file at the root of this distribution. If 
 * you cannot locate the Apache License, Version 2.0, please send an email to 
 * vspython@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
 * by the terms of the Apache License, Version 2.0.
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.NodejsTools.Analysis.Analyzer;
using Microsoft.NodejsTools.Interpreter;
using Microsoft.NodejsTools.Parsing;

namespace Microsoft.NodejsTools.Analysis.Values {
    internal class FunctionValue : ExpandoValue {
        private readonly ObjectValue _instance;
        private ReferenceDict _references;

        internal FunctionValue(ProjectEntry projectEntry)
            : base(projectEntry) {
            _instance = new ObjectValue(ProjectEntry, this);
        }

        public IAnalysisSet NewThis {
            get {
                return _instance.SelfSet;
            }
        }

        public override IAnalysisSet Construct(Node node, AnalysisUnit unit, IAnalysisSet[] args) {
            return _instance;
        }

        public override Dictionary<string, IAnalysisSet> GetAllMembers() {
            var res = base.GetAllMembers();

            if (this != ProjectState._functionPrototype) {
                foreach (var keyValue in ProjectState._functionPrototype.GetAllMembers()) {
                    IAnalysisSet existing;
                    if (!res.TryGetValue(keyValue.Key, out existing)) {
                        res[keyValue.Key] = keyValue.Value;
                    } else {
                        res[keyValue.Key] = existing.Union(keyValue.Value);
                    }
                }
            }

            return res;
        }

        public override IAnalysisSet GetMember(Node node, AnalysisUnit unit, string name) {
            var res = base.GetMember(node, unit, name);
            if (this != ProjectState._functionPrototype) {
                res = res.Union(ProjectState._functionPrototype.GetMember(node, unit, name));
            }
            return res;
        }

#if FALSE
        public override IAnalysisSet GetDescriptor(Node node, AnalysisValue instance, AnalysisValue context, AnalysisUnit unit) {
            if (instance == ProjectState._noneInst || IsStatic) {
                return SelfSet;
            }
            if (_methods == null) {
                _methods = new Dictionary<AnalysisValue, IAnalysisSet>();
            }

            IAnalysisSet result;
            if (!_methods.TryGetValue(instance, out result) || result == null) {
                _methods[instance] = result = new BoundMethodInfo(this, instance).SelfSet;
            }

            if (IsProperty) {
                throw new NotImplementedException("IsProperty function call");
                //return result.Call(node, unit, ExpressionEvaluator.EmptySets);
            }

            return result;
        }
#endif

        public override JsMemberType MemberType {
            get {
                return JsMemberType.Function;
            }
        }

        internal static string MakeParameterName(ParameterDeclaration curParam) {
            return curParam.Name;
        }

        internal override void AddReference(Node node, AnalysisUnit unit) {
            if (!unit.ForEval) {
                if (_references == null) {
                    _references = new ReferenceDict();
                }
                _references.GetReferences(unit.DeclaringModule.ProjectEntry).AddReference(new EncodedLocation(unit.Tree, node));
            }
        }

        internal override IEnumerable<LocationInfo> References {
            get {
                if (_references != null) {
                    return _references.AllReferences;
                }
                return new LocationInfo[0];
            }
        }

        public override BuiltinTypeId TypeId {
            get {
                return BuiltinTypeId.Function;
            }
        }
        
        internal override bool UnionEquals(AnalysisValue av, int strength) {
#if FALSE
            if (strength >= MergeStrength.ToObject) {
                return av is FunctionValue /*|| av is BuiltinFunctionInfo || av == ProjectState.ClassInfos[BuiltinTypeId.Function].Instance*/;
            }
#endif
            return base.UnionEquals(av, strength);
        }

        internal override int UnionHashCode(int strength) {
#if FALSE
            if (strength >= MergeStrength.ToObject) {
                return ProjectState._numberPrototype.GetHashCode();
            }
#endif
            return base.UnionHashCode(strength);
        }

        internal override AnalysisValue UnionMergeTypes(AnalysisValue av, int strength) {
#if FALSE
            if (strength >= MergeStrength.ToObject) {
                return ProjectState.ClassInfos[BuiltinTypeId.Function].Instance;
            }
#endif

            return base.UnionMergeTypes(av, strength);
        }
    }
}

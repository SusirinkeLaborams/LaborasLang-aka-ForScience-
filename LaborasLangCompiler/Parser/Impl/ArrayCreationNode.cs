using LaborasLangCompiler.Common;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LaborasLangCompiler.Codegen;
using System.Diagnostics.Contracts;

namespace LaborasLangCompiler.Parser.Impl
{
    class ArrayCreationNode : ExpressionNode, IArrayCreationNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ArrayCreation; } }
        public override bool IsGettable { get { return true; } }
        public override bool IsSettable { get { return true; } }
        public override TypeReference ExpressionReturnType { get { return type; } }

        public IReadOnlyList<IExpressionNode> Dimensions { get; private set; }
        public IReadOnlyList<IExpressionNode> Initializer { get { return InitializerList.Initializers.ToArray(); } }
        public InitializerList InitializerList { get; private set; }

        public bool IsImplicit { get; private set; }

        private TypeReference type;

        private ArrayCreationNode(SequencePoint point)
            : base(point)
        { 
        }

        public static ArrayCreationNode Create(ContextNode context, TypeReference type, IEnumerable<ExpressionNode> dims, InitializerList initializer,  SequencePoint point)
        {
            Contract.Requires(dims == null || dims.Any());
            Contract.Requires(type != null || initializer != null);
            //dims are diclared inside the type, cant have dims without type
            Contract.Requires(!(dims != null && type == null));
            var instance = new ArrayCreationNode(point);

            if(type == null)
            {
                type = initializer.ElementType;
                instance.IsImplicit = true;
            }

            if(dims == null)
            {
                if(initializer == null)
                {
                    ErrorCode.MissingArraySize.ReportAndThrow(point, "Cannot create array without size or an initializer");
                }
                dims = CreateArrayDims(context, point, initializer.Dimmensions.ToArray());
            }

            foreach(var dim in dims)
            {
                if(!dim.IsGettable || !dim.ExpressionReturnType.IsIntegerType())
                {
                    ErrorCode.NotAnRValue.ReportAndThrow(dim.SequencePoint, "Array dimensions must be gettable integer expressions");
                }
            }

            if (initializer != null)
            {
                if(!initializer.ElementType.IsAssignableTo(type) && initializer.Initializers.Any())
                {
                    ErrorCode.TypeMissmatch.ReportAndThrow(point,
                        "Cannot initializer array of element type {0} when initializer element type is {1}",
                        type.FullName, initializer.ElementType.FullName);
                }

                if(initializer.Dimmensions.Count() != dims.Count())
                {
                    ErrorCode.MisshapedMatrix.ReportAndThrow(point,
                        "Cannot initialize array of {0} dimmensions with a matrix of {1} dimmensions",
                        dims.Count(), initializer.Dimmensions.Count());
                }
            }
            instance.type = AssemblyRegistry.GetArrayType(type, dims.Count());
            instance.Dimensions = dims.ToArray();
            instance.InitializerList = initializer;
            return instance;
        }

        public static ArrayCreationNode Create(ContextNode context, InitializerList initializer, SequencePoint point)
        {
            return Create(context, null, null, initializer, point);
        }

        private static IEnumerable<LiteralNode> CreateArrayDims(ContextNode context, SequencePoint point, params int[] dims)
        {
            return dims.Select(dim => LiteralNode.Create(context, dim, point));
        }

        public override string ToString(int indent)
        {
            throw new NotImplementedException();
        }
    }
}

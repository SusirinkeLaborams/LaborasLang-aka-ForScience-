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
using Lexer.Containers;
using LaborasLangCompiler.Parser.Utils;
using Lexer;

namespace LaborasLangCompiler.Parser.Impl
{
    class ArrayCreationNode : ExpressionNode, IArrayCreationNode
    {
        public override ExpressionNodeType ExpressionType { get { return ExpressionNodeType.ArrayCreation; } }
        public override bool IsGettable { get { return true; } }
        public override bool IsSettable { get { return true; } }
        public override TypeReference ExpressionReturnType { get { return type; } }

        public IReadOnlyList<IExpressionNode> Dimensions { get { return dimensions.ToArray(); } }
        public IReadOnlyList<IExpressionNode> Initializer 
        { 
            get 
            { 
                return InitializerList != null ? 
                    InitializerList.Initializers.ToArray() :
                    null; 
            } 
        }
        public InitializerList InitializerList { get; private set; }

        public bool IsImplicit { get; private set; }

        private ArrayType type;
        private IEnumerable<ExpressionNode> dimensions;

        private ArrayCreationNode(SequencePoint point)
            : base(point)
        { 
        }

        public static ArrayCreationNode Parse(ContextNode context, IAbstractSyntaxTree lexerNode)
        {
            Contract.Requires(lexerNode.Type == Lexer.TokenType.ArrayLiteral);
            Contract.Ensures(Contract.Result<ArrayCreationNode>() != null);
            var point = context.Parser.GetSequencePoint(lexerNode);
            if(lexerNode.Children.Count == 1)
            {
                var initializer = InitializerList.Parse(context, lexerNode.Children[0]);
                return Create(context, initializer, point);
            }
            else
            {
                var builder = new TypeNode.TypeBuilder(context);
                var lexerType = lexerNode.Children[0];
                for (int i = 0; i < lexerType.Children.Count - 1; i++)
                {
                    builder.Append(lexerType.Children[i]);
                }
                //.Last() returns parameter list
                var last = lexerType.Children.Last().Children[0];
                var elementType = builder.Type;
                IReadOnlyList<ExpressionNode> dims;
                if(ArrayAccessNode.IsEmptyIndexer(last))
                {
                    dims = Enumerable.Repeat((ExpressionNode)null, ArrayAccessNode.CountEmptyIndexerDims(last)).ToArray();
                }
                else
                {
                    dims = ArrayAccessNode.ParseIndex(context, last);
                }
                var initializer = InitializerList.Parse(context, lexerNode.Children[1]);
                return Create(context, elementType, dims, initializer, point);
            }
        }

        public static ArrayCreationNode Create(ContextNode context, TypeReference elementType, IReadOnlyList<ExpressionNode> dims, InitializerList initializer,  SequencePoint point)
        {
            //rank 0 makes no sense
            Contract.Requires(dims == null || dims.Any());
            Contract.Requires(elementType != null || initializer != null);
            //dims are declared inside the type, both must be null or not null
            Contract.Requires((dims == null) == (elementType == null));
            //cant have [5,] or some shit
            Contract.Requires(dims == null || dims.All(d => d == null) || !dims.Any(d => d == null));
            Contract.Ensures(Contract.Result<ArrayCreationNode>() != null);

            var instance = new ArrayCreationNode(point);

            if(elementType == null)
            {
                if (initializer == null)
                {
                    ErrorCode.MissingArraySize.ReportAndThrow(point, "Cannot create array without size or an initializer");
                }
                if (initializer.ElementType.IsTypeless())
                {
                    ErrorCode.InferrenceFromTypeless.ReportAndThrow(initializer.SequencePoint, "Cannot infer array type from typeless expressions");
                }
                elementType = initializer.ElementType;
                instance.IsImplicit = true;
                dims = CreateArrayDims(context, point, initializer.Dimensions.ToArray());
            }

            if(initializer != null && dims.Count() != initializer.Dimensions.Count())
            {
                ErrorCode.MisshapedMatrix.ReportAndThrow(point,
                        "Cannot initialize array of {0} dimmensions with a matrix of {1} dimmensions",
                        dims.Count(), initializer.Dimensions.Count());
            }

            if(dims.Any(d => d == null))
            {
                if(initializer == null)
                {
                    ErrorCode.MissingArraySize.ReportAndThrow(point, "Cannot create array with implicit dimensions without initialization");
                }
                dims = CreateArrayDims(context, point, initializer.Dimensions.ToArray());
            }
            else
            {
                if(initializer != null && dims.Any(dim => !(dim is LiteralNode)))
                {
                    ErrorCode.NotLiteralArrayDims.ReportAndThrow(point,
                        "When initializing arrays, dimensions must be literal, or implicit");
                }
            }

            foreach(var dim in dims)
            {
                if (!dim.IsGettable || !dim.ExpressionReturnType.IsIntegerType())
                {
                    ErrorCode.NotAnRValue.ReportAndThrow(dim.SequencePoint, "Array dimensions must be gettable integer expressions");
                }
            }

            if (initializer != null && !initializer.Initializers.Any())
            {
                initializer = null;
            }

            if (initializer != null)
            {
                if(!initializer.ElementType.IsAssignableTo(elementType))
                {
                    ErrorCode.TypeMissmatch.ReportAndThrow(point,
                        "Cannot initializer array of element type {0} when initializer element type is {1}",
                        elementType.FullName, initializer.ElementType.FullName);
                }

                var numericDims = dims.Cast<LiteralNode>().Select(num => (int)num.Value).ToArray();

                for(int i = 0; i < dims.Count; i++)
                {
                    var value = numericDims[i];
                    if (value != initializer.Dimensions[i])
                    {
                        ErrorCode.ArrayDimMissmatch.ReportAndThrow(point,
                            "Dimension mismatch, array dimensions are [{0}] initializer are [{1}]",
                            String.Join(", ", numericDims), String.Join(", ", initializer.Dimensions));
                    }
                }
            }
            instance.type = AssemblyRegistry.GetArrayType(elementType, dims.Count);
            instance.dimensions = dims;
            instance.InitializerList = initializer;
            return instance;
        }

        public static ArrayCreationNode Create(ContextNode context, InitializerList initializer, SequencePoint point)
        {
            return Create(context, null, null, initializer, point);
        }

        private static IReadOnlyList<LiteralNode> CreateArrayDims(ContextNode context, SequencePoint point, params int[] dims)
        {
            return dims.Select(dim => LiteralNode.Create(context, dim, point)).ToArray();
        }

        public override string ToString(int indent)
        {
            StringBuilder builder = new StringBuilder();
            builder.Indent(indent).AppendLine("ArrayCreation:");
            builder.Indent(indent + 1).AppendFormat("ElementType: {0}", type.ElementType).AppendLine();
            builder.Indent(indent + 1).AppendFormat("Rank: {0}", type.Rank).AppendLine();
            builder.Indent(indent + 1).AppendLine("Dimensions:");
            foreach(var dim in dimensions)
            {
                builder.AppendLine(dim.ToString(indent + 2));
            }
            if(InitializerList != null)
            {
                builder.Indent(indent + 1).AppendLine("Initializer:");
                builder.AppendLine(InitializerList.ToString(indent + 2));
            }
            return builder.ToString();
        }
    }
}

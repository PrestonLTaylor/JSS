using JSS.Lib.AST.Values;
using JSS.Lib.Runtime;

namespace JSS.Lib.Execution;

enum RuntimeErrorType
{
    AssignmentToConst,
    BindingNotDefined,
    BindingThisToAlreadyThisInitializedEnvironment,
    CallingANonFunction,
    ConstructingFromNonConstructor,
    CouldNotCreateDataProperty,
    CouldNotDefineProperty,
    FailedToSet,
    FunctionConstructWithKindBaseNotReturningObject,
    PuttingValueInNonReference,
    RedeclarationOfImmutableBinding,
    RedeclarationOfLet,
    RedeclarationOfMutableBinding,
    RedeclarationOfUnconfigurable,
    RedeclarationOfUnconfigurableFunction,
    RedeclarationOfUnextensibleVar,
    RedeclarationOfVar,
    RhsOfInIsNotObject,
    ThisIsNotAnObject,
    UnableToConvertObjectToPrimitive,
    UnableToConvertToObject,
    UninitializedThisValue,
    UnknownError,
}

internal static class ThrowHelper
{
    static public Completion ThrowReferenceError(VM vm, RuntimeErrorType type, params object?[] args)
    {
        try
        {
            return ConstructError(vm, vm.ReferenceErrorConstructor, CreateErrorMessage(type, args));
        }
        catch (KeyNotFoundException)
        {
            return ThrowReferenceError(vm, RuntimeErrorType.UnknownError, type);
        }
    }

    static public Completion ThrowTypeError(VM vm, RuntimeErrorType type, params object?[] args)
    {
        try
        {
            return ConstructError(vm, vm.TypeErrorConstructor, CreateErrorMessage(type, args));
        }
        catch (KeyNotFoundException)
        {
            return ThrowTypeError(vm, RuntimeErrorType.UnknownError, type);
        }
    }

    static public Completion ThrowSyntaxError(VM vm, RuntimeErrorType type, params object?[] args)
    {
        try
        {
            return ConstructError(vm, vm.SyntaxErrorConstructor, CreateErrorMessage(type, args));
        }
        catch (KeyNotFoundException)
        {
            return ThrowSyntaxError(vm, RuntimeErrorType.UnknownError, type);
        }
    }

    static private Completion ConstructError(VM vm, NativeErrorConstructor constructor, string message)
    {
        List arguments = new();
        arguments.Values.Add(message);
        var constructResult = constructor.Construct(vm, arguments);
        if (constructResult.IsAbruptCompletion()) return constructResult;
        return Completion.ThrowCompletion(constructResult.Value);
    }

    static private string CreateErrorMessage(RuntimeErrorType type, params object?[] args)
    {
        var formatString = errorTypeToFormatString[type];
        return string.Format(formatString, args);
    }

    static private readonly Dictionary<RuntimeErrorType, string> errorTypeToFormatString = new()
    {
        { RuntimeErrorType.AssignmentToConst, "invalid assignment to const {0}" },
        { RuntimeErrorType.BindingNotDefined, "{0} is not defined" },
        { RuntimeErrorType.BindingThisToAlreadyThisInitializedEnvironment, "tried to bind a this value to already this-initialized function environment" },
        { RuntimeErrorType.CallingANonFunction, "{0} is not a function" },
        { RuntimeErrorType.ConstructingFromNonConstructor, "{0} is not a constructor" },
        { RuntimeErrorType.CouldNotCreateDataProperty, "could not create a data property of name {0} with a value of {0}" },
        { RuntimeErrorType.CouldNotDefineProperty, "could not define property of name {0} with a value of {0}" },
        { RuntimeErrorType.FailedToSet, "failed to set {0}" },
        { RuntimeErrorType.FunctionConstructWithKindBaseNotReturningObject, "function constructor without kind of base did not return an object/undefined" },
        { RuntimeErrorType.PuttingValueInNonReference, "tried to put a value into a non-reference" },
        { RuntimeErrorType.RedeclarationOfImmutableBinding, "redeclaration of immutable binding {0}" },
        { RuntimeErrorType.RedeclarationOfLet, "redeclaration of var {0}" },
        { RuntimeErrorType.RedeclarationOfMutableBinding, "redeclaration of mutable binding {0}" },
        { RuntimeErrorType.RedeclarationOfUnconfigurable, "redeclaration of unconfigurable {0}" },
        { RuntimeErrorType.RedeclarationOfUnconfigurableFunction, "redeclaration of unconfigurable function {0}" },
        { RuntimeErrorType.RedeclarationOfUnextensibleVar, "redeclaration of Unextensible var {0}" },
        { RuntimeErrorType.RedeclarationOfVar, "redeclaration of var {0}" },
        { RuntimeErrorType.RhsOfInIsNotObject, "rhs of 'in' should be an Object, but got {0}" },
        { RuntimeErrorType.ThisIsNotAnObject, "this is not an object" },
        { RuntimeErrorType.UnableToConvertObjectToPrimitive, "unable to convert object to a primitive" },
        { RuntimeErrorType.UnableToConvertToObject, "unable to convert {0} to an object" },
        { RuntimeErrorType.UninitializedThisValue, "tried to get an uninitialized this value" },
        { RuntimeErrorType.UnknownError, "Unknown internal error of type {0}" },
    };
}

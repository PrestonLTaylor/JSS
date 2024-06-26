﻿using JSS.Lib.AST.Values;
using JSS.Lib.Execution;

namespace JSS.Lib.Runtime;

// 23.1.1 The Array Constructor, https://tc39.es/ecma262/#sec-array-constructor
internal sealed class ArrayConstructor : Object
{
    // The Array constructor has a [[Prototype]] internal slot whose value is %Function.prototype%.
    public ArrayConstructor(FunctionPrototype prototype) : base(prototype)
    {
    }

    public void Initialize(Realm realm, VM vm)
    {
        // 23.1.2.2 Array.isArray ( arg ), https://tc39.es/ecma262/#sec-array.isarray
        InternalDefineProperty(vm, "isArray", 1, isArray, new(true, false, true));

        // 23.1.2.4 Array.prototype, https://tc39.es/ecma262/#sec-array.prototype
        InternalDefineProperty("prototype", realm.ArrayPrototype, new(true, false, true));
    }

    // 23.1.2.2 Array.isArray ( arg ), https://tc39.es/ecma262/#sec-array.isarray
    private Completion isArray(VM vm, Value thisArgument, List argumentList, Object newTarget)
    {
        // 1. Return ? IsArray(arg).
        return argumentList[0].IsArray();
    }
}

namespace JSS.Lib.Runtime;

// FIXME: The Number prototype object is itself a Number object; it has a [[NumberData]] internal slot with the value +0𝔽.
// 21.1.3 Properties of the Number Prototype Object, https://tc39.es/ecma262/#sec-number.prototype
internal sealed class NumberPrototype : Object
{
	// The Number prototype object has a [[Prototype]] internal slot whose value is %Object.prototype%.
	public NumberPrototype(ObjectPrototype prototype) : base(prototype)
	{
	}
}

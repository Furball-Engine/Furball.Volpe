using System;

namespace Furball.Volpe.Exceptions; 

public class OutOfBoundsException : VolpeException {
	private readonly string _val;
	private readonly Type   _type;
	public OutOfBoundsException(string val, Type type, PositionInText positionInText) : base(positionInText) {
		this._val  = val;
		this._type = type;
	}
	public override string Description => $"{this._val} is out of bounds for type {this._type.Name}!";
}

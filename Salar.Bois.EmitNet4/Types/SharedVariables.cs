using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Salar.Bois.Types
{
	class SharedVariables
	{
		private readonly ILGenerator _il;
		private readonly List<LocalBuilder> _variables = new List<LocalBuilder>();


		public SharedVariables(ILGenerator il)
		{
			_il = il;
		}

		public LocalBuilder GetOrAdd(Type type)
		{
			for (var index = 0; index < _variables.Count; index++)
			{
				var variable = _variables[index];
				if (variable.LocalType == type)
				{
					_variables.RemoveAt(index);
					return variable;
				}
			}

			return _il.DeclareLocal(type);
		}

		public void ReturnVariable(LocalBuilder variable)
		{
			_variables.Add(variable);
		}
	}
}
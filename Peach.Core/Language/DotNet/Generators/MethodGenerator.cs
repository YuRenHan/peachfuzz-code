
/* Copyright (c) 2007-2009 Michael Eddington
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy 
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights 
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
 * copies of the Software, and to permit persons to whom the Software is 
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in	
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 * Authors:
 *   Michael Eddington (mike@dejavusecurity.com)
 * 
 * $Id$
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace Peach.Core.Language.DotNet.Generators
{
	public class MethodGenerator : ITypeGenerator
	{
		IContext _context;
		IGroup _group;
		MethodInfo _methodInfo;
		object[] _args = null;
		IGenerator _ctorGenerator;

		Dictionary<ParameterInfo, IGenerator> _parameterGenerators = new Dictionary<ParameterInfo, IGenerator>();

		int _position = 0;
		ParameterInfo[] _parameters = null;

		public MethodGenerator(IContext context, IGroup group, MethodInfo ctorInfo, IGenerator ctorGenerator)
		{
			_context = context;
			_group = group;
			_methodInfo = ctorInfo;
			_ctorGenerator = ctorGenerator;

			_parameters = ctorInfo.GetParameters();
			_args = new object[_parameters.Length];

			foreach (ParameterInfo param in _parameters)
				_parameterGenerators.Add(param, _context.GetTypeGenerator(group, param.ParameterType, new object[] { param }));
		}

		#region ITypeGenerator Members

		public static bool SupportedType(Type type)
		{
			if (type.IsSubclassOf(typeof(MethodInfo)))
				return true;

			return false;
		}

		public static ITypeGenerator CreateInstance(IContext context, IGroup group, Type type, object [] obj)
		{
			return new MethodGenerator(context, group, (MethodInfo)obj[0], (IGenerator) obj[1]);
		}

		#endregion

		#region IGenerator Members

		public object GetValue()
		{
			for (int i = 0; i < _parameters.Length; i++)
			{
				if (_parameterGenerators[_parameters[i]] == null)
					_args[i] = _parameterGenerators[_parameters[i]];
				else
					_args[i] = _parameterGenerators[_parameters[i]].GetValue();
			}

			try
			{
				return _methodInfo.Invoke(_ctorGenerator.GetValue(), _args);
			}
			catch
			{
				return null;
			}
		}

		public void Next()
		{
			try
			{
				if (_parameters.Length == 0)
					throw new GeneratorCompleted();

				_parameterGenerators[_parameters[_position]].Next();
			}
			catch (GeneratorCompleted)
			{
				_position++;
				if (_position >= _parameters.Length)
				{
					_position--;
					throw new GeneratorCompleted();
				}
			}
		}

		public void Reset()
		{
			_position = 0;
			foreach (IGenerator generator in _parameterGenerators.Values)
				generator.Reset();
		}

		#endregion
	}
}

// end
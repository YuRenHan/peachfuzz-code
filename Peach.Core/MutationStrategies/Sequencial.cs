﻿
//
// Copyright (c) Michael Eddington
//
// Permission is hereby granted, free of charge, to any person obtaining a copy 
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights 
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in	
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

// Authors:
//   Michael Eddington (mike@dejavusecurity.com)

// $Id$

using System;
using System.Collections.Generic;
using System.Text;
using Peach.Core.Dom;
using System.Reflection;

using NLog;

namespace Peach.Core.MutationStrategies
{
	[MutationStrategy("Sequencial")]
	public class Sequencial : MutationStrategy
	{
		static NLog.Logger logger = LogManager.GetCurrentClassLogger();

		Dictionary<DataElement, List<Mutator>> _stuffs = new Dictionary<DataElement, List<Mutator>>();
		List<Type> _mutators = new List<Type>();
		bool recording = true;
		int? _count = null;
		int _iterationCount = 0;

		public Sequencial(Dictionary<string, Variant> args)
			: base(args)
		{
		}

		public override void Initialize(RunContext context, Engine engine)
		{
			base.Initialize(context, engine);

			// Setup our handlers to record first iteration
			recording = true;
			StateModel.Starting += new StateModelStartingEventHandler(StateModel_Starting);
			StateModel.Finished += new StateModelFinishedEventHandler(StateModel_Finished);
			Core.Dom.Action.Starting += new ActionStartingEventHandler(Action_Starting);

			_iterationCount = 0;

			_mutators.AddRange(EnumerateValidMutators());
		}

		public override int IterationCount
		{
			get { return _iterationCount; }
		}

		void Action_Starting(Core.Dom.Action action)
		{
			if (!recording && enumeratorsInitialized)
			{
				string fullName = _dataElementEnumerator.Current.fullName;
				if (action.dataModel != null)
				{
					DataElement elem = action.dataModel.find(fullName);
					if (elem != null)
					{
						logger.Info("Action_Starting: Fuzzing: " + elem.fullName);
						logger.Info("Action_Starting: Mutator: " + _mutatorEnumerator.Current.name);

						try
						{
							_mutatorEnumerator.Current.sequencialMutation(elem);
                            //_mutatorEnumerator.Current.randomMutation(elem);
						}
						catch (OutOfMemoryException)
						{
							logger.Debug("Mutator caused out of memory exception, Ignoring!");
						}
					}
				}
				else if(action.parameters != null && action.parameters.Count > 0)
				{
					foreach (ActionParameter param in action.parameters)
					{
						if (param.dataModel == null)
							continue;

						DataElement elem = param.dataModel.find(fullName);
						if (elem != null)
						{
							logger.Info("Action_Starting: Fuzzing: " + elem.fullName);
							logger.Info("Action_Starting: Mutator: " + _mutatorEnumerator.Current.name);

							try
							{
								_mutatorEnumerator.Current.sequencialMutation(elem);
							}
							catch (OutOfMemoryException)
							{
								logger.Debug("Mutator caused out of memory exception, Ignoring!");
							}
						}
					}
				}

				return;
			}

			if (action.dataModel != null)
			{
				List<DataElement> allElements = new List<DataElement>();
				RecursevlyGetElements(action.dataModel as DataElementContainer, allElements);
				foreach (DataElement elem in allElements)
				{
					List<Mutator> elemMutators = new List<Mutator>();

					foreach (Type t in _mutators)
					{
                        // can add specific mutators here
						if (SupportedDataElement(t, elem))
							elemMutators.Add(GetMutatorInstance(t, elem));
					}

					_stuffs[elem] = elemMutators;
				}
			}
			else if (action.parameters != null && action.parameters.Count > 0)
			{
				foreach (ActionParameter param in action.parameters)
				{
					if (param.dataModel == null)
						continue;

					List<DataElement> allElements = new List<DataElement>();
					RecursevlyGetElements(param.dataModel as DataElementContainer, allElements);
					foreach (DataElement elem in allElements)
					{
						List<Mutator> elemMutators = new List<Mutator>();

						foreach (Type t in _mutators)
						{
							if (SupportedDataElement(t, elem))
								elemMutators.Add(GetMutatorInstance(t, elem));
						}

						_stuffs[elem] = elemMutators;
					}
				}
			}
		}

		void StateModel_Finished(StateModel model)
		{
			recording = false;
		}

		void StateModel_Starting(StateModel model)
		{
			if (recording)
			{
				_stuffs.Clear();
			}
		}

		public override uint count
		{
			get
			{
				if (_count != null)
					return (uint)_count;

				_count = 1; // Always one iteration before us!
				foreach (List<Mutator> l in _stuffs.Values)
				{
					foreach (Mutator m in l)
					{
						_count += m.count;
					}
				}

				return (uint)_count;
			}
		}

		public override Mutator currentMutator()
		{
			throw new NotImplementedException();
		}

		bool enumeratorsInitialized = false;
		Dictionary<DataElement, List<Mutator>>.KeyCollection.Enumerator _dataElementEnumerator;
		List<Mutator>.Enumerator _mutatorEnumerator;

		public override void next()
		{
			_iterationCount++;

			if (enumeratorsInitialized)
			{
				try
				{
					_mutatorEnumerator.Current.next();
				}
				catch
				{
					while (!_mutatorEnumerator.MoveNext())
					{
						if (!_dataElementEnumerator.MoveNext())
							throw new MutatorCompleted();

						_mutatorEnumerator = _stuffs[_dataElementEnumerator.Current].GetEnumerator();
					}
				}
			}
			else
			{
				enumeratorsInitialized = true;
				_dataElementEnumerator = _stuffs.Keys.GetEnumerator();
				if (!_dataElementEnumerator.MoveNext())
					throw new MutatorCompleted();

				_mutatorEnumerator = _stuffs[_dataElementEnumerator.Current].GetEnumerator();
				while (!_mutatorEnumerator.MoveNext())
				{
					if (!_dataElementEnumerator.MoveNext())
						throw new MutatorCompleted();

					_mutatorEnumerator = _stuffs[_dataElementEnumerator.Current].GetEnumerator();
				}
			}
		}
	}
}

// end

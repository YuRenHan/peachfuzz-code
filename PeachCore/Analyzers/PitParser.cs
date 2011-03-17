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
//   Michael Eddington (mike@phed.org)

// $Id$

using System;
using System.Xml;
using System.Xml.Schema;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using PeachCore.Dom;

namespace PeachCore.Analyzers
{
	public interface IPitParsable
	{
		// TODO: These should be static?  Need to look into it.

		/// <summary>
		/// Ask object if it can parse XmlNode.
		/// </summary>
		/// <param name="node">node to check</param>
		/// <param name="parent">parent of this object</param>
		/// <returns>Returns true if class can parse xml node.</returns>
		bool pit_canParse(XmlNode node, object parent);

		/// <summary>
		/// Called by PitParser analyzer to parse 
		/// current XML Node.
		/// </summary>
		/// <param name="node"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		object pit_handleNode(XmlNode node, object parent);
	}

	/// <summary>
	/// This is the default analyzer for Peach.  It will
	/// parse a Peach PIT file (XML document) into a Peach DOM.
	/// </summary>
	public class PitParser : Analyzer
	{
		static int ErrorsCount = 0;
		static string ErrorMessage = "";
		Dom.Dom _dom = null;
		bool isScriptingLanguageSet = false;

		static PitParser()
		{
			PitParser.supportParser = true;
			Analyzer.defaultParser = new PitParser();
		}

		public PitParser()
		{
		}

		public override Dom.Dom asParser(Dictionary<string, string> args, string fileName)
		{
			if (!File.Exists(fileName))
				throw new PeachException("Error: Unable to locate Pit file [" + fileName + "].\n");

			validatePit(fileName);

			XmlDocument xmldoc = new XmlDocument();
			xmldoc.Load(fileName);

			_dom = new Dom.Dom();

			if (xmldoc.FirstChild.Name == "Peach")
				handlePeach(xmldoc.FirstChild, _dom);

			return _dom;
		}

		public override void asParserValidation(Dictionary<string, string> args, string fileName)
		{
			validatePit(fileName);
		}

		/// <summary>
		/// Validate PIT XML using Schema file.
		/// </summary>
		/// <param name="fileName">Pit file to validate</param>
		/// <param name="schema">Peach XML Schema file</param>
		public void validatePit(string fileName)
		{
			XmlTextReader tr = new XmlTextReader(
				Assembly.GetExecutingAssembly().GetManifestResourceStream("PeachCore.peach.xsd"));
			XmlSchemaSet set = new XmlSchemaSet();
			set.Add(null, tr);

			XmlReaderSettings settings = new XmlReaderSettings();
			settings.IgnoreComments = true;
			settings.Schemas = set;
			settings.ValidationType = ValidationType.Schema;
			settings.ValidationEventHandler += new ValidationEventHandler(vr_ValidationEventHandler);

			XmlReader xmlFile = XmlTextReader.Create(fileName, settings);

			while (xmlFile.Read()) ;
			xmlFile.Close();

			if (ErrorsCount > 0)
				throw new PeachException("Error: Pit file failed to validate: " + ErrorMessage);
		}

		/// <summary>
		/// Called when the Schema validator hits an error.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void vr_ValidationEventHandler(object sender, ValidationEventArgs e)
		{
			ErrorMessage = ErrorMessage + e.Message + "\r\n";
			ErrorsCount++;
		}

		/// <summary>
		/// Handle parsing the top level Peach node.
		/// </summary>
		/// <param name="node">XmlNode to parse</param>
		/// <param name="dom">DOM to fill</param>
		/// <returns>Returns the parsed Dom object.</returns>
		protected Dom.Dom handlePeach(XmlNode node, Dom.Dom dom)
		{
			// Pass 1 - Handle imports, includes, python path

			foreach (XmlNode child in node)
			{
				switch (child.Name)
				{
					case "Include":
						string ns = getXmlAttribute(child, "ns");
						string fileName = getXmlAttribute(child, "src");

						PitParser parser = new PitParser();
						if (!File.Exists(fileName))
							throw new PeachException("Error: Unable to locate Pit file [" + fileName + "].\n");

						validatePit(fileName);

						XmlDocument xmldoc = new XmlDocument();
						xmldoc.Load(fileName);

						Dom.DomNamespace nsObj = new Dom.DomNamespace();
						nsObj.parent = dom;
						nsObj.name = ns;

						if (xmldoc.FirstChild.Name == "Peach")
							handlePeach(xmldoc.FirstChild, nsObj);

						dom.ns[ns] = nsObj;
						break;

					case "Require":
						Scripting.Imports.Add(getXmlAttribute(child, "require"));
						break;
					case "Import":
						if (hasXmlAttribute(child, "from"))
							throw new PeachException("Error, This version of Peach does not support the 'from' attribute for 'Import' elements.");

						Scripting.Imports.Add(getXmlAttribute(child, "import"));
						break;
					case "PythonPath":
						if (isScriptingLanguageSet && 
							Scripting.DefaultScriptingEngine != ScriptingEngines.Python)
						{
							throw new PeachException("Error, cannot mix Python and Ruby!");
						}
						Scripting.DefaultScriptingEngine = ScriptingEngines.Python;
						Scripting.Paths.Add(getXmlAttribute(child, "import"));
						isScriptingLanguageSet = true;
						break;
					case "RubyPath":
						if (isScriptingLanguageSet && 
							Scripting.DefaultScriptingEngine != ScriptingEngines.Ruby)
						{
							throw new PeachException("Error, cannot mix Python and Ruby!");
						}
						Scripting.DefaultScriptingEngine = ScriptingEngines.Ruby;
						Scripting.Paths.Add(getXmlAttribute(child, "require"));
						isScriptingLanguageSet = true;
						break;
					case "Python":
						if (isScriptingLanguageSet && 
							Scripting.DefaultScriptingEngine != ScriptingEngines.Python)
						{
							throw new PeachException("Error, cannot mix Python and Ruby!");
						}
						Scripting.DefaultScriptingEngine = ScriptingEngines.Python;
						Scripting.Exec(getXmlAttribute(child, "code"), new Dictionary<string,object>());
						isScriptingLanguageSet = true;
						break;
					case "Ruby":
						if (isScriptingLanguageSet && 
							Scripting.DefaultScriptingEngine != ScriptingEngines.Ruby)
						{
							throw new PeachException("Error, cannot mix Python and Ruby!");
						}
						Scripting.DefaultScriptingEngine = ScriptingEngines.Ruby;
						Scripting.Exec(getXmlAttribute(child, "code"), new Dictionary<string, object>());
						isScriptingLanguageSet = true;
						break;

					case "Defaults":
						throw new NotSupportedException("Implement Defaults element parsing support.");
				}
			}

			// Pass 3 - Handle data model

			foreach (XmlNode child in node)
			{
				if (child.Name == "DataModel")
				{
					DataModel dm = handleDataModel(child);
					dom.dataModels.Add(dm.name, dm);
				}
			}

			// Pass 4 - Handle Data

			foreach (XmlNode child in node)
			{
				if (child.Name == "Data")
				{
				}
			}

			// Pass 5 - Handle State model

			foreach (XmlNode child in node)
			{
				if (child.Name == "StateModel")
				{
				}
			}

			// Pass 6 - Handle Test

			foreach (XmlNode child in node)
			{
				if (child.Name == "Test")
				{
				}
			}

			// Pass 7 - Handle Run

			foreach (XmlNode child in node)
			{
				if (child.Name == "Run")
				{
				}
			}

			return dom;
		}

		#region Utility Methods

		/// <summary>
		/// Get attribute from XmlNode object.
		/// </summary>
		/// <param name="node">XmlNode to get attribute from</param>
		/// <param name="name">Name of attribute</param>
		/// <returns>Returns innerText or null.</returns>
		protected string getXmlAttribute(XmlNode node, string name)
		{
			try
			{
				return node.Attributes[name].InnerText;
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// Get attribute from XmlNode object.
		/// </summary>
		/// <param name="node">XmlNode to get attribute from</param>
		/// <param name="name">Name of attribute</param>
		/// <param name="defaultValue">Default value if attribute is missing</param>
		/// <returns>Returns true/false or default value</returns>
		protected bool getXmlAttributeAsBool(XmlNode node, string name, bool defaultValue)
		{
			try
			{
				string value = node.Attributes[name].InnerText.ToLower();
				switch (value)
				{
					case "1":
					case "true":
						return true;
					case "0":
					case "false":
						return false;
					default:
						throw new PeachException("Error, " + name + " has unknown value, should be boolean.");
				}
			}
			catch
			{
				return defaultValue;
			}
		}

		/// <summary>
		/// Check to see if XmlNode has specific attribute.
		/// </summary>
		/// <param name="node">XmlNode to check</param>
		/// <param name="name">Name of attribute</param>
		/// <returns>Returns boolean true or false.</returns>
		protected bool hasXmlAttribute(XmlNode node, string name)
		{
			try
			{
				object o = node.Attributes[name];
				return false;
			}
			catch
			{
				return true;
			}
		}

		/// <summary>
		/// Resolve a 'ref' attribute.  Will throw a PeachException if
		/// namespace is given, but not found.
		/// </summary>
		/// <param name="dom">DOM to use for resolving ref.</param>
		/// <param name="name">Ref name to resolve.</param>
		/// <returns>DataElement for ref or null if not found.</returns>
		protected DataElement getReference(Dom.Dom dom, string name)
		{
			if (name.IndexOf(':') > -1)
			{
				string ns = name.Substring(0, name.IndexOf(':') - 1);

				if (!dom.ns.Keys.Contains(ns))
					throw new PeachException("Unable to locate namespace '"+ns+"' in ref '"+name+"'.");

				name = name.Substring(name.IndexOf(':'));
				dom = dom.ns["name"];
			}

			DataElement obj = null;
			foreach (DataModel model in dom.dataModels.Values)
			{
				obj = model.find(name) as DataElement;
				if (obj != null)
					return obj;
			}

			return null;
		}

#endregion

		#region Data Model

		protected DataModel handleDataModel(XmlNode node)
		{
			DataModel dataModel = new DataModel();

			if (hasXmlAttribute(node, "ref"))
			{
				DataModel refObj = getReference(_dom, getXmlAttribute(node, "ref")) as DataModel;
				if (refObj != null)
				{
					dataModel = ObjectCopier.Clone<DataModel>(refObj);
				}
				else
				{
					throw new PeachException("Unable to locate 'ref' [" + getXmlAttribute(node, "ref") + "] or found node did not match type. [" + node.OuterXml + "].");
				}
			}

			dataModel.name = getXmlAttribute(node, "name");
			if(dataModel.name == null)
				throw new PeachException("Error, DataModel missing required 'name' attribute.");

			handleCommonDataElementAttributes(node, dataModel);
			handleCommonDataElementChildren(node, dataModel);
			handleDataElementContainer(node, dataModel);

			return dataModel;
		}

		protected Block handleBlock(XmlNode node, DataElementContainer parent)
		{
			Block block = new Block();

			if (hasXmlAttribute(node, "ref"))
			{
				Block refObj = getReference(_dom, getXmlAttribute(node, "ref")) as Block;
				if (refObj != null)
				{
					block = ObjectCopier.Clone<Block>(refObj);
				}
				else
				{
					throw new PeachException("Unable to locate 'ref' [" + getXmlAttribute(node, "ref") + "] or found node did not match type. [" + node.OuterXml + "].");
				}
			}

			// name
			string name = getXmlAttribute(node, "name");
			block.name = name;

			// lengthType
			if (hasXmlAttribute(node, "lengthType"))
			{
				block.lengthType = getXmlAttribute(node, "lengthType");
				block.lengthCalc = getXmlAttribute(node, "length");
				block.length = null;

				if (block.lengthType == null)
					throw new PeachException("Error, Block attribute 'lengthType' has invalid value.");
				if (block.lengthCalc == null)
					throw new PeachException("Error, When specifying lenghType=\"calc\" you must also provide a valid 'length' attribute.");
			}
			// length
			else if (hasXmlAttribute(node, "length"))
			{
				block.length = new Variant(Convert.ToInt32(getXmlAttribute(node, "length")));
			}

			// alignment

			handleCommonDataElementAttributes(node, block);
			handleCommonDataElementChildren(node, block);
			handleDataElementContainer(node, block);

			return block;
		}

		/// <summary>
		/// Handle common attributes such as the following:
		/// 
		///  * mutable
		///  * contraint
		///  * pointer
		///  * pointerDepth
		///  * token
		///  
		/// </summary>
		/// <param name="node">XmlNode to read attributes from</param>
		/// <param name="element">Element to set attributes on</param>
		protected void handleCommonDataElementAttributes(XmlNode node, DataElement element)
		{
			if (hasXmlAttribute(node, "token"))
				throw new NotSupportedException("implement token attribute");
			
			if (hasXmlAttribute(node, "mutable"))
				element.isMutable = false;
			
			if (hasXmlAttribute(node, "constraint"))
				throw new NotSupportedException("Implement me!");

			if (hasXmlAttribute(node, "pointer"))
				throw new NotSupportedException("Implement pointer attribute");
			
			if (hasXmlAttribute(node, "pointerDepth"))
				throw new NotSupportedException("Implement pointerDepth attribute");
		}

		/// <summary>
		/// Handle parsing common dataelement children liek relation, fixup and
		/// transformer.
		/// </summary>
		/// <param name="node">Node to read values from</param>
		/// <param name="element">Element to set values on</param>
		protected void handleCommonDataElementChildren(XmlNode node, DataElement element)
		{
			foreach (XmlNode child in node.ChildNodes)
			{
				switch (child.Name)
				{
					case "Relation":
						handleRelation(child, element);
						break;

					case "Fixup":
						element.fixup = handleFixup(child, element);
						break;

					case "Transformer":
						element.transformer = handleTransformer(child, element);
						break;
				}
			}
		}

		/// <summary>
		/// Handle parsing child data types into containers.
		/// </summary>
		/// <param name="node">XmlNode tor read children elements from</param>
		/// <param name="element">Element to add items to</param>
		protected void handleDataElementContainer(XmlNode node, DataElementContainer element)
		{
			foreach (XmlNode child in node.ChildNodes)
			{
				switch (child.Name)
				{
					case "Block":
						element.Add(handleBlock(child, element));
						break;

					case "Choice":
						element.Add(handleChoice(child, element));
						break;

					case "String":
						element.Add(handleString(child, element));
						break;

					case "Number":
						element.Add(handleNumber(child, element));
						break;

					case "Blob":
						element.Add(handleBlob(child, element));
						break;

					case "Flags":
						element.Add(handleFlags(child, element));
						break;

					case "Custom":
						throw new NotSupportedException("Implement custom types");
				}
			}
		}

		protected Choice handleChoice(XmlNode node, DataElementContainer parent)
		{
			Choice choice = new Choice();

			// First name
			if (hasXmlAttribute(node, "name"))
				choice.name = getXmlAttribute(node, "name");
			
			handleCommonDataElementAttributes(node, choice);
			handleCommonDataElementChildren(node, choice);
			handleDataElementContainer(node, choice);

			// Array
			if (hasXmlAttribute(node, "minOccurs") || hasXmlAttribute(node, "maxOccurs"))
			{
				Dom.Array array = new Dom.Array();
				array.Add(choice);
				array.name = choice.name;
			}

			return choice;
		}

		protected Dom.String handleString(XmlNode node, DataElementContainer parent)
		{
			Dom.String str = new Dom.String();

			if (hasXmlAttribute(node, "name"))
				str.name = getXmlAttribute(node, "name");

			if (hasXmlAttribute(node, "length"))
				throw new NotSupportedException("Implement length attribute on String");

			if (hasXmlAttribute(node, "nullTerminated"))
				throw new NotSupportedException("Implement nullTerminated attribute on String");

			if (hasXmlAttribute(node, "type"))
				throw new NotSupportedException("Implement type attribute on String");

			if (hasXmlAttribute(node, "padCharacter"))
				throw new NotSupportedException("Implement padCharacter attribute on String");

			if (hasXmlAttribute(node, "lengthType"))
				throw new NotSupportedException("Implement lengthType attribute on String");

			if (hasXmlAttribute(node, "tokens"))
				throw new NotSupportedException("Implement tokens attribute on String");

			if (hasXmlAttribute(node, "analyzer"))
				throw new NotSupportedException("Implement analyzer attribute on String");

			handleCommonDataElementAttributes(node, str);
			handleCommonDataElementChildren(node, str);

			return str;
		}

		protected Number handleNumber(XmlNode node, DataElementContainer parent)
		{
			Number num = new Number();

			if (hasXmlAttribute(node, "name"))
				num.name = getXmlAttribute(node, "name");

			if (hasXmlAttribute(node, "signed"))
				num.Signed = getXmlAttributeAsBool(node, "signed", false);

			if (hasXmlAttribute(node, "size"))
			{
				uint size;
				try
				{
					size = uint.Parse(getXmlAttribute(node, "size"));
				}
				catch
				{
					throw new PeachException("Error, " + num.name + " size attribute is not valid number.");
				}

				if (size < 1 || size > 64)
					throw new PeachException(string.Format("Error, unsupported size {0} for element {1}.", size, num.name));

				num.Size = size;
			}

			if (hasXmlAttribute(node, "endian"))
			{
				string endian = getXmlAttribute(node, "endian").ToLower();
				switch (endian)
				{
					case "little":
						num.LittleEndian = true;
						break;
					case "big":
						num.LittleEndian = false;
						break;
					case "network":
						num.LittleEndian = false;
						break;
					default:
						throw new PeachException(
							string.Format("Error, unsupported value \"{0}\" for \"endian\" attribute on field \"{1}\".", endian, num.name));
				}
			}

			handleCommonDataElementAttributes(node, num);
			handleCommonDataElementChildren(node, num);

			return num;
		}

		protected Blob handleBlob(XmlNode node, DataElementContainer parent)
		{
			Blob blob = new Blob();

			if (hasXmlAttribute(node, "length"))
				throw new NotSupportedException("Implement length attribute on Blob");

			if (hasXmlAttribute(node, "lengthType"))
				throw new NotSupportedException("Implement lengthType attribute on Blob");

			handleCommonDataElementAttributes(node, blob);
			handleCommonDataElementChildren(node, blob);

			return blob;
		}

		protected Flags handleFlags(XmlNode node, DataElementContainer parent)
		{
			Flags flags = new Flags();

			handleCommonDataElementAttributes(node, flags);
			handleCommonDataElementChildren(node, flags);

			return flags;
		}

		protected void handleRelation(XmlNode node, DataElement parent)
		{
			switch (getXmlAttribute(node, "type"))
			{
				case "size":
					if (hasXmlAttribute(node, "of"))
					{
						SizeRelation rel = new SizeRelation();
						rel.OfName = getXmlAttribute(node, "of");
						parent.relations.Add(rel);
					}
					else if (hasXmlAttribute(node, "from"))
					{
						SizeRelation rel = new SizeRelation();
						rel.FromName = getXmlAttribute(node, "from");
						parent.relations.Add(rel);
					}
					break;
				
				case "count":
					if (hasXmlAttribute(node, "of"))
					{
						CountRelation rel = new CountRelation();
						rel.OfName = getXmlAttribute(node, "of");
						parent.relations.Add(rel);
					}
					else if (hasXmlAttribute(node, "from"))
					{
						CountRelation rel = new CountRelation();
						rel.FromName = getXmlAttribute(node, "from");
						parent.relations.Add(rel);
					}
					break;
				
				case "offset":
					if (hasXmlAttribute(node, "of"))
					{
						OffsetRelation rel = new OffsetRelation();
						rel.OfName = getXmlAttribute(node, "of");
						parent.relations.Add(rel);
					}
					else if (hasXmlAttribute(node, "from"))
					{
						OffsetRelation rel = new OffsetRelation();
						rel.FromName = getXmlAttribute(node, "from");
						parent.relations.Add(rel);
					}
					break;
				
				case "when":
					{
						WhenRelation rel = new WhenRelation();
						rel.WhenExpression = getXmlAttribute(node, "when");
						parent.relations.Add(rel);
					}
					break;
				
				default:
					throw new ApplicationException("Unknown relation type found '"+
						getXmlAttribute(node, "type")+"'.");
			}
		}

		protected Fixup handleFixup(XmlNode node, DataElement parent)
		{
			if (!hasXmlAttribute(node, "class"))
				throw new PeachException("Fixup element has no 'class' attribute [" + node.OuterXml + "].");

			string cls = getXmlAttribute(node, "class");
			Dictionary<string, object> args = new Dictionary<string, object>();

			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name == "Param")
				{
					throw new NotSupportedException("Implement Fixup Paramters");
				}
				else
				{
					throw new PeachException("Fixup element has invalid child element '"+child.Name+"' at [" + node.OuterXml + "].");
				}
			}

			// Create fixup object here!
			throw new NotSupportedException("Finish implementing Fixups!");

			return null;
		}

		protected Transformer handleTransformer(XmlNode node, DataElement parent)
		{
			return null;
		}

#endregion

		#region State Model

		protected StateModel handleStateModel(XmlNode node)
		{
			string name = getXmlAttribute(node, "name");
			string initialState = getXmlAttribute(node, "initial");
			StateModel stateModel = new StateModel();
			stateModel.name = name;

			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name == "State")
				{
					State state = handleState(child, stateModel);
					stateModel.states.Add(state);
					if (state.name == initialState)
						stateModel.initialState = state;
				}
			}

			if (stateModel.initialState == null)
				throw new PeachException("Error, did not locate inital ('" + initialState + "') for state model '" + name + "'.");

			return stateModel;
		}

		protected State handleState(XmlNode node, StateModel parent)
		{
			State state = new State();
			state.parent = parent;
			state.name = getXmlAttribute(node, "name");

			foreach (XmlNode child in node.ChildNodes)
			{
				if (child.Name == "Action")
				{
					Action action = handleAction(child, state);
					state.actions.Add(action);
				}
			}

			return state;
		}

		protected Action handleAction(XmlNode node, State parent)
		{
			throw new NotImplementedException("Todo");
		}

		#endregion

	}
}

// end

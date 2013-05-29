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
//  Michael Eddington (mike@dejavusecurity.com)
//	Adam Cecchetti (adam@dejavusecurity.com) 

// $Id$

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NUnit.Framework;
using NUnit.Framework.Constraints;

using Peach.Core;
using Peach.Core.Dom;
using Peach.Core.Analyzers;
using Peach.Core.Cracker;
using Peach.Core.IO;

namespace Peach.Core.Test.Analyzers
{
    [TestFixture]
    internal class StringTokenTests
    {
        [Test]
        public void BasicTest()
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
                         "	<DataModel name=\"TheDataModel\">" +
                         "		<String name=\"TheString\">" +
                         "			<Analyzer class=\"StringToken\" />" +
                         "		</String>" +
                         "	</DataModel>" +
                         "</Peach>";

            PitParser parser = new PitParser();
            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            BitStream data = new BitStream();
            data.LittleEndian();
            data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("Hello World"));
            data.SeekBits(0, SeekOrigin.Begin);

            DataCracker cracker = new DataCracker();
            cracker.CrackData(dom.dataModels[0], data);

            Assert.AreEqual("Hello World", ASCIIEncoding.ASCII.GetString((byte[]) dom.dataModels[0][0].InternalValue));
            Assert.AreEqual(3, ((DataElementContainer) ((DataElementContainer) dom.dataModels[0][0])[0]).Count);
        }

        [Test]
        public void NoTokens()
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
                         "	<DataModel name=\"TheDataModel\">" +
                         "		<String name=\"TheString\">" +
                         "			<Analyzer class=\"StringToken\" />" +
                         "		</String>" +
                         "	</DataModel>" +
                         "</Peach>";

            PitParser parser = new PitParser();
            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            BitStream data = new BitStream();
            data.LittleEndian();
            data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("HelloWorld"));
            data.SeekBits(0, SeekOrigin.Begin);

            DataCracker cracker = new DataCracker();
            cracker.CrackData(dom.dataModels[0], data);

            Assert.AreEqual("HelloWorld", dom.dataModels[0][0].InternalValue.ToString());
            Assert.AreEqual(1, ((DataElementContainer) dom.dataModels[0]).Count);
        }

        [Test]
        public void JustTokens()
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
                         "	<DataModel name=\"TheDataModel\">" +
                         "		<String name=\"TheString\">" +
                         "			<Analyzer class=\"StringToken\" />" +
                         "		</String>" +
                         "	</DataModel>" +
                         "</Peach>";

            PitParser parser = new PitParser();
            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            BitStream data = new BitStream();
            data.LittleEndian();
            data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("\r\n\"'[]{}<>` \t.,~!@#$%^?&*_=+-|\\:;/"));
            data.SeekBits(0, SeekOrigin.Begin);

            DataCracker cracker = new DataCracker();
            cracker.CrackData(dom.dataModels[0], data);

            Assert.AreEqual("\r\n\"'[]{}<>` \t.,~!@#$%^?&*_=+-|\\:;/",
                            ASCIIEncoding.ASCII.GetString((byte[]) dom.dataModels[0][0].InternalValue));
            Assert.AreEqual(3, ((DataElementContainer) ((DataElementContainer) dom.dataModels[0][0])[0]).Count);
        }

        [Test]
        public void SingleTokenMiddle()
        {
             string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
                         "	<DataModel name=\"TheDataModel\">" +
                         "		<String name=\"TheString\">" +
                         "			<Analyzer class=\"StringToken\" />" +
                         "		</String>" +
                         "	</DataModel>" +
                         "</Peach>";

            PitParser parser = new PitParser();
            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            BitStream data = new BitStream();
            data.LittleEndian();
            data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("AAAA:AAAA"));
            data.SeekBits(0, SeekOrigin.Begin);

            DataCracker cracker = new DataCracker();
            cracker.CrackData(dom.dataModels[0], data);

            Assert.AreEqual("AAAA:AAAA",
                            ASCIIEncoding.ASCII.GetString((byte[]) dom.dataModels[0][0].InternalValue));

						Assert.AreEqual("AAAA",((DataElementContainer) ((DataElementContainer) dom.dataModels[0][0])[0])[0].InternalValue.ToString());           
            Assert.AreEqual(":", ((DataElementContainer) ((DataElementContainer) dom.dataModels[0][0])[0])[1].InternalValue.ToString());           
            Assert.AreEqual("AAAA", ((DataElementContainer) ((DataElementContainer) dom.dataModels[0][0])[0])[2].InternalValue.ToString());           

            Assert.AreEqual(3, ((DataElementContainer) ((DataElementContainer) dom.dataModels[0][0])[0]).Count);           
        }

        [Test]
        public void SingleTokenEnd()
        {
             string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
                         "	<DataModel name=\"TheDataModel\">" +
                         "		<String name=\"TheString\">" +
                         "			<Analyzer class=\"StringToken\" />" +
                         "		</String>" +
                         "	</DataModel>" +
                         "</Peach>";

            PitParser parser = new PitParser();
            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            BitStream data = new BitStream();
            data.LittleEndian();
            data.WriteBytes(ASCIIEncoding.ASCII.GetBytes("AAAAAAAA:"));
            data.SeekBits(0, SeekOrigin.Begin);

            DataCracker cracker = new DataCracker();
            cracker.CrackData(dom.dataModels[0], data);

            Assert.AreEqual("AAAAAAAA:",
                            ASCIIEncoding.ASCII.GetString((byte[]) dom.dataModels[0][0].InternalValue));

						Assert.AreEqual("AAAAAAAA",((DataElementContainer) ((DataElementContainer) dom.dataModels[0][0])[0])[0].InternalValue.ToString());           
            Assert.AreEqual(":", ((DataElementContainer) ((DataElementContainer) dom.dataModels[0][0])[0])[1].InternalValue.ToString());           

            Assert.AreEqual(3, ((DataElementContainer) ((DataElementContainer) dom.dataModels[0][0])[0]).Count);           
        }

        [Test]
        public void SingleTokenBegin()
        {
             string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
                         "	<DataModel name=\"TheDataModel\">" +
                         "		<String name=\"TheString\">" +
                         "			<Analyzer class=\"StringToken\" />" +
                         "		</String>" +
                         "	</DataModel>" +
                         "</Peach>";

            PitParser parser = new PitParser();
            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            BitStream data = new BitStream();
            data.LittleEndian();
            data.WriteBytes(ASCIIEncoding.ASCII.GetBytes(":AAAAAAAA"));
            data.SeekBits(0, SeekOrigin.Begin);

            DataCracker cracker = new DataCracker();
            cracker.CrackData(dom.dataModels[0], data);

            Assert.AreEqual(":AAAAAAAA",
                            ASCIIEncoding.ASCII.GetString((byte[]) dom.dataModels[0][0].InternalValue));

            Assert.AreEqual(":", ((DataElementContainer) ((DataElementContainer) dom.dataModels[0][0])[0])[1].InternalValue.ToString());           
						Assert.AreEqual("AAAAAAAA",((DataElementContainer) ((DataElementContainer) dom.dataModels[0][0])[0])[2].InternalValue.ToString());           

            Assert.AreEqual(3, ((DataElementContainer) ((DataElementContainer) dom.dataModels[0][0])[0]).Count);           
        }

        [Test]
        public void StringTokenAllWhiteSpaceTokens()
        {
              string xml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<Peach>\n" +
                         "	<DataModel name=\"TheDataModel\">" +
                         "		<String name=\"TheString\">" +
                         "			<Analyzer class=\"StringToken\" />" +
                         "		</String>" +
                         "	</DataModel>" +
                         "</Peach>";

            PitParser parser = new PitParser();
            Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

            BitStream data = new BitStream();
            data.LittleEndian();
            data.WriteBytes(ASCIIEncoding.ASCII.GetBytes(@"\t\n\r "));
            data.SeekBits(0, SeekOrigin.Begin);

            DataCracker cracker = new DataCracker();
            cracker.CrackData(dom.dataModels[0], data);

            Assert.AreEqual(@"\t\n\r ",
                            ASCIIEncoding.ASCII.GetString((byte[]) dom.dataModels[0][0].InternalValue));
            Assert.AreEqual(3, ((DataElementContainer) ((DataElementContainer) dom.dataModels[0][0])[0]).Count);                      
        }
    }
}

// end

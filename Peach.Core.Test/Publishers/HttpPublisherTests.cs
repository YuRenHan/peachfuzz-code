﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using Peach.Core;
using Peach.Core.Analyzers;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace Peach.Core.Test.Publishers
{

	class SimpleHttpListener
	{

		// This example requires the System and System.Net namespaces. 
		public void Listen(string[] prefixes)
		{
			if (!HttpListener.IsSupported)
			{
				Console.WriteLine("HttpListener Class is not Supported.");
				return;
			}
			// URI prefixes are required, 
			// for example "http://localhost:8080/index/".
			if (prefixes == null || prefixes.Length == 0)
				throw new ArgumentException("prefixes");

			// Create a listener.
			HttpListener listener = new HttpListener();
			// Add the prefixes. 
			foreach (string s in prefixes)
			{
				listener.Prefixes.Add(s);
			}
			listener.Start();
			while(true)
			{
				try
				{
					// Note: The GetContext method blocks while waiting for a request. 
					HttpListenerContext context = listener.GetContext();
					HttpListenerRequest request = context.Request;
					// Obtain a response object.
					HttpListenerResponse response = context.Response;

					// Construct a response. 
					string responseString = "Hello World Too";
					byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
					// Get a response stream and write the response to it.
					response.ContentLength64 = buffer.Length;
					System.IO.Stream output = response.OutputStream;
					output.Write(buffer, 0, buffer.Length);
					// You must close the output stream.
					output.Close();
				}
				catch (ThreadInterruptedException e)
				{
					listener.Stop();
				}

			}
		}
	}


	[TestFixture]
	class HttpPublisherTests : DataModelCollector
	{
		public string template = @"
<Peach>
	<DataModel name=""TheDataModel"">
		<String name=""str"" value=""Hello World""/>
	</DataModel>

	<DataModel name=""TheDataModel2"">
		<String name=""str"" value=""Hello World Too""/>
	</DataModel>

	<StateModel name=""ClientState"" initialState=""InitialState"">
		<State name=""InitialState"">
			<Action name=""Send"" type=""output"">
				<DataModel ref=""TheDataModel""/>
			</Action>
			<Action name=""Recv"" type=""input"">
				<DataModel ref=""TheDataModel2""/>
			</Action>
		</State>
	</StateModel>

<Test name=""Default"">
		<StateModel ref=""ClientState""/>
		<Publisher class=""Http"">
			<Param name=""Method"" value=""{0}""/>
			<Param name=""Url"" value=""{1}""/>
		</Publisher>
	</Test>

</Peach>
";
		
		public void HttpClient(string method)
		{
			ushort port = TestBase.MakePort(56000, 57000);
			string url = "http://localhost:" + port.ToString() + "/";

			SimpleHttpListener listener = new SimpleHttpListener();
			string[] prefixes = new string[1] {url};
			Thread lThread = new Thread(() => listener.Listen(prefixes));

			lThread.Start();
			try
			{
				string xml = string.Format(template, method, url);

				PitParser parser = new PitParser();
				Dom.Dom dom = parser.asParser(null, new MemoryStream(ASCIIEncoding.ASCII.GetBytes(xml)));

				RunConfiguration config = new RunConfiguration();
				config.singleIteration = true;

				Engine e = new Engine(null);
				e.startFuzzing(dom, config);

				Assert.AreEqual(2, actions.Count);

				var de1 = actions[0].dataModel.find("TheDataModel.str");
				Assert.NotNull(de1);
				var de2 = actions[1].dataModel.find("TheDataModel2.str");
				Assert.NotNull(de2);

				string send = (string)de1.DefaultValue;
				string recv = (string)de2.DefaultValue;

				Assert.AreEqual("Hello World", send);
				Assert.AreEqual("Hello World Too", recv);
			}
			finally
			{
				if (lThread.IsAlive)
					lThread.Interrupt();
			}
		}

		[Test]
		public void HttpClientGet()
		{
			// Test TcpClient deals with itself initiating shutdown
			HttpClient("GET");
		}

		[Test]
		public void HttpClientPost()
		{
			// Test TcpListener deals with client initiating shutdown
			HttpClient("POST");
		}

	}
}
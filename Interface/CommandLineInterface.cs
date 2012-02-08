#region License
/*
Microsoft Public License (Ms-PL)
MonoGame - Copyright © 2009-2012 The MonoGame Team

All rights reserved.

This license governs use of the accompanying software. If you use the software,
you accept this license. If you do not accept the license, do not use the
software.

1. Definitions

The terms "reproduce," "reproduction," "derivative works," and "distribution"
have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the
software.

A "contributor" is any person that distributes its contribution under this
license.

"Licensed patents" are a contributor's patent claims that read directly on its
contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the
license conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free copyright license to reproduce its
contribution, prepare derivative works of its contribution, and distribute its
contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license
conditions and limitations in section 3, each contributor grants you a
non-exclusive, worldwide, royalty-free license under its licensed patents to
make, have made, use, sell, offer for sale, import, and/or otherwise dispose of
its contribution in the software or derivative works of the contribution in the
software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any
contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you
claim are infringed by the software, your patent license from such contributor
to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all
copyright, patent, trademark, and attribution notices that are present in the
software.

(D) If you distribute any portion of the software in source code form, you may
do so only under this license by including a complete copy of this license with
your distribution. If you distribute any portion of the software in compiled or
object code form, you may only do so under a license that complies with this
license.

(E) The software is licensed "as-is." You bear the risk of using it. The
contributors give no express warranties, guarantees or conditions. You may have
additional consumer rights under your local laws which this license cannot
change. To the extent permitted under your local laws, the contributors exclude
the implied warranties of merchantability, fitness for a particular purpose and
non-infringement
*/
#endregion License

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Xsl;

using NDesk.Options;

using NUnit.Core;
using NUnit.Util;
using System.Text.RegularExpressions;

namespace MonoGame.Tests
{
	class CommandLineInterface : EventListener, ITestFilter
	{
		public static void RunMain (string [] args)
		{
			bool launchResults = true;
			bool performXslTransform = true;
			bool showHelp = false;

			var directory = Directory.GetCurrentDirectory ();

			string xmlResultsFile = Path.Combine (directory, "test_results.xml");
			string transformedResultsFile = Path.Combine (directory, "test_results.html");
			string xslTransformPath = Path.Combine ("Resources", "tests.xsl");
			string stdoutFile = Path.Combine (directory, "stdout.txt");
			var filters = new List<RegexFilter> ();
			var optionSet = new OptionSet () {
				{ "i|include=", x => filters.Add(RegexFilter.Parse(x, FilterAction.Include)) },
				{ "x|exclude=", x => filters.Add(RegexFilter.Parse(x, FilterAction.Exclude)) },
				{ "no-launch-results", x => launchResults = false },
				{ "no-xsl-transform", x => performXslTransform = false },
				{ "xml-results=", x => xmlResultsFile = x },
				{ "xsl-transform=", x => xslTransformPath = x },
				{ "transformed-results=", x => transformedResultsFile = x },
				{ "stdout=", x => stdoutFile = x },
//				{ "v|verbose",  x => ++verbose },
				{ "h|?|help",   x => showHelp = true },
			};

			List<string> extra = optionSet.Parse (args);
			if (extra.Count > 0)
				Console.WriteLine (
					"Ignoring {0} unrecognized argument(s): {1}",
					extra.Count, string.Join (", ", extra));

			if (showHelp) {
				ShowHelp (optionSet);
				System.Threading.Thread.Sleep (3000);
				return;
			}

			CoreExtensions.Host.InitializeService ();

			var assembly = Assembly.GetExecutingAssembly ();

			var simpleTestRunner = new SimpleTestRunner ();
			TestPackage package = new TestPackage (assembly.GetName ().Name);
			package.Assemblies.Add (assembly.Location);
			if (!simpleTestRunner.Load (package)) {
				Console.WriteLine ("Could not find the tests.");
				return;
			}

			var cli = new CommandLineInterface (filters);

			var result = simpleTestRunner.Run (cli, cli);

			var resultWriter = new XmlResultWriter (xmlResultsFile);
			resultWriter.SaveTestResult (result);

			if (performXslTransform) {
				var transform = new XslTransform ();
				transform.Load (xslTransformPath);
				transform.Transform (xmlResultsFile, transformedResultsFile);
			}

			File.WriteAllText (stdoutFile, cli._stdoutStandin.ToString ());

			if (performXslTransform && launchResults)
				System.Diagnostics.Process.Start (transformedResultsFile);
		}

		private static void ShowHelp (OptionSet optionSet)
		{
			string executableName = Path.GetFileName (
				Assembly.GetExecutingAssembly ().Location);
			Console.WriteLine ("Usage: {0} [OPTIONS]+", executableName);
			Console.WriteLine ("Options:");
			optionSet.WriteOptionDescriptions (Console.Out);
		}

		private TextWriter _stdoutStandin;
		private StreamWriter _stdout;
		private List<RegexFilter> _filters;
		private CommandLineInterface (IEnumerable<RegexFilter> filters)
		{
			_stdoutStandin = new StringWriter ();
			Console.SetOut (_stdoutStandin);
			_stdout = new StreamWriter (Console.OpenStandardOutput ());
			_stdout.AutoFlush = true;

			if (filters == null)
				_filters = new List<RegexFilter> ();
			else
				_filters = new List<RegexFilter> (filters);
		}

		public void RunStarted (string name, int testCount)
		{
			_stdout.WriteLine("Run Started: {0}", name);
		}

		public void RunFinished (Exception exception)
		{
			// Error
			_stdout.WriteLine ();
		}

		public void RunFinished (TestResult result)
		{
			// Success
			_stdout.WriteLine ();
		}

		public void SuiteFinished (TestResult result)
		{
			// Console.WriteLine("SuiteFinished");
		}

		public void SuiteStarted (TestName testName)
		{
			// Console.WriteLine("SuiteStarted");
		}

		public void TestStarted (TestName testName)
		{
			_stdoutStandin.WriteLine(testName.FullName);
			// Console.WriteLine("Test {0}", testName.FullName);
		}

		public void TestFinished (TestResult result)
		{
			char output;
			switch (result.ResultState) {
			case ResultState.Cancelled:
				output = 'C';
				break;
			case ResultState.Error:
				output = 'E';
				break;
			case ResultState.Failure:
				output = 'F';
				break;
			case ResultState.Ignored:
				output = 'I';
				break;
			case ResultState.Inconclusive:
				output = '?';
				break;
			case ResultState.NotRunnable:
				output = 'N';
				break;
			case ResultState.Skipped:
				output = 'S';
				break;
			default:
			case ResultState.Success:
				output = '.';
				break;
			}

			_stdout.Write (output);

			_stdoutStandin.WriteLine("Finished: " + result.FullName);
			_stdoutStandin.WriteLine();
		}

		public void TestOutput (TestOutput testOutput)
		{
			// Console.WriteLine("TestOutput");
		}

		public void UnhandledException (Exception exception)
		{
			// Console.WriteLine("UnhandledException");
		}

		#region ITestFilter Implementation

		public bool IsEmpty
		{
			get { return false; }
		}

		public bool Match (ITest test)
		{
			return false;
		}

		public bool Pass (ITest test)
		{
			if (test.IsSuite)
				return true;

			foreach (var filter in _filters)
				if (!filter.Pass (test.TestName.FullName))
					return false;

			return true;
		}

		#endregion ITestFilter Implementation

		class RegexFilter {
			private readonly Regex _regex;
			private readonly FilterAction _filterAction;

			public RegexFilter (Regex regex, FilterAction filterAction)
			{
				if (regex == null)
					throw new ArgumentNullException ("regex");
				_regex = regex;
				_filterAction = filterAction;
			}

			public bool Pass (string name)
			{
				var match = _regex.Match (name);

				if (_filterAction == FilterAction.Exclude)
					return !match.Success;
				return match.Success;
			}

			public static RegexFilter Parse (string s, FilterAction filterAction)
			{
				if (string.IsNullOrEmpty (s))
					throw new ArgumentException ("Filter string cannot be null or empty", "s");

				string pattern;
				if (s.Length > 1 && s.StartsWith ("/") && s.EndsWith ("/")) {
					pattern =s.Substring(1, s.Length - 2); 
				} else {
					pattern = string.Join ("", ".*", s, ".*");
				}

				var regex = new Regex (pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
				return new RegexFilter (regex, filterAction);
			}
		}

		enum FilterAction {
			Include,
			Exclude
		}
	}
}

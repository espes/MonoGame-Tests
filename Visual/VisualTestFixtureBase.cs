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
non-infringement.
*/
#endregion License

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;

using IGameComponent = Microsoft.Xna.Framework.IGameComponent;

using NUnit.Framework;

using MonoGame.Tests.Components;

namespace MonoGame.Tests.Visual {
	class VisualTestFixtureBase {
		private VisualTestGame _game;
		protected VisualTestGame Game { get { return _game; } }

		[SetUp]
		public void SetUp ()
		{
			Paths.SetStandardWorkingDirectory();
			_game = new VisualTestGame ();
		}

		[TearDown]
		public void TearDown ()
		{
			_game.Dispose ();
			_game = null;
		}

		protected static void WriteFrameDiffsAndAssertComparisonPassed (
			IEnumerable<FrameComparisonResult> results, string diffDirectory,
			float threshold, int expectedCount)
		{
			WriteFrameComparisonDiffs (results, diffDirectory);
			AssertFrameComparisonResultsPassed (results, threshold, expectedCount);
		}

		protected static void AssertFrameComparisonResultsPassed (
			IEnumerable<FrameComparisonResult> results, float threshold, int expectedCount)
		{
			var allResults = new List<FrameComparisonResult> ();
			var failedResults = new List<FrameComparisonResult> ();
			foreach (var result in results) {
				allResults.Add (result);
				if (result.Similarity < threshold)
					failedResults.Add (result);
			}

			if (allResults.Count != expectedCount)
				Assert.Fail (
					"Expected {0} frame comparison result(s), but found {1}",
					expectedCount, allResults.Count);

			WriteComparisonResultReport (allResults, threshold);

			if (failedResults.Count > 0) {
				Assert.Fail (
					"{0} of {1} frames failed the similarity test.",
					failedResults.Count, allResults.Count);
			}
		}

		private static void WriteComparisonResultReport (
			IEnumerable<FrameComparisonResult> results, float threshold)
		{
			Console.WriteLine ("Required similarity: {0:0.####}", threshold);
			foreach (var result in results)
				Console.WriteLine (
					"Similarity: {0:0.####}, Capture: {1}, Reference: {2}",
					result.Similarity, result.CapturedImagePath, result.ReferenceImagePath);
		}

		protected static void WriteFrameComparisonDiffs(
			IEnumerable<FrameComparisonResult> results, string directory)
		{
			try {
				Directory.CreateDirectory (directory);
			} catch (IOException) { }

			foreach (var result in results) {

				string diffFileName = string.Format(
					"diff-{0}-{1}.png",
					Path.GetFileNameWithoutExtension(result.ReferenceImagePath),
					Path.GetFileNameWithoutExtension(result.CapturedImagePath));

				string diffOutputPath = Path.Combine(directory, diffFileName);

				using (var a = (Bitmap)Bitmap.FromFile(result.ReferenceImagePath))
				using (var b = (Bitmap)Bitmap.FromFile(result.CapturedImagePath))
				using (var diff = CreateDiff(a, b)) {
					diff.Save(diffOutputPath);
				}
			}
		}

		private static Bitmap CreateDiff(Bitmap a, Bitmap b)
		{
			int minWidth, maxWidth, minHeight, maxHeight;

			MathUtility.MinMax (a.Width, b.Width, out minWidth, out maxWidth);
			MathUtility.MinMax (a.Height, b.Height, out minHeight, out maxHeight);

			var diff = new Bitmap(maxWidth, maxHeight);

			var aData = a.LockBits(
				new Rectangle(0, 0, a.Width, a.Height),
				ImageLockMode.ReadOnly,
				PixelFormat.Format32bppArgb);
			var bData = b.LockBits(
				new Rectangle(0, 0, b.Width, b.Height),
				ImageLockMode.ReadOnly,
				PixelFormat.Format32bppArgb);
			var diffData = diff.LockBits(
				new Rectangle(0, 0, diff.Width, diff.Height),
				ImageLockMode.ReadWrite,
				PixelFormat.Format32bppArgb);

			try {
				CreateDiff(aData, bData, diffData);
			} finally {
				a.UnlockBits(aData);
				b.UnlockBits(bData);
				diff.UnlockBits(diffData);
			}

			return diff;
		}

		private static unsafe void CreateDiff(
			BitmapData aData, BitmapData bData, BitmapData diffData)
		{
			int minWidth, maxWidth, minHeight, maxHeight;

			MathUtility.MinMax (aData.Width, bData.Width, out minWidth, out maxWidth);
			MathUtility.MinMax (aData.Height, bData.Height, out minHeight, out maxHeight);

			var pRowA = (byte*)aData.Scan0;
			var pRowB = (byte*)bData.Scan0;
			var pRowDiff = (byte*)diffData.Scan0;

			for (int y = 0; y < minHeight; ++y) {

				var pPixelA = (PixelArgb*)pRowA;
				var pPixelB = (PixelArgb*)pRowB;
				var pPixelDiff = (PixelArgb*)pRowDiff;

				for (int x = 0; x < minWidth; ++x) {
					pPixelDiff->B = (byte) (pPixelA->B ^ pPixelB->B);
					pPixelDiff->G = (byte) (pPixelA->G ^ pPixelB->G);
					pPixelDiff->R = (byte) (pPixelA->R ^ pPixelB->R);
					pPixelDiff->A = 0xff;
					// Ignore alpha.  If alpha diffs are
					// needed, a special strategy will have
					// to be devised, since XOR'ing two
					// opaque pixels will cause a totally
					// transparent pixel and hide any other
					// difference.

					pPixelA++;
					pPixelB++;
					pPixelDiff++;
				}

				pRowA += aData.Stride;
				pRowB += bData.Stride;
				pRowDiff += diffData.Stride;
			}
		}

		//routines to do simple testing of drawing components
		protected void TestComponents(string testImageName,
		                              IGameComponent[] components,
		                              string frameFolder,
		                              int framesToDraw = 1)
		{
			foreach (var component in components)
			{
				Game.Components.Add (component);
			}

			var frameComparer = new FrameCompareComponent (
				Game, x => true,
				testImageName + "-{0:00}.png",
				Paths.ReferenceImage (frameFolder),
				Paths.CapturedFrame (frameFolder)) {
					{ new PixelDeltaFrameComparer (), 1 },
				};
			Game.Components.Add (frameComparer);

			Game.ExitCondition = x => x.DrawNumber > framesToDraw;
			Game.Run ();

			WriteFrameComparisonDiffs (
				frameComparer.Results,
				Paths.CapturedFrameDiff (frameFolder));
			AssertFrameComparisonResultsPassed (
				frameComparer.Results, Constants.StandardRequiredSimilarity, framesToDraw);
		}

		protected void TestComponents(IGameComponent[] components,
		                              string frameFolder,
		                              int framesToDraw = 1)
		{
			var stackTrace = new System.Diagnostics.StackTrace ();
			var name = stackTrace.GetFrame (1).GetMethod ().Name;

			TestComponents (name, components, frameFolder, framesToDraw);
		}

		protected void TestComponent(string testImageName,
		                             IGameComponent component,
		                             string frameFolder,
		                             int framesToDraw = 1)
		{
			TestComponents(testImageName, new IGameComponent[] {component}, frameFolder, framesToDraw);
		}

		protected void TestComponent(IGameComponent component,
		                             string frameFolder,
		                             int framesToDraw = 1)
		{
			var stackTrace = new System.Diagnostics.StackTrace ();
			var name = stackTrace.GetFrame (1).GetMethod ().Name;

			TestComponents(name, new IGameComponent[] {component}, frameFolder, framesToDraw);
		}
	}
}

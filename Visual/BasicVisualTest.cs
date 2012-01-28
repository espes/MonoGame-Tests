﻿#region License
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
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;

using NUnit.Framework;

using MonoGame.Tests.Components;

namespace MonoGame.Tests.Visual {
	[TestFixture]
	class BasicVisualTest : VisualTestFixtureBase {

		private const string ClearFolder = "Clear";
		[Test, RequiresSTA]
		public void Clear ()
		{
			var colors = new Color [] {
				Color.Red,
				Color.Orange,
				Color.Yellow,
				Color.Green,
				Color.Blue,
				Color.Indigo,
				Color.Violet
			};

			Game.Components.Add (new ClearComponent (Game) {
				ColorFunction = x => colors[x.DrawNumber - 1]
			});

			var frameComparer = new FrameCompareComponent(
				Game, x => true,
				"frame-{0:00}.png",
				Paths.ReferenceImage(ClearFolder),
				Paths.CapturedFrame(ClearFolder)) {
					{ new PixelDeltaFrameComparer(), 1 },
				};
			Game.Components.Add (frameComparer);

			Game.ExitCondition = x => x.DrawNumber > colors.Length;
			Game.Run ();

			WriteFrameComparisonDiffs(
				frameComparer.Results,
				Paths.CapturedFrameDiff(ClearFolder));
			AssertFrameComparisonResultsPassed (
				frameComparer.Results, Constants.StandardRequiredSimilarity, colors.Length);
		}

		private const string LabelledFrameFolder = "LabelledFrame";
		[Test, RequiresSTA]
		public void Labelled_frame ()
		{
			const int FramesToDraw = 5;

			Game.Components.Add (new ClearComponent (Game) { ColorFunction = x => Color.Red });
			Game.Components.Add (new DrawFrameNumberComponent (Game));

			var frameComparer = new FrameCompareComponent(
				Game, x => true,
				"frame-{0:00}.png",
				Paths.ReferenceImage(LabelledFrameFolder),
				Paths.CapturedFrame(LabelledFrameFolder)) {
					{ new PixelDeltaFrameComparer(), 1 },
				};
			Game.Components.Add (frameComparer);

			Game.ExitCondition = x => x.DrawNumber > FramesToDraw;
			Game.Run ();

			WriteFrameComparisonDiffs(
				frameComparer.Results,
				Paths.CapturedFrameDiff(LabelledFrameFolder));
			AssertFrameComparisonResultsPassed (
				frameComparer.Results, Constants.StandardRequiredSimilarity, FramesToDraw);
		}

		private const string ImplicitDrawOrderFolder = "ImplicitDrawOrder";
		[Test, RequiresSTA]
		public void DrawOrder_falls_back_to_order_of_addition_to_Game ()
		{
			const int FramesToDraw = 4;

			Game.Components.Add (new ClearComponent (Game) { ColorFunction = x => Color.CornflowerBlue });
			Game.Components.Add (new ImplicitDrawOrderComponent (Game));

			var frameComparer = new FrameCompareComponent(
				Game, x => true,
				"frame-{0:00}.png",
				Paths.ReferenceImage(ImplicitDrawOrderFolder),
				Paths.CapturedFrame(ImplicitDrawOrderFolder)) {
					{ new PixelDeltaFrameComparer(), 1 },
				};
			Game.Components.Add (frameComparer);

			Game.ExitCondition = x => x.DrawNumber > FramesToDraw;
			Game.Run ();

			WriteFrameComparisonDiffs(
				frameComparer.Results,
				Paths.CapturedFrameDiff(ImplicitDrawOrderFolder));

			AssertFrameComparisonResultsPassed (
				frameComparer.Results, Constants.StandardRequiredSimilarity, FramesToDraw);
		}
	}
}

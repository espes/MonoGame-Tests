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
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGame.Tests.Components {
	class Draw2DComponent : VisualTestDrawableGameComponent {
		private SpriteBatch _spriteBatch;
		private Texture2D _texture;
		//private SpriteFont _font;
		private float _size;
		private float _rotation;
		private float _clippingSize = 0.0f;
		private Color _alphaColor = Color.White;

		public Draw2DComponent (Game game)
			: base (game)
		{
		}

		protected override void LoadContent ()
		{
			base.LoadContent ();

			_spriteBatch = new SpriteBatch (Game.GraphicsDevice);
			_texture = Game.Content.Load<Texture2D> (Paths.Texture ("MonoGameIcon"));
			//_font = Game.Content.Load<SpriteFont> (Paths.Font ("Default"));
		}

		protected override void UnloadContent ()
		{
			base.UnloadContent ();

			if (_spriteBatch != null) {
				_spriteBatch.Dispose ();
				_spriteBatch = null;
			}

			_texture = null;
			//_font = null;
		}

		protected override void UpdateOncePerDraw (GameTime gameTime)
		{
			base.UpdateOncePerDraw (gameTime);

			_size += 0.5f;
			if (_size > 150)
				_size = 0;

			_rotation += 0.1f;
			if (_rotation > MathHelper.TwoPi)
				_rotation = 0.0f;

			_clippingSize += 0.5f;
			if (_clippingSize > Game.GraphicsDevice.Viewport.Width)
				_clippingSize = 0.0f;
		}

		public override void Draw (GameTime gameTime)
		{
			base.Draw (gameTime);

			// Draw without blend
			_spriteBatch.Begin (SpriteSortMode.Deferred, BlendState.Opaque);
			_spriteBatch.Draw (_texture, new Vector2 (250, 20), Color.White);
			_spriteBatch.End ();

			// Draw with additive blend
			_spriteBatch.Begin (SpriteSortMode.Deferred, BlendState.Additive);
			_spriteBatch.Draw (_texture, new Vector2 (250, 110), Color.White);
			_spriteBatch.Draw (_texture, new Vector2 (260, 120), Color.White);
			_spriteBatch.End ();

			_spriteBatch.Begin ();


			// Normal draw
			// TODO _spriteBatch.Draw (ball, new Vector2 (200,300), Color.White);	
			// TODO _spriteBatch.Draw (ball, new Vector2 (200,300), null, Color.Yellow, 0.0f, new Vector2 (5,5), 1.0f, SpriteEffects.None, 0);	

			// Normal draw
			_spriteBatch.Draw (_texture, new Vector2 (10, 390), Color.White);
			// Draw stretching
			_spriteBatch.Draw (_texture, new Rectangle (0, 0, (int) _size, (int) _size), Color.White);
			// Draw with Filter Color
			_spriteBatch.Draw (_texture, new Vector2 (120, 120), Color.Red);
			// Draw rotated
			_spriteBatch.Draw (_texture, new Rectangle (100, 300, _texture.Width, _texture.Height), null, Color.White, _rotation, new Vector2 (_texture.Width / 2, _texture.Height / 2), SpriteEffects.None, 0);
			// Draw _texture section
			_spriteBatch.Draw (_texture, new Vector2 (200, 200), new Rectangle (20, 20, 40, 40), Color.White);
			// Draw _texture section and scale
			_spriteBatch.Draw (_texture, new Rectangle (10, 120, (int) _size, (int) _size), new Rectangle (20, 20, 40, 40), Color.White);
			// Alpha blending
			_spriteBatch.Draw (_texture, new Vector2 (140, 0), _alphaColor);
			// Flip horizontaly
			_spriteBatch.Draw (_texture, new Rectangle (80, 390, _texture.Width, _texture.Height), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally, 0);
			// Flip verticaly
			_spriteBatch.Draw (_texture, new Rectangle (150, 390, _texture.Width, _texture.Height), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.FlipVertically, 0);
			// Flip horizontaly and verticaly
			_spriteBatch.Draw (_texture, new Rectangle (220, 390, _texture.Width, _texture.Height), null, Color.White, 0.0f, Vector2.Zero, SpriteEffects.FlipHorizontally | SpriteEffects.FlipVertically, 0);
			_spriteBatch.End ();

			base.Draw (gameTime);

			// FIXME: This scissoring code is not valid in XNA. It
			//        complains about RasterizerState being
			//        immutable after it's bound to a
			//        GraphicsDevice.  MonoGame probably should to,
			//        rather than allowing mutation.

			// Now let's try some scissoring
			//_spriteBatch.Begin ();
			//_spriteBatch.GraphicsDevice.ScissorRectangle = new Rectangle (50, 40, (int) _clippingSize, (int) _clippingSize);
			//_spriteBatch.GraphicsDevice.RasterizerState.ScissorTestEnable = true;
			//_spriteBatch.Draw (_texture, new Rectangle (50, 40, 320, 40), Color.White);
			//_spriteBatch.DrawString (_font, "Scissor Clipping Test", new Vector2 (50, 40), Color.Red);
			//_spriteBatch.End ();



			//_spriteBatch.GraphicsDevice.RasterizerState.ScissorTestEnable = false;
		}
	}
}

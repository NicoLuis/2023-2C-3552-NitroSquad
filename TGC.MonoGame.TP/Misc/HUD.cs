using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace TGC.MonoGame.TP.Misc
{
	class HUD
	{
		public const string ContentFolderSpriteFonts = "SpriteFonts/";
		public const string ContentFolderTextures = "Textures/";

		public ContentManager Content;
		public GraphicsDevice GraphicsDevice;
		public SpriteBatch SpriteBatch;
		public Texture2D HUDTexture;
		private SpriteFont Font;
		private Vector2 HUDPosition;

		public HUD (ContentManager content, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
		{
			Content = content;
			GraphicsDevice = graphicsDevice;
			SpriteBatch = spriteBatch;
			Font = Content.Load<SpriteFont>(ContentFolderSpriteFonts + "CascadiaCode/CascadiaCodePL");
			HUDTexture = Content.Load<Texture2D>(ContentFolderTextures + "basehud");
			HUDPosition = new Vector2(GraphicsDevice.Viewport.Width / 2f, GraphicsDevice.Viewport.Height - 75);
		}

		public void Draw(bool hasShield, string remainingMissiles, float remainingHealth, float remainingBoost, float currentSpeed, string currentGear)
		{
			DepthStencilState dss = GraphicsDevice.DepthStencilState;
			SpriteBatch.Begin();
			string speed = ((int)currentSpeed).ToString();

			if (hasShield)
				DrawHUDPart(new Rectangle(0, 50, 60, 50), Vector2.UnitX * -190f);
			else
				DrawHUDPart(new Rectangle(0, 0, 60, 50), Vector2.UnitX * -190f);

			if (remainingMissiles == "x0")
				DrawHUDPart(new Rectangle(60, 0, 64, 50), Vector2.UnitX * -140f, remainingMissiles, new Vector2(30f, 12.5f), Color.DarkGray);
			else
				DrawHUDPart(new Rectangle(60, 50, 64, 50), Vector2.UnitX * -140f, remainingMissiles, new Vector2(30f, 12.5f), Color.Black);

			DrawHUDPart(new Rectangle(123, 0, (int)(255 * (1 - remainingHealth)), 25), new Vector2((int)(255 * remainingHealth) - 78f, 0f));
			DrawHUDPart(new Rectangle(123 + (int)(255 * (1 - remainingHealth)), 50, (int)(255 * remainingHealth), 25), new Vector2(- 78f, 0f));

			DrawHUDPart(new Rectangle(123 + (int)(255 * remainingBoost), 25, (int)(255 * (1 - remainingBoost)), 25), new Vector2((int)(255 * remainingBoost) - 78f, 25f));
			DrawHUDPart(new Rectangle(123, 75, (int)(255 * remainingBoost), 25), new Vector2(-78f, 25f));

			DrawHUDPart(new Rectangle(380, 0, 62, 50), Vector2.UnitX * 140f, speed, new Vector2(27.5f - Font.MeasureString(speed).X / 2, 12.5f), currentSpeed > 200f ? Color.Red : Color.LightGoldenrodYellow);
			DrawHUDPart(new Rectangle(440, 0, 60, 50), Vector2.UnitX * 190f, currentGear, new Vector2(20f, 12.5f), Color.LightYellow);

			SpriteBatch.End();
			GraphicsDevice.DepthStencilState = dss;
		}

		private void DrawHUDPart(Rectangle rectangle, Vector2 offset, string text = null, Vector2? textOffset = null, Color? color = null)
		{
			SpriteBatch.Draw(
				texture: HUDTexture,
				position: HUDPosition + offset,
				sourceRectangle: rectangle,
				color: Color.White,
				rotation: 0.0f,
				origin: new Vector2(0, 0),
				scale: 1f,
				effects: SpriteEffects.None,
				layerDepth: 0.0f
			);
			if(text != null)
				SpriteBatch.DrawString(Font, text, HUDPosition + offset + textOffset.Value, color.Value);
		}
	}
}

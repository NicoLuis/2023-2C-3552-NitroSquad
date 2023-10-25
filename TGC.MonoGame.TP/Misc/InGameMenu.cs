using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Scenarios;
using BepuUtilities;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using static System.Formats.Asn1.AsnWriter;
using TGC.MonoGame.TP.Cameras;
using System.Reflection.Metadata.Ecma335;

namespace TGC.MonoGame.TP.Misc
{
	class InGameMenu
	{
		public const string ContentFolder3D = "Models/";
		public const string ContentFolderEffects = "Effects/";
		public const string ContentFolderMusic = "Music/";
		public const string ContentFolderSounds = "Sounds/";
		public const string ContentFolderSpriteFonts = "SpriteFonts/";
		public const string ContentFolderTextures = "Textures/";

		public bool IsActive = false;
		public bool Exit = false;

		private ContentManager Content;
		private GraphicsDevice GraphicsDevice;
		private SpriteBatch SpriteBatch;
		private Texture2D OptionImage;
		private SpriteFont Font;
		private SoundEffect ChangeOptionSound;
		private SoundEffect EnterSound;

		private float _keyPressedCooldown = 0.1f;
		private InGameMenuOption _currentSelectedOption = InGameMenuOption.Continue;
		public CameraType CameraType = 0;
		public CarColor MainCarColor = 0;
		public bool GodMode = false; 
		public bool DisplayGizmos = false;

		public InGameMenu(ContentManager content, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
		{
			Content = content;
			GraphicsDevice = graphicsDevice;
			SpriteBatch = spriteBatch;

			Font = Content.Load<SpriteFont>(ContentFolderSpriteFonts + "CascadiaCode/CascadiaCodePL");
			OptionImage = Content.Load<Texture2D>(ContentFolderTextures + "menuoption");
			ChangeOptionSound = Content.Load<SoundEffect>(ContentFolderSounds + "changeoption");
			EnterSound = Content.Load<SoundEffect>(ContentFolderSounds + "enter");
		}

		public void Update(GameTime gameTime, CameraType cameraType, CarColor mainCarColor, bool godmode, bool gizmos)
		{
			float elapsedTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

			CameraType = cameraType;
			MainCarColor = mainCarColor;
			GodMode = godmode;
			DisplayGizmos = gizmos;

			ChangeOption(elapsedTime, InGameMenuOption.Continue, InGameMenuOption.ExitGame);
		}

		public void Draw()
		{
			DepthStencilState dss = GraphicsDevice.DepthStencilState;
			SpriteBatch.Begin();

			//DrawBackground();

			DrawMenuOption(new Vector2(0f, -250f), InGameMenuOption.Continue);
			DrawMenuOption(new Vector2(0f, -150f), InGameMenuOption.ChangeView);
			DrawMenuOption(new Vector2(0f, -50f), InGameMenuOption.ShowGizmos);
			//DrawMenuOption(new Vector2(0f, -50f), InGameMenuOption.ChangeColor);
			DrawMenuOption(new Vector2(0f, 50f), InGameMenuOption.GodMod);
			DrawMenuOption(new Vector2(0f, 150f), InGameMenuOption.ExitGame);

			SpriteBatch.End();
			GraphicsDevice.DepthStencilState = dss;
		}

		public void DrawMenuOption(Vector2 offset, InGameMenuOption option)
		{
			string text = GetText(option);
			Vector2 screenCenter = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height) / 2f;

			SpriteBatch.Draw(
				texture: OptionImage,
				position: screenCenter - new Vector2(OptionImage.Width * 2.5f / 2f, 75f) + offset,
				sourceRectangle: new Rectangle(0, 0, 200, 75),
				color: Color.White,
				rotation: 0.0f,
				origin: new Vector2(0, 0),
				scale: new Vector2(2.5f, 1f),
				effects: SpriteEffects.None,
				layerDepth: 0.0f
			);
			SpriteBatch.DrawString(Font, text, screenCenter - new Vector2(Font.MeasureString(text).X / 2f, 50f) + offset, _currentSelectedOption == option ? Color.White : Color.Red);
		}

		public void DrawBackground()
		{
			Vector2 screenCenter = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height) / 2f;

			SpriteBatch.Draw(
				texture: OptionImage,
				position: screenCenter - new Vector2(OptionImage.Width, OptionImage.Height * 2.5f),
				sourceRectangle: new Rectangle(0, 0, 200, 75),
				color: Color.White,
				rotation: 0.0f,
				origin: new Vector2(0f, 0f),
				scale: new Vector2(3f, 7.5f),
				effects: SpriteEffects.None,
				layerDepth: 0.0f
			);
		}

		public void ChangeOption(float elapsedTime, InGameMenuOption minOption, InGameMenuOption maxOption)
		{
			_keyPressedCooldown = MathF.Min(_keyPressedCooldown + elapsedTime, 0.1f);
			if (_keyPressedCooldown != 0.1f) return;
			_keyPressedCooldown = 0f;

			KeyboardState keyboardState = Keyboard.GetState();

			if (keyboardState.IsKeyDown(Keys.Up) && keyboardState.IsKeyUp(Keys.Down))
			{
				ChangeOptionSound.Play();
				_currentSelectedOption = (int)(_currentSelectedOption - 1) < (int)minOption ? _currentSelectedOption = minOption : _currentSelectedOption - 1;
			}

			if (keyboardState.IsKeyDown(Keys.Down) && keyboardState.IsKeyUp(Keys.Up))
			{
				ChangeOptionSound.Play();
				_currentSelectedOption = (int)(_currentSelectedOption + 1) > (int)maxOption ? _currentSelectedOption = maxOption : _currentSelectedOption + 1;
			}

			if (keyboardState.IsKeyDown(Keys.Left) && keyboardState.IsKeyUp(Keys.Right))
			{
				ChangeOptionSound.Play();
				switch (_currentSelectedOption)
				{
					case InGameMenuOption.ChangeView:
						CameraType--;
						CameraType = CameraType < CameraType.Ortographic ? CameraType.FarBehindUnlocked : CameraType;
						break;
					case InGameMenuOption.GodMod:
						GodMode = !GodMode;
						break;
					case InGameMenuOption.ShowGizmos:
						DisplayGizmos = !DisplayGizmos;
						break;
						//case InGameMenuOption.ChangeColor:
						//	MainCarColor--;
						//	break;
				}
			}

			if (keyboardState.IsKeyDown(Keys.Right) && keyboardState.IsKeyUp(Keys.Left))
			{
				ChangeOptionSound.Play();
				switch (_currentSelectedOption)
				{
					case InGameMenuOption.ChangeView:
						CameraType++;
						CameraType = CameraType > CameraType.FarBehindUnlocked ? CameraType.Ortographic : CameraType;
						break;
					case InGameMenuOption.GodMod:
						GodMode = !GodMode;
						break;
					case InGameMenuOption.ShowGizmos:
						DisplayGizmos = !DisplayGizmos;
						break;
					//case InGameMenuOption.ChangeColor:
					//	MainCarColor++;
					//	break;
				}
			}

			if (keyboardState.IsKeyDown(Keys.Enter))
			{
				EnterSound.Play();
				switch (_currentSelectedOption)
				{
					case InGameMenuOption.Continue:
						IsActive = false;
						break;
					case InGameMenuOption.ExitGame:
						Exit = true;
						break;
				}
			}
		}

		public string GetText(InGameMenuOption option)
		{
			switch (option)
			{
				case InGameMenuOption.Continue:
					return "Continue";
				case InGameMenuOption.ChangeView:
					return GetCameraText();
				case InGameMenuOption.ShowGizmos:
					return "Gizmos: " + (DisplayGizmos ? "Enabled" : "Disabled");
				//case InGameMenuOption.ChangeColor:
				//	return "ChangeColor";
				case InGameMenuOption.GodMod:
					return "God Mode: " + (GodMode ? "Enabled" : "Disabled");
				case InGameMenuOption.ExitGame:
					return "Exit Game";
			}
			return "";
		}

		public string GetCameraText()
		{
			switch (CameraType)
			{
				case CameraType.Ortographic:
					return "Ortographic";
				case CameraType.Behind:
					return "Behind";
				case CameraType.FarBehind:
					return "Far Behind";
				case CameraType.FarBehindUnlocked:
					return "Far Behind (Unlocked)";
			}
			return "";
		}
	}

	public enum InGameMenuOption
	{
		Continue = 0,
		ChangeView = 1,
		ShowGizmos = 2,
		//ChangeColor = 2,
		GodMod = 3,
		ExitGame = 4
	}
}

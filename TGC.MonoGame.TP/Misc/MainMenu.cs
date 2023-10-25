using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using TGC.MonoGame.TP.Scenarios;
using BepuUtilities;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

namespace TGC.MonoGame.TP.Misc
{
	class MainMenu
	{
		public const string ContentFolder3D = "Models/";
		public const string ContentFolderEffects = "Effects/";
		public const string ContentFolderMusic = "Music/";
		public const string ContentFolderSounds = "Sounds/";
		public const string ContentFolderSpriteFonts = "SpriteFonts/";
		public const string ContentFolderTextures = "Textures/";

		public bool IsActive = true;
		public bool Exit = false;

		private MainMenuScene Scene;
		private ContentManager Content;
		private GraphicsDevice GraphicsDevice;
		private SpriteBatch SpriteBatch;
		private Texture2D OptionImage;
		private SpriteFont Font;
		private Song Song;
		private SoundEffect ChangeOptionSound;
		private SoundEffect EnterSound;

		private float _keyPressedCooldown = 0.1f;
		private float _buttonAnimationTime = 0f;
		private MenuOption _currentSelectedOption = MenuOption.Begin;
		private MenuState _currentState = MenuState.Main;
		private bool isSongPlaying = false;

		public MainMenu(ContentManager content, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
		{
			Content = content;
			GraphicsDevice = graphicsDevice;
			SpriteBatch = spriteBatch;

			Font = Content.Load<SpriteFont>(ContentFolderSpriteFonts + "CascadiaCode/CascadiaCodePL");
			OptionImage = Content.Load<Texture2D>(ContentFolderTextures + "menuoption");
			Song = Content.Load<Song>(ContentFolderMusic + "MenuSong");
			ChangeOptionSound = Content.Load<SoundEffect>(ContentFolderSounds + "changeoption");
			EnterSound = Content.Load<SoundEffect>(ContentFolderSounds + "enter");
			Scene = new MainMenuScene(content, graphicsDevice, spriteBatch);
		}

		public void Update(GameTime gameTime)
		{
			float elapsedTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);

			switch (_currentState)
			{
				case MenuState.Main:
					ChangeOption(elapsedTime, MenuOption.Begin, MenuOption.Exit);
					break;
				case MenuState.Settings:
					ChangeOption(elapsedTime, MenuOption.ChangeColor, MenuOption.SettingsBack);
					break;
				case MenuState.ChangeColor:
					ChangeOption(elapsedTime, MenuOption.SelectColor, MenuOption.ChangeColorBack);
					break;
				case MenuState.Controls:
					ChangeOption(elapsedTime, MenuOption.ControlsBack, MenuOption.ControlsBack);
					break;
			}

			if (!isSongPlaying)
			{
				MediaPlayer.Play(Song);
				isSongPlaying = true;
			}

			Scene.Update(gameTime);
		}

		public void Draw()
		{
			DepthStencilState dss = GraphicsDevice.DepthStencilState;
			SpriteBatch.Begin();

			Scene.Draw();

			switch (_currentState)
			{
				case MenuState.Main:
					DrawMenuOption("Begin", new Vector2(-300f, 300f * (0.5f - MathF.Min(_buttonAnimationTime, 0.5f))), MenuOption.Begin);
					DrawMenuOption("Settings", new Vector2(0f, 300f * (0.75f - MathF.Min(_buttonAnimationTime, 0.75f))), MenuOption.Settings);
					DrawMenuOption("Exit", new Vector2(300f, 300f * (1f - _buttonAnimationTime)), MenuOption.Exit);
					break;
				case MenuState.Settings:
					DrawMenuOption("Change Color", new Vector2(-300f, 300f * (0.5f - MathF.Min(_buttonAnimationTime, 0.5f))), MenuOption.ChangeColor);
					DrawMenuOption("Controls", new Vector2(0f, 300f * (0.75f - MathF.Min(_buttonAnimationTime, 0.75f))), MenuOption.Controls);
					DrawMenuOption("Back", new Vector2(300f, 300f * (1f - _buttonAnimationTime)), MenuOption.SettingsBack);
					break;
				case MenuState.ChangeColor:
					DrawMenuOption("Change", new Vector2(-300f, 300f * (0.5f - MathF.Min(_buttonAnimationTime, 0.5f))), MenuOption.SelectColor);
					DrawMenuOption("Back", new Vector2(300f, 300f * (0.75f - MathF.Min(_buttonAnimationTime, 0.75f))), MenuOption.ChangeColorBack);
					break;
				case MenuState.Controls:
					DrawControls();
					DrawMenuOption("Back", new Vector2(0f, 300f * (0.75f - MathF.Min(_buttonAnimationTime, 0.75f))), MenuOption.ControlsBack);
					break;
			}

			SpriteBatch.End();
			GraphicsDevice.DepthStencilState = dss;
		}

		public void DrawMenuOption(string text, Vector2 offset, MenuOption option)
		{
			Vector2 screenCenter = new Vector2(GraphicsDevice.Viewport.Width / 2f, GraphicsDevice.Viewport.Height - GraphicsDevice.Viewport.Height / 3f);

			SpriteBatch.Draw(
				texture: OptionImage,
				position: screenCenter + new Vector2(-100f, 125f) + offset,
				sourceRectangle: new Rectangle(0, 0, 200, 75),
				color: Color.White,
				rotation: 0.0f,
				origin: new Vector2(0, 0),
				scale: 1f,
				effects: SpriteEffects.None,
				layerDepth: 0.0f
			);
			SpriteBatch.DrawString(Font, text, screenCenter + new Vector2(-Font.MeasureString(text).X / 2f, 150f) + offset, _currentSelectedOption == option ? Color.White : Color.Red);
		}

		public void DrawControls()
		{
			Vector2 screenCenter = new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height) / 2f;

			SpriteBatch.Draw(
				texture: OptionImage,
				position: screenCenter - new Vector2(OptionImage.Width * 1f, OptionImage.Height * 2.5f),
				sourceRectangle: new Rectangle(0, 0, 200, 75),
				color: Color.White,
				rotation: 0.0f,
				origin: new Vector2(0f, 0f),
				scale: new Vector2(2f, 5f),
				effects: SpriteEffects.None,
				layerDepth: 0.0f
			);
			string[] textList = new string[] {
				"Accelerate: W",
				"Brake/Reverse: S",
				"Turn Left: A",
				"Turn Right: D",
				"Jump: Space",
				"Shoot: Left Ctrl",
				"Boost: Left Shift",
				"Exit Game: Esc"
			};
			for (int i = 0; i < textList.Length; i++)
			{
				float offsetY = (Font.MeasureString(textList[i]).Y + 5f) * i - OptionImage.Height * 2.5f;
				SpriteBatch.DrawString(Font, textList[i], screenCenter + new Vector2(-Font.MeasureString(textList[i]).X / 2f, 50f + offsetY), Color.White);
			}
		}

		public void ChangeOption(float elapsedTime, MenuOption minOption, MenuOption maxOption)
		{
			_keyPressedCooldown = MathF.Min(_keyPressedCooldown + elapsedTime, 0.1f);
			if (_buttonAnimationTime != 1f) _buttonAnimationTime = MathF.Min(_buttonAnimationTime + elapsedTime, 1f);
			if (_keyPressedCooldown != 0.1f) return;
			_keyPressedCooldown = 0f;

			KeyboardState keyboardState = Keyboard.GetState();

			if (keyboardState.IsKeyDown(Keys.Left) && keyboardState.IsKeyUp(Keys.Right))
			{
				ChangeOptionSound.Play();
				_currentSelectedOption = (int)(_currentSelectedOption - 1) < (int)minOption ? _currentSelectedOption = minOption : _currentSelectedOption - 1;
			}

			if (keyboardState.IsKeyDown(Keys.Right) && keyboardState.IsKeyUp(Keys.Left))
			{
				ChangeOptionSound.Play();
				_currentSelectedOption = (int)(_currentSelectedOption + 1) > (int)maxOption ? _currentSelectedOption = maxOption : _currentSelectedOption + 1;
			}

			if (keyboardState.IsKeyDown(Keys.Enter))
			{
				EnterSound.Play();
				_buttonAnimationTime = 0f;
				switch (_currentSelectedOption)
				{
					case MenuOption.Begin:
						MediaPlayer.Stop();
						IsActive = false;
						break;
					case MenuOption.Settings:
						_currentState = MenuState.Settings;
						_currentSelectedOption = MenuOption.ChangeColor;
						break;
					case MenuOption.Exit:
						MediaPlayer.Stop();
						Exit = false;
						break;
					case MenuOption.ChangeColor:
						_currentState = MenuState.ChangeColor;
						_currentSelectedOption = MenuOption.ChangeColorBack;
						break;
					case MenuOption.Controls:
						_currentState = MenuState.Controls;
						_currentSelectedOption = MenuOption.ControlsBack;
						break;
					case MenuOption.SettingsBack:
						_currentState = MenuState.Main;
						_currentSelectedOption = MenuOption.Settings;
						break;
					case MenuOption.SelectColor:
						Scene.ChangeCarColor(1); 
						break;
					case MenuOption.ChangeColorBack:
						_currentState = MenuState.Settings;
						_currentSelectedOption = MenuOption.ChangeColor;
						break;
					case MenuOption.ControlsBack:
						_currentState = MenuState.Settings;
						_currentSelectedOption = MenuOption.Controls;
						break;
				}
			}
		}
	}

	public enum MenuOption
	{
		Begin = 0,
		Settings = 1,
		Exit = 2,
		ChangeColor = 3,
		Controls = 4,
		SettingsBack = 5,
		ControlsBack = 6,
		SelectColor = 7,
		ChangeColorBack = 8
	}
	public enum MenuState
	{
		Main = 0,
		Settings = 1,
		Controls = 2,
		ChangeColor = 3
	}
	public enum CarColor
	{
		Red = 0,
		Green = 1,
		Blue = 2,
		Purple = 3,
		Black = 4,
	}
}

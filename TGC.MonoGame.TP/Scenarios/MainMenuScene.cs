using BepuPhysics.Constraints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using TGC.MonoGame.TP.Cars;
using TGC.MonoGame.TP.Misc.Colliders;
using TGC.MonoGame.TP.Misc;
using TGC.MonoGame.TP.Misc.Gizmos;
using TGC.MonoGame.TP.Misc.Primitives;
using TGC.MonoGame.TP.Cameras;
using Microsoft.Xna.Framework.Input;
using Microsoft.VisualBasic.FileIO;

namespace TGC.MonoGame.TP.Scenarios
{
	class MainMenuScene
	{
		public const string ContentFolder3D = "Models/";
		public const string ContentFolderEffects = "Effects/";
		public const string ContentFolderTextures = "Textures/";

		// Floor
		private QuadPrimitive Quad;
		private Matrix FloorWorld;
		private Texture2D FloorTexture;
		private Effect FloorEffect;

		private GraphicsDevice _graphicsDevice;
		private ContentManager _content;
		private SpriteBatch _spriteBatch;
		private MainMenuCamera Camera;

		#region car
		private Model CarModel;
		private Effect CarEffect;
		private Matrix CarWorldMatrix;
		private ModelBone FrontRightWheelBone;
		private ModelBone FrontLeftWheelBone;
		private ModelBone BackLeftWheelBone;
		private ModelBone BackRightWheelBone;
		private ModelBone CarBone;
		private Matrix FrontRightWheelTransform;
		private Matrix FrontLeftWheelTransform;
		private Matrix BackLeftWheelTransform;
		private Matrix BackRightWheelTransform;
		private Matrix CarTransform;
		private Matrix[] BoneTransforms;
		private Texture2D BaseTexture_Black;
		private Texture2D BaseTexture_Red;
		private Texture2D BaseTexture_Green;
		private Texture2D BaseTexture_Blue;
		private Texture2D BaseTexture_Purple;
		private Texture2D NormalTexture;
		private Texture2D RoughnessTexture;
		private Texture2D MetallicTexture;
		private Texture2D AoTexture;
		public CarColor SelectedColor;
		#endregion car

		public MainMenuScene(ContentManager content, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch)
		{
			_graphicsDevice = graphicsDevice;
			_content = content;
			_spriteBatch = spriteBatch;

			Camera = new MainMenuCamera(_graphicsDevice.Viewport);

			InitializeFloor(content);
			InitializeCar(content);
		}

		public void Update(GameTime gameTime)
		{
			Camera.Update(gameTime);
			UpdateFloor();
			UpdateCar();
		}

		public void Draw()
		{
			_graphicsDevice.Clear(Color.Black);
			_graphicsDevice.DepthStencilState = DepthStencilState.Default;
			DrawFloor();
			DrawCar();
		}


		#region Floor
		private void InitializeFloor(ContentManager content)
		{
			Quad = new QuadPrimitive(_graphicsDevice);
			FloorWorld = Matrix.CreateScale(1500f);
			FloorTexture = content.Load<Texture2D>(ContentFolderTextures + "asphalt_road");
			FloorEffect = content.Load<Effect>(ContentFolderEffects + "FloorShader");

			FloorEffect.Parameters["baseTexture"].SetValue(FloorTexture);

			FloorEffect.Parameters["ambientColor"].SetValue(Vector3.One);
			FloorEffect.Parameters["diffuseColor"].SetValue(Vector3.One);// new Vector3(1f, 1f, 1f));
			FloorEffect.Parameters["specularColor"].SetValue(Vector3.One);
			FloorEffect.Parameters["Ka"].SetValue(0.1f);
			FloorEffect.Parameters["Kd"].SetValue(0.5f);
			FloorEffect.Parameters["Ks"].SetValue(0.2f);
			FloorEffect.Parameters["shininess"].SetValue(18.0f);
			FloorEffect.Parameters["Tiling"].SetValue(Vector2.One);

			FloorEffect.Parameters["World"].SetValue(FloorWorld);
			FloorEffect.Parameters["InverseTransposeWorld"].SetValue(Matrix.Transpose(Matrix.Invert(FloorWorld)));
		}
		private void UpdateFloor()
		{
			FloorEffect.Parameters["eyePosition"].SetValue(Camera.PreviousCameraPosition);
			FloorEffect.Parameters["lightPosition"].SetValue(new Vector3(-500f, 500f, -1000f));
		}
		private void DrawFloor()
		{
			FloorEffect.Parameters["WorldViewProjection"].SetValue(FloorWorld * Camera.View * Camera.Projection);
			Quad.Draw(FloorEffect);
		}
		#endregion Floor

		#region Car
		private void InitializeCar(ContentManager content)
		{
			CarModel = content.Load<Model>(ContentFolder3D + "racingcar/racingcar");
			BaseTexture_Black = content.Load<Texture2D>(ContentFolder3D + "racingcar/Vehicle_basecolor");
			BaseTexture_Red = content.Load<Texture2D>(ContentFolder3D + "racingcar/Vehicle_basecolor_red");
			BaseTexture_Green = content.Load<Texture2D>(ContentFolder3D + "racingcar/Vehicle_basecolor_green");
			BaseTexture_Blue = content.Load<Texture2D>(ContentFolder3D + "racingcar/Vehicle_basecolor_blue");
			BaseTexture_Purple = content.Load<Texture2D>(ContentFolder3D + "racingcar/Vehicle_basecolor_purple");
			CarEffect = content.Load<Effect>(ContentFolderEffects + "RacingCarShader");
			CarWorldMatrix = Matrix.CreateTranslation(Vector3.Zero);

			foreach (var mesh in CarModel.Meshes)
			{
				foreach (var meshPart in mesh.MeshParts)
					meshPart.Effect = CarEffect;
			}

			FrontRightWheelBone = CarModel.Bones["WheelA"];
			FrontLeftWheelBone = CarModel.Bones["WheelB"];
			BackLeftWheelBone = CarModel.Bones["WheelC"];
			BackRightWheelBone = CarModel.Bones["WheelD"];
			CarBone = CarModel.Bones["Car"];
			CarTransform = CarBone.Transform;
			FrontLeftWheelTransform = FrontLeftWheelBone.Transform;
			FrontRightWheelTransform = FrontRightWheelBone.Transform;
			BackLeftWheelTransform = BackLeftWheelBone.Transform;
			BackRightWheelTransform = BackRightWheelBone.Transform;
			BoneTransforms = new Matrix[CarModel.Bones.Count];
		}
		private void UpdateCar()
		{
			CarEffect.Parameters["eyePosition"].SetValue(Camera.PreviousCameraPosition);
			CarEffect.Parameters["lightPosition"].SetValue(new Vector3(-500f, 500f, -1000f));
		}
		private void DrawCar()
		{
			switch (SelectedColor)
			{
				case CarColor.Red:
					CarEffect.Parameters["ModelTexture"].SetValue(BaseTexture_Red);
					break;
				case CarColor.Green:
					CarEffect.Parameters["ModelTexture"].SetValue(BaseTexture_Green);
					break;
				case CarColor.Blue:
					CarEffect.Parameters["ModelTexture"].SetValue(BaseTexture_Blue);
					break;
				case CarColor.Purple:
					CarEffect.Parameters["ModelTexture"].SetValue(BaseTexture_Purple);
					break;
				case CarColor.Black:
					CarEffect.Parameters["ModelTexture"].SetValue(BaseTexture_Black);
					break;
			}

			// Set the world matrix as the root transform of the model.
			CarModel.Root.Transform = CarWorldMatrix;

			// Calculate matrices based on the current animation position.
			var wheelRotationX = Matrix.CreateRotationX(0f);
			var steeringRotationY = Matrix.CreateRotationY(25f * (MathF.PI / 180f));

			// Apply matrices to the relevant bones.
			FrontLeftWheelBone.Transform = wheelRotationX * steeringRotationY * FrontLeftWheelTransform;
			FrontRightWheelBone.Transform = wheelRotationX * steeringRotationY * FrontRightWheelTransform;
			BackLeftWheelBone.Transform = wheelRotationX * BackLeftWheelTransform;
			BackRightWheelBone.Transform = wheelRotationX * BackRightWheelTransform;
			CarBone.Transform = CarTransform;

			// Look up combined bone matrices for the entire model.
			CarModel.CopyAbsoluteBoneTransformsTo(BoneTransforms);
			// For each mesh in the model,
			foreach (var mesh in CarModel.Meshes)
			{
				// Obtain the world matrix for that mesh (relative to the parent)
				var meshWorld = BoneTransforms[mesh.ParentBone.Index];
				CarEffect.Parameters["World"].SetValue(meshWorld);
				CarEffect.Parameters["WorldViewProjection"].SetValue(meshWorld * Camera.View * Camera.Projection);
				CarEffect.Parameters["NormalWorldMatrix"].SetValue(Matrix.Invert(Matrix.Transpose(meshWorld)));
				mesh.Draw();
			}
		}

		public void ChangeCarColor(int offset)
		{
			SelectedColor = (int)(SelectedColor + 1) > 4 ? 0 : SelectedColor + offset;
		}

		#endregion Car

	}
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing;
using System;
using TGC.MonoGame.TP.Misc.Gizmos;
using TGC.MonoGame.TP.Misc.Colliders;
using TGC.MonoGame.TP.Misc;
using Microsoft.Xna.Framework.Audio;
using TGC.MonoGame.TP.Cameras;

namespace TGC.MonoGame.TP.Cars
{
    class RacingCar : BaseCar
	{
		public RacingCar(ContentManager content, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, Matrix startRotation,  Vector3 startPosition)
        {
			Gizmos = new Gizmos();
			Gizmos.LoadContent(graphicsDevice, content);
			ShowGizmos = false;

			GraphicsDevice = graphicsDevice;
			Content = content;
			SpriteBatch = spriteBatch;

			Model = content.Load<Model>(ContentFolder3D + "racingcar/racingcar");
			NormalTexture = content.Load<Texture2D>(ContentFolder3D + "racingcar/Vehicle_normal");
			RoughnessTexture = content.Load<Texture2D>(ContentFolder3D + "racingcar/Vehicle_rougness");
			MetallicTexture = content.Load<Texture2D>(ContentFolder3D + "racingcar/Vehicle_metallic");
			AoTexture = content.Load<Texture2D>(ContentFolder3D + "racingcar/Vehicle_ao");
			Effect = content.Load<Effect>(ContentFolderEffects + "RacingCarShader");
			EngineSound = Content.Load<SoundEffect>(ContentFolderSounds + "engine");
			SoundEffectInstance = EngineSound.CreateInstance();
			SoundEffectInstance.IsLooped = true;
			Rotation = startRotation;
			Position = startPosition;
			Direction = Vector3.Left;

			foreach (var mesh in Model.Meshes)
			{
				foreach (var meshPart in mesh.MeshParts)
					meshPart.Effect = Effect;
			}

			Effect.CurrentTechnique = Effect.Techniques["Full"];
			Effect.Parameters["lightPosition"].SetValue(new Vector3(0.0f, 7500f, 0.0f));
			Effect.Parameters["ambientColor"].SetValue(new Vector3(1f, 1f, 1f));
			Effect.Parameters["diffuseColor"].SetValue(new Vector3(1f, 1f, 1f));
			Effect.Parameters["specularColor"].SetValue(new Vector3(1f, 1f, 1f));
			Effect.Parameters["Ka"].SetValue(0.5f);
			Effect.Parameters["Kd"].SetValue(0.8f);
			Effect.Parameters["Ks"].SetValue(1.0f);
			Effect.Parameters["shininess"].SetValue(15.0f);

			Effect.Parameters["NormalTexture"].SetValue(NormalTexture);
			Effect.Parameters["RoughnessTexture"].SetValue(RoughnessTexture);
			Effect.Parameters["MetallicTexture"].SetValue(MetallicTexture);
			Effect.Parameters["AoTexture"].SetValue(AoTexture);
			Effect.Parameters["Tiling"].SetValue(Vector2.One);

			var temporaryCubeAABB = BoundingVolumesExtensions.CreateAABBFrom(Model);
			BoundingBox = OrientedBoundingBox.FromAABB(temporaryCubeAABB);
			BoundingBox.Center = Vector3.One + startPosition;
			BoundingBox.Orientation = startRotation;

			FrontRightWheelBone = Model.Bones["WheelA"];
            FrontLeftWheelBone = Model.Bones["WheelB"];
            BackLeftWheelBone = Model.Bones["WheelC"];
            BackRightWheelBone = Model.Bones["WheelD"];
            CarBone = Model.Bones["Car"];
            CarTransform = CarBone.Transform;
            FrontLeftWheelTransform = FrontLeftWheelBone.Transform;
            FrontRightWheelTransform = FrontRightWheelBone.Transform;
            BackLeftWheelTransform = BackLeftWheelBone.Transform;
            BackRightWheelTransform = BackRightWheelBone.Transform;
            BoneTransforms = new Matrix[Model.Bones.Count];

			MaxBoost = 7.5f;
			DefaultSteeringSpeed = 0.02f;
			DefaultSteeringRotation = 25f;
			DefaultBrakingForce = 50f;
			DefaultJumpSpeed = 1000f;
			DefaultBoostSpeed = 20f;
			MaxSpeed = new float[8] { 800f, 1f, 1126f, 1860f, 2660f, 3700f, 5020f, 6400f }; // R-N-1-2-3-4-5-6
			Acceleration = new float[8] { 15f, -3f, 10f, 7.5f, 5f, 3f, 2f, 1.5f }; // R-N-1-2-3-4-5-6

			//workaround to load missile content before shooting 1st missile
			MissilesList.Add(new Missile(Position, Direction, Rotation, GraphicsDevice, Content, SpriteBatch));
			MissilesList.RemoveAll(x => x.IsActive);
		}

        public new void Draw(Matrix view, Matrix projection, Vector3 cameraPosition, CameraType cameraType)
		{
			Effect.Parameters["NormalTexture"].SetValue(NormalTexture);
			Effect.Parameters["RoughnessTexture"].SetValue(RoughnessTexture);
			Effect.Parameters["MetallicTexture"].SetValue(MetallicTexture);
			Effect.Parameters["AoTexture"].SetValue(AoTexture);
			Effect.Parameters["Tiling"].SetValue(Vector2.One);
			Effect.Parameters["eyePosition"].SetValue(cameraPosition);
			base.Draw(view, projection, cameraPosition, cameraType);
		}
	}
}
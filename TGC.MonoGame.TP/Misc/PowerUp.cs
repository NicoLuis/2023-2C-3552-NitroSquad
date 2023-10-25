using BepuPhysics.Collidables;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using TGC.MonoGame.TP.Misc.Colliders;
using TGC.MonoGame.TP.Misc.Primitives;

namespace TGC.MonoGame.TP.Misc
{
    class PowerUp
	{
		public const string ContentFolder3D = "Models/";
		public const string ContentFolderEffects = "Effects/";
		public const string ContentFolderTextures = "Textures/";

		private CubePrimitive Cube;
		private Model Model;
		private Matrix[] ModelBoneTransforms;
		private Matrix WorldMatrix;
		private Matrix Rotation;
		private Matrix Scale;
		public OrientedBoundingBox Collider;
		private Texture2D Texture;
		private Effect Effect;
		private Vector3 BoxSize;
		public PowerUpType Type;
		private Random Random;
		private float HidingCooldown = 0f;
		public bool IsActive
		{
			get => HidingCooldown == 0f;
		}

		public PowerUp(Matrix worldMatrix, Vector3 boxSize, GraphicsDevice graphicsDevice, ContentManager content, Random random, PowerUpType? type = null)
		{
			BoxSize = boxSize;
			Rotation = Matrix.CreateRotationY(1f);
			Effect = content.Load<Effect>(ContentFolderEffects + "CubeShader");
			WorldMatrix = worldMatrix;
			Collider = OrientedBoundingBox.FromAABB(new BoundingBox(worldMatrix.Translation - BoxSize / 2f, worldMatrix.Translation + BoxSize / 2f));
			Random = random;

			if (type != null && type == PowerUpType.Wrench)
			{
				Type = type.Value;
				Scale = Matrix.CreateScale(35f);
				Rotation = Matrix.CreateRotationX(MathHelper.Pi + MathHelper.PiOver4);
				Model = content.Load<Model>(ContentFolder3D + "wrench");
				ModelBoneTransforms = new Matrix[Model.Bones.Count];
			}
			else
			{
				Scale = Matrix.Identity;
				Texture = content.Load<Texture2D>(ContentFolderTextures + "powerup");
				Effect = content.Load<Effect>(ContentFolderEffects + "CubeShader");
				Effect.Parameters["Tiling"].SetValue(Vector2.One);
				Type = (PowerUpType)Random.Next(3);
				Cube = new CubePrimitive(graphicsDevice, BoxSize);
			}
		}

		public void Update(float elapsedTime)
		{
			Rotation *= Matrix.CreateRotationY(-MathHelper.PiOver2 * elapsedTime);
			if (IsActive)
			{
				WorldMatrix = Scale * Rotation * Matrix.CreateTranslation(WorldMatrix.Translation);
				Collider = OrientedBoundingBox.FromAABB(new BoundingBox(WorldMatrix.Translation - BoxSize / 2f, WorldMatrix.Translation + BoxSize / 2f));
			}
			else
				HidingCooldown = Math.Max(HidingCooldown - elapsedTime, 0f);
		}

		public void Draw(Matrix view, Matrix projection, Vector3 cameraPosition)
		{
			if (!IsActive) return;

			if (Type == PowerUpType.Wrench)
			{
				Model.Root.Transform = WorldMatrix;
				Model.CopyAbsoluteBoneTransformsTo(ModelBoneTransforms);
				foreach (var mesh in Model.Meshes)
				{
					var meshWorld = ModelBoneTransforms[mesh.ParentBone.Index];
					foreach (BasicEffect effect in mesh.Effects)
					{
						effect.DiffuseColor = new Vector3(1f, 1f, 1f);
						effect.World = meshWorld;
						effect.View = view;
						effect.Projection = projection;
					}
					mesh.Draw();
				}
			}
			else
			{
				Effect.Parameters["Texture"].SetValue(Texture);
				Effect.Parameters["WorldViewProjection"].SetValue(WorldMatrix * view * projection);
				Cube.Draw(Effect);
			}
		}

		public void Hide()
		{
			HidingCooldown = 5f;
			Type = Type == PowerUpType.Wrench ? PowerUpType.Wrench : (PowerUpType) Random.Next(3);
		}
	}

	public enum PowerUpType
	{
		Boost = 0,
		Missiles = 1,
		Shield = 2,
		Wrench = 10
	}
}

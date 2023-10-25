using BepuPhysics.Constraints;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Misc.Colliders;

namespace TGC.MonoGame.TP.Misc
{
	class Missile
	{
		public const string ContentFolder3D = "Models/";
		public const string ContentFolderEffects = "Effects/";
		public const string ContentFolderTextures = "Textures/";

		private Model Model;
		public Matrix[] BoneTransforms;
		private Vector3 Size = new Vector3(100f, 50f, 50f);
		private Matrix WorldMatrix;
		private Matrix Rotation;
		private Vector3 Direction;
		private Vector3 Position;
		public OrientedBoundingBox Collider;
		private Texture2D Texture;
		private Texture2D Explosion;
		private Effect Effect;
		public float Speed = 7000f;
		public float Acceleration = 5000f;
		public bool Exploded = false;
		public bool IsActive = true;
		private SpriteBatch SpriteBatch;
		private int SpriteIndex = 0;
		private float SpriteIndexCooldown = 0f;
		private GraphicsDevice GraphicsDevice;
		private BoundingFrustum BoundingFrustum;

		public Missile(Vector3 initialPosition, Vector3 direction, Matrix rotation, GraphicsDevice graphicsDevice, ContentManager content, SpriteBatch spriteBatch)
		{
			GraphicsDevice = graphicsDevice;
			SpriteBatch = spriteBatch;
			Effect = content.Load<Effect>(ContentFolderEffects + "CubeShader");
			Model = content.Load<Model>(ContentFolder3D + "missile");
			BoneTransforms = new Matrix[Model.Bones.Count];
			Texture = content.Load<Texture2D>(ContentFolderTextures + "missile");
			Explosion = content.Load<Texture2D>(ContentFolderTextures + "explosion");
			Position = initialPosition + Vector3.Up * 75f;
			Direction = direction;
			Rotation = Matrix.CreateRotationZ(MathHelper.PiOver2) * Matrix.CreateRotationY(MathHelper.PiOver2) * rotation;
			Collider = new OrientedBoundingBox(Position, Size / 2f);
			BoundingFrustum = new BoundingFrustum(Matrix.Identity);

			foreach (var mesh in Model.Meshes)
			{
				foreach (var meshPart in mesh.MeshParts)
					meshPart.Effect = Effect;
			}
		}

		public void Update(float elapsedTime, BaseCollider[] colliders)
		{
			if (Exploded)
			{
				SpriteIndexCooldown = MathF.Min(SpriteIndexCooldown + elapsedTime, 0.05f);
				if (SpriteIndexCooldown == 0.05f)
				{
					SpriteIndex++;
					SpriteIndexCooldown = 0f;
				}
				if (SpriteIndex == 25) IsActive = false;
				return;
			}

			Speed += Acceleration * elapsedTime;
			Position += CheckForCollisions(Direction * Speed * elapsedTime, colliders);
			WorldMatrix = Matrix.CreateScale(25f) * Rotation * Matrix.CreateTranslation(Position);
			Collider = new OrientedBoundingBox(Position, Size / 2f);
		}

		public void Draw(Matrix view, Matrix projection, Vector3 cameraPosition, CameraType cameraType)
		{
			BoundingFrustum.Matrix = view * projection;
			if (!Collider.Intersects(BoundingFrustum))
				return;

			if (Exploded)
			{
				Vector3 projectedPosition = GraphicsDevice.Viewport.Project(Vector3.Zero, projection, view, WorldMatrix);
				DepthStencilState dss = GraphicsDevice.DepthStencilState;
				SpriteBatch.Begin();
				SpriteBatch.Draw(
					texture: Explosion,
					position: new Vector2(projectedPosition.X, projectedPosition.Y),
					sourceRectangle: new Rectangle(new Point(128 * (SpriteIndex % 5), 128 * (SpriteIndex / 5)), new Point(128, 128)),
					color: Color.White,
					rotation: 0.0f,
					origin: new Vector2(64f, 110f),
					scale: cameraType == CameraType.Ortographic ? 2f : 5000f / (Position - cameraPosition).Length(),
					effects: SpriteEffects.None,
					layerDepth: 0.0f
				);
				SpriteBatch.End();
				GraphicsDevice.DepthStencilState = dss;
			}
			else
			{
				Model.Root.Transform = WorldMatrix;
				Effect.Parameters["Texture"].SetValue(Texture);
				Model.CopyAbsoluteBoneTransformsTo(BoneTransforms);
				foreach (var mesh in Model.Meshes)
				{
					var meshWorld = BoneTransforms[mesh.ParentBone.Index];
					Effect.Parameters["WorldViewProjection"].SetValue(meshWorld * view * projection);
					mesh.Draw();
				}
			}
		}

		private Vector3 CheckForCollisions(Vector3 positionDelta, BaseCollider[] colliders)
		{
			Collider.Center += positionDelta;
			// Check intersection for every collider
			for (var index = 0; index < colliders.Length; index++)
			{
				if (Collider.Intersects(colliders[index]))
				{
					Exploded = true;
					return Vector3.Zero;
				}
				continue;
			}
			return positionDelta;
		}
	}
}

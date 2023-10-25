using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using TGC.MonoGame.TP.Cameras;
using TGC.MonoGame.TP.Misc;
using TGC.MonoGame.TP.Misc.Colliders;
using TGC.MonoGame.TP.Misc.Gizmos;

namespace TGC.MonoGame.TP.Cars
{
	class BaseCar
	{
		public const string ContentFolder3D = "Models/";
		public const string ContentFolderEffects = "Effects/";
		public const string ContentFolderSounds = "Sounds/";

		#region Properties
		//current state
		private int _currentGear = 1;
		private float _currentSpeed = 0f;
		private float _currentWheelRotation = 0f;
		private float _currentSteeringWheelRotation = 0f;
		private float _currentBodyRotationX = 0f;
		private float _currentBodyRotationZ = 0f;
		private bool _isAccelerating = false;
		private bool _isBraking = false;
		private bool _isUsingBoost = false;
		private bool _isShooting = false;
		private bool _isTurningLeft = false;
		private bool _isTurningRight = false;
		private bool _isJumping = false;
		private bool _godMode = false;
		private float _godModeCooldown = 0f;
		private float _gizmosCooldown = 0f;
		private float _shootCooldown = 0f;
		private float _remainingHealth = 100f;
		private bool _enginestarted = false;

		//Power Ups
		private float _remainingBoost = 0f;  // in seconds
		private bool _hasShield = false;
		private int _remainingMissiles = 0;
		public List<Missile> MissilesList = new List<Missile>();

		//Telemetry
		public bool Shield => _hasShield;
		public string Missiles => _godMode ? "Inf" : "x" + _remainingMissiles.ToString();
		public float Health => _remainingHealth / MaxHealth;
		public float Boost => _remainingBoost / MaxBoost;
		public float Speed => MathF.Abs(_currentSpeed) / 20;
		public string Gear => GetGear();
		public float RPM => _currentSpeed / MaxSpeed[_currentGear];
		public bool GodMode => _godMode;

		//car specs
		public float DefaultSteeringSpeed;
		public float DefaultSteeringRotation;
		public float DefaultBrakingForce;
		public float DefaultJumpSpeed;
		public float DefaultBoostSpeed;
		public float[] MaxSpeed;
		public float[] Acceleration;
		public float MaxBoost = 0f;
		public float MaxHealth = 100f;

		// todo: global
		public const float Gravity = 50f;

		public Effect Effect;
		public Model Model;
		public Matrix World;
		public Matrix Rotation = Matrix.Identity;
		public Matrix Rotation2 = Matrix.Identity;
		public Matrix Scale = Matrix.Identity;
		public Vector3 Position = Vector3.Zero;
		public Vector3 Direction = Vector3.Backward;
		public Vector3 DirectionSpeed = Vector3.Backward;
		public OrientedBoundingBox BoundingBox;
		public Gizmos Gizmos;
		public bool ShowGizmos;
		public ContentManager Content;
		public GraphicsDevice GraphicsDevice;
		public SpriteBatch SpriteBatch;
		public SoundEffect EngineSound;
		public SoundEffectInstance SoundEffectInstance;

		public ModelBone FrontRightWheelBone;
		public ModelBone FrontLeftWheelBone;
		public ModelBone BackLeftWheelBone;
		public ModelBone BackRightWheelBone;
		public ModelBone CarBone;
		public Matrix FrontRightWheelTransform;
		public Matrix FrontLeftWheelTransform;
		public Matrix BackLeftWheelTransform;
		public Matrix BackRightWheelTransform;
		public Matrix CarTransform;
		public Matrix[] BoneTransforms;
		public Texture2D BaseTexture;
		public Texture2D NormalTexture;
		public Texture2D RoughnessTexture;
		public Texture2D MetallicTexture;
		public Texture2D AoTexture;
		#endregion Properties

		public BaseCar() { }

		public void Update(GameTime gameTime, BaseCollider[] colliders, PowerUp[] powerups)
		{
			SetKeyboardState(gameTime);
			var previousPosition = Position;
			var elapsedTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
			if (Position.Y == 0f)
			{
				// para tener control sobre el auto hice que deba estar sobre el suelo, ninguna razon en particular, me gusto asi
				Drive(elapsedTime);
				Turn(elapsedTime);
				Shoot();
				if (_isJumping) Jump();
			}
			else
			{
				_currentSpeed /= 1 + elapsedTime; // para que vaya desacelerando gradualemente
				DirectionSpeed -= Vector3.Up * Gravity;
			}

			if (!_enginestarted)
			{
				_enginestarted = true;
				SoundEffectInstance.Play();
			}
			SoundEffectInstance.Pitch = MathF.Min(_currentSpeed <= 1 ? 0f : RPM, 1f);
			UpdateMissiles(elapsedTime, colliders);

			// combino las velocidades horizontal y vertical
			DirectionSpeed = Direction * _currentSpeed + Vector3.Up * DirectionSpeed.Y;
			Position += DirectionSpeed * elapsedTime;

			if (Position.Y < 0f)
			{
				// si quedara por debajo del suelo lo seteo en 0
				Position.Y = 0f;
				DirectionSpeed.Y = 0f;
			}

			if (Position != previousPosition)
				Position -= CheckForCollisions(Position - previousPosition, colliders, powerups);

			World = Scale * Rotation * Matrix.CreateTranslation(Position);
		}

		public void Draw(Matrix view, Matrix projection, Vector3 cameraPosition, CameraType cameraType)
		{
			// Set the world matrix as the root transform of the model.
			Model.Root.Transform = World;

			// Calculate matrices based on the current animation position.
			var wheelRotationX = Matrix.CreateRotationX(_currentWheelRotation);
			var steeringRotationY = Matrix.CreateRotationY(_currentSteeringWheelRotation);
			var bodyRotation = Matrix.CreateRotationX(_currentBodyRotationX) * Matrix.CreateRotationZ(_currentBodyRotationZ);

			// Apply matrices to the relevant bones.
			FrontLeftWheelBone.Transform = wheelRotationX * steeringRotationY * FrontLeftWheelTransform;
			FrontRightWheelBone.Transform = wheelRotationX * steeringRotationY * FrontRightWheelTransform;
			BackLeftWheelBone.Transform = wheelRotationX * BackLeftWheelTransform;
			BackRightWheelBone.Transform = wheelRotationX * BackRightWheelTransform;
			CarBone.Transform = bodyRotation * CarTransform;

			// Look up combined bone matrices for the entire model.
			Model.CopyAbsoluteBoneTransformsTo(BoneTransforms);
			// For each mesh in the model,
			foreach (var mesh in Model.Meshes)
			{
				// Obtain the world matrix for that mesh (relative to the parent)
				var meshWorld = BoneTransforms[mesh.ParentBone.Index];
				Effect.Parameters["World"].SetValue(meshWorld);
				Effect.Parameters["WorldViewProjection"].SetValue(meshWorld * view * projection);
				Effect.Parameters["NormalWorldMatrix"].SetValue(Matrix.Invert(Matrix.Transpose(meshWorld)));
				mesh.Draw();
				if (ShowGizmos) Gizmos.Draw();
			}
			if (ShowGizmos)
			{
				Gizmos.UpdateViewProjection(view, projection);
				Gizmos.DrawCube(Matrix.CreateScale(BoundingBox.Extents * 2f) * BoundingBox.Orientation * Matrix.CreateTranslation(BoundingBox.Center), Color.Red);
			}

			DrawMissiles(view, projection, cameraPosition, cameraType);
		}

		#region Movement
		public void Drive(float elapsedTime)
		{
			if (_currentSpeed < 0f)
			{
				Reverse(elapsedTime);
				return;
			}

			if (_isAccelerating)
			{
				if (_isUsingBoost && (_godMode || _remainingBoost > 0f))
				{
					_currentSpeed += Acceleration[_currentGear] * DefaultBoostSpeed;
					_remainingBoost = Math.Max(_remainingBoost - elapsedTime, 0f);
				}
				else
					_currentSpeed += Acceleration[_currentGear];

				if (_currentSpeed > MaxSpeed[_currentGear] && _currentGear < MaxSpeed.Length - 1) _currentGear++;
				_currentSpeed = _currentSpeed > MaxSpeed[_currentGear] ? MaxSpeed[_currentGear] : _currentSpeed;
			}

			_currentBodyRotationX = _currentSpeed > 0f ? ToRadians(-RPM * 2.5f) : _currentBodyRotationX;

			if (_isBraking)
			{
				_currentSpeed -= DefaultBrakingForce;
				if (_currentGear > 1 && _currentSpeed < MaxSpeed[_currentGear - 1]) _currentGear--;
				_currentBodyRotationX = _currentSpeed > 0f ? _currentBodyRotationX - elapsedTime : _currentBodyRotationX;
			}

			if (!_isAccelerating && !_isBraking)
			{
				_currentSpeed /= 1 + elapsedTime / 5f; // para que vaya desacelerando gradualemente
				if (_currentGear > 1 && _currentSpeed < MaxSpeed[_currentGear - 1]) _currentGear--;
			}

			_currentWheelRotation += ToRadians(_currentSpeed / 10f);
		}

		public void Reverse(float elapsedTime)
		{
			if (_isAccelerating)
			{
				_currentGear = 0;
				_currentSpeed -= Acceleration[_currentGear];
				_currentSpeed = _currentSpeed < -MaxSpeed[_currentGear] ? -MaxSpeed[_currentGear] : _currentSpeed;
			}

			if (_isBraking)
			{
				_currentSpeed += DefaultBrakingForce;
				if (_currentSpeed > MaxSpeed[1]) _currentGear++;
			}

			if (!_isAccelerating && !_isBraking)
			{
				_currentSpeed /= 1 + elapsedTime / 5f; // para que vaya desacelerando gradualemente
				if (_currentSpeed > MaxSpeed[1]) _currentGear++;
			}

			_currentWheelRotation += ToRadians(_currentSpeed / 10f);
		}

		public void Turn(float elapsedTime)
		{
			if (_isTurningLeft && !_isTurningRight)
			{
				if (_currentGear != 1)
				{
					Rotation *= Matrix.CreateRotationY(DefaultSteeringSpeed * RPM);
					Direction = Vector3.Transform(Vector3.Backward, Rotation);
				}
				_currentSteeringWheelRotation = ToRadians(DefaultSteeringRotation);
				_currentBodyRotationZ = MathF.Min(_currentBodyRotationZ + elapsedTime, _currentSteeringWheelRotation / 7.5f);
			}
			else if (_isTurningRight && !_isTurningLeft)
			{
				if (_currentGear != 1)
				{
					Rotation *= Matrix.CreateRotationY(-DefaultSteeringSpeed * RPM);
					Direction = Vector3.Transform(Vector3.Backward, Rotation);
				}
				_currentSteeringWheelRotation = ToRadians(-DefaultSteeringRotation);
				_currentBodyRotationZ = MathF.Max(_currentBodyRotationZ - elapsedTime, _currentSteeringWheelRotation / 7.5f);
			}
			else
			{
				_currentSteeringWheelRotation = 0f;
				_currentBodyRotationZ = MathF.Max(_currentBodyRotationZ - elapsedTime, 0f);
			}
		}

		public void Jump()
		{
			DirectionSpeed += Vector3.Up * DefaultJumpSpeed;
		}

		private Vector3 CheckForCollisions(Vector3 positionDelta, BaseCollider[] colliders, PowerUp[] powerups)
		{
			BoundingBox.Center += positionDelta;
			BoundingBox.Orientation = Rotation;
			// Check intersection for every active powerup
			for (var index = 0; index < powerups.Length; index++)
			{
				if (powerups[index].IsActive && BoundingBox.Intersects(powerups[index].Collider))
				{
					powerups[index].Hide();
					switch (powerups[index].Type)
					{
						case PowerUpType.Boost:
							_remainingBoost = Math.Min(_remainingBoost + 3f, MaxBoost);
							break;
						case PowerUpType.Missiles:
							_remainingMissiles = 3;
							break;
						case PowerUpType.Shield:
							_hasShield = true;
							break;
						case PowerUpType.Wrench:
							_remainingHealth = 100f;
							break;
					}
					return Vector3.Zero;
				}
				continue;
			}
			// Check intersection for every collider
			for (var index = 0; !_godMode && index < colliders.Length; index++)
			{
				if (BoundingBox.Intersects(colliders[index]))
				{
					if (_hasShield) 
						_hasShield = false;
					else
						_remainingHealth = MathF.Max(_remainingHealth - (Math.Abs(_currentSpeed) / 200f), 0f);

					BoundingBox.Center -= positionDelta * 1.5f;
					_currentSpeed = -_currentSpeed * 0.3f;
					_currentGear = 1;
					return positionDelta * 1.5f;
				}
				continue;
			}
			return Vector3.Zero;
		}
		#endregion Movement

		#region Missiles
		public void Shoot()
		{
			if (_isShooting && _shootCooldown == 0.5f && (_godMode || _remainingMissiles > 0))
			{
				_shootCooldown = 0f;
				if(!_godMode) _remainingMissiles--;
				MissilesList.Add(new Missile(Position, Direction, Rotation, GraphicsDevice, Content, SpriteBatch));
			}
		}

		private void UpdateMissiles(float elapsedTime, BaseCollider[] colliders)
		{
			_shootCooldown = MathF.Min(_shootCooldown + elapsedTime, 0.5f);

			foreach (Missile missile in MissilesList)
				missile.Update(elapsedTime, colliders);

			MissilesList.RemoveAll(missile => !missile.IsActive);
		}

		private void DrawMissiles(Matrix view, Matrix projection, Vector3 cameraPosition, CameraType cameraType)
		{
			foreach (Missile m in MissilesList)
			{
				m.Draw(view, projection, cameraPosition, cameraType);
			}
		}
		#endregion Missiles

		#region utils

		public void SetKeyboardState(GameTime gameTime)
		{
			KeyboardState keyboardState = Keyboard.GetState();
			bool goingForward = _currentSpeed >= 0;
			_isAccelerating = goingForward ? keyboardState.IsKeyDown(Keys.W) : keyboardState.IsKeyDown(Keys.S);
			_isBraking = goingForward ? keyboardState.IsKeyDown(Keys.S) : keyboardState.IsKeyDown(Keys.W);
			_isTurningLeft = keyboardState.IsKeyDown(Keys.A);
			_isTurningRight = keyboardState.IsKeyDown(Keys.D);
			_isUsingBoost = keyboardState.IsKeyDown(Keys.LeftShift);
			_isShooting = keyboardState.IsKeyDown(Keys.LeftControl);
			_isJumping = keyboardState.IsKeyDown(Keys.Space);
			if (IsAbleToChangeGodMode(gameTime) && Keyboard.GetState().IsKeyDown(Keys.P)) EnableGodMode();
			if (IsAbleToChangeGizmosVisibility(gameTime) && Keyboard.GetState().IsKeyDown(Keys.G)) ChangeGizmosVisibility();
		}

		public float ToRadians(float angle)
		{
			return angle * (MathF.PI / 180f);
		}

		public void EnableGodMode()
		{
			_godMode = !_godMode;
			_godModeCooldown = 0f;
		}
		public bool IsGizmosDisplayed()
		{
			return ShowGizmos;
		}

		public void PauseResumeEngineSound(bool play)
		{
			if (play) SoundEffectInstance.Resume();
			else SoundEffectInstance.Pause();
		}

		public bool IsAbleToChangeGodMode(GameTime gameTime)
		{
			_godModeCooldown = MathF.Min(_godModeCooldown + Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds), 0.5f);
			return _godModeCooldown == 0.5f;
		}
		public void ChangeGizmosVisibility()
		{
			ShowGizmos = !ShowGizmos;
			_gizmosCooldown = 0f;
		}

		public bool IsAbleToChangeGizmosVisibility(GameTime gameTime)
		{
			_gizmosCooldown = MathF.Min(_gizmosCooldown + Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds), 0.5f);
			return _gizmosCooldown == 0.5f;
		}

		private string GetGear()
		{
			switch (_currentGear)
			{
				case 0: return "R";
				case 1: return "N";
				default: return MathF.Abs(_currentSpeed) / 20 < 1f ? "N": (_currentGear - 1).ToString();
			}
		}
		#endregion
	}
}

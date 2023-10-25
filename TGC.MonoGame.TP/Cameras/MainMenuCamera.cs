using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TGC.MonoGame.TP.Cameras
{
    /// <summary>
    /// Una camara que sigue objetos
    /// </summary>
    class MainMenuCamera
	{
        public Matrix Projection { get; private set; }

        public Matrix View { get; private set; }

        private float FrontVectorInterpolator { get; set; } = 0f;
		public Vector3 CurrentCameraPosition { get; set; } = Vector3.Forward;
		public Vector3 CurrentCameraLocation;
		public Vector3 PreviousCameraPosition;
		private float CameraChangeCooldown = 0f;
		private Viewport Viewport;
		public int CameraType = 0;

		public MainMenuCamera(Viewport viewport)
        {
			Viewport = viewport;
			Projection = Matrix.CreatePerspectiveFieldOfView(15 * (MathF.PI / 180f), viewport.AspectRatio, 0.1f, 5000f);
		}

		public void Update(GameTime gameTime)
		{
			CameraChangeCooldown = MathF.Min(CameraChangeCooldown + Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds) / 3f, 1f);
			if (CameraChangeCooldown == 1f)
			{
				CameraType++;
				CameraChangeCooldown = 0f;
			}

			var elapsedTime = Convert.ToSingle(gameTime.ElapsedGameTime.TotalSeconds);
			Vector3 cameraPosition = Vector3.One; // la posicion donde va a estar parada la camara
			Vector3 targetPosition = Vector3.Zero; // la posicion donde apunta la camara
			Vector3 vector = Vector3.Up;

			switch (CameraType)
			{
				case 0:
					cameraPosition = new Vector3(-500f, 50f, 1300f * CameraChangeCooldown);
					targetPosition = new Vector3(0f, 30f, 0f);
					break;
				case 1:
					cameraPosition = new Vector3(1000f, 50f + 50f * CameraChangeCooldown, 500f);
					targetPosition = new Vector3(100f, 30f * CameraChangeCooldown, 50f);
					break;
				case 2:
					cameraPosition = new Vector3(-100f * (CameraChangeCooldown - 0.5f), 1000f, -100f * (CameraChangeCooldown - 0.5f));
					targetPosition = new Vector3(-100f * (CameraChangeCooldown - 0.5f), 0f, -100f * (CameraChangeCooldown - 0.5f));
					vector = Vector3.One;
					break;
				default:
					CameraType = 0;
					cameraPosition = new Vector3(-500f, 50f, 1300f * CameraChangeCooldown);
					targetPosition = new Vector3(0f, 30f, 0f);
					break;
			};

			var offsetedPosition = targetPosition + cameraPosition;
            var backPosition = cameraPosition;
			backPosition.Normalize();
			
            var upPosition = Vector3.Cross(backPosition, vector);
            var cameraCorrectUp = Vector3.Cross(upPosition, backPosition);

			View = Matrix.CreateLookAt(offsetedPosition, targetPosition, cameraCorrectUp);

			PreviousCameraPosition = cameraPosition;
			CurrentCameraLocation = offsetedPosition;
		}

        public void ChangeCamera()
        {
            CameraType++;
			if (CameraType > 2) CameraType = 0;
		}
	}
}

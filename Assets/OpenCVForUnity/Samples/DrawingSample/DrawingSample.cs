using UnityEngine;
using System.Collections;

using OpenCVForUnity;

namespace OpenCVForUnitySample
{
		/// <summary>
		/// Drawing sample.
		/// </summary>
		public class DrawingSample : MonoBehaviour
		{

				// Use this for initialization
		public Texture2D textur;
				
		void Start ()
				{
			
			//Texture2D imgTexture = textur.getp as Texture2D;
			Texture2D imgTexture = new Texture2D (textur.width, textur.height);
			imgTexture.SetPixels (textur.GetPixels ());

			
						Mat imgMat = new Mat (imgTexture.height, imgTexture.width, CvType.CV_8UC3);
			
						Utils.texture2DToMat (imgTexture, imgMat);
						Debug.Log ("imgMat dst ToString " + imgMat.ToString ());
						


						Core.line (imgMat, new OpenCVForUnity.Point (50, 50), new OpenCVForUnity.Point (400, 105), new Scalar (0, 0, 200), 3);  

						Core.rectangle (imgMat, new OpenCVForUnity.Point (150, 200), new OpenCVForUnity.Point (300, 300), new Scalar (0, 200, 0), 5);

						Core.circle (imgMat, new OpenCVForUnity.Point (500, 300), 80, new Scalar (200, 0, 0), 1);

						Core.arrowedLine (imgMat, new OpenCVForUnity.Point (100, 500), new OpenCVForUnity.Point (550, 350), new Scalar (255, 255, 0), 4, Core.LINE_8, 0, 0.1);


						double angle = 100;
						Core.ellipse (imgMat, new OpenCVForUnity.Point (200, 400), new Size (80, 150), angle, angle - 200, angle + 100, new Scalar (255, 255, 255), -1);


						int[] face = {Core.FONT_HERSHEY_SIMPLEX, Core.FONT_HERSHEY_PLAIN, Core.FONT_HERSHEY_DUPLEX, Core.FONT_HERSHEY_COMPLEX, 
			Core.FONT_HERSHEY_TRIPLEX, Core.FONT_HERSHEY_COMPLEX_SMALL, Core.FONT_HERSHEY_SCRIPT_SIMPLEX, 
			Core.FONT_HERSHEY_SCRIPT_COMPLEX, Core.FONT_ITALIC};
		

						Core.putText (imgMat, "OpenCV", new OpenCVForUnity.Point (50, 50), face [0], 1.2, new Scalar (0, 0, 200), 2, Core.LINE_AA, false);
						Core.putText (imgMat, "OpenCV", new OpenCVForUnity.Point (50, 100), face [1], 1.2, new Scalar (0, 200, 0), 2, Core.LINE_AA, false);
						Core.putText (imgMat, "OpenCV", new OpenCVForUnity.Point (50, 150), face [2], 1.2, new Scalar (200, 0, 0), 2, Core.LINE_AA, false);
						Core.putText (imgMat, "OpenCV", new OpenCVForUnity.Point (50, 200), face [3], 1.2, new Scalar (0, 100, 100), 2, Core.LINE_AA, false);
						Core.putText (imgMat, "OpenCV", new OpenCVForUnity.Point (50, 250), face [4], 1.2, new Scalar (100, 100, 0), 2, Core.LINE_AA, false);
						Core.putText (imgMat, "OpenCV", new OpenCVForUnity.Point (50, 300), face [5], 1.2, new Scalar (100, 0, 100), 2, Core.LINE_AA, false);
						Core.putText (imgMat, "OpenCV", new OpenCVForUnity.Point (50, 350), face [6], 1.2, new Scalar (100, 100, 100), 2, Core.LINE_AA, false);
						Core.putText (imgMat, "OpenCV", new OpenCVForUnity.Point (50, 400), face [7], 1.2, new Scalar (100, 100, 200), 2, Core.LINE_AA, false);
						Core.putText (imgMat, "OpenCV", new OpenCVForUnity.Point (300, 50), face [0] | face [8], 1.2, new Scalar (100, 200, 100), 2, Core.LINE_AA, false);
						Core.putText (imgMat, "OpenCV", new OpenCVForUnity.Point (300, 100), face [1] | face [8], 1.2, new Scalar (200, 100, 100), 2, Core.LINE_AA, false);
						Core.putText (imgMat, "OpenCV", new OpenCVForUnity.Point (300, 150), face [2] | face [8], 1.2, new Scalar (200, 200, 100), 2, Core.LINE_AA, false);
						Core.putText (imgMat, "OpenCV", new OpenCVForUnity.Point (300, 200), face [3] | face [8], 1.2, new Scalar (200, 100, 200), 2, Core.LINE_AA, false);
						Core.putText (imgMat, "OpenCV", new OpenCVForUnity.Point (300, 250), face [4] | face [8], 1.2, new Scalar (100, 200, 200), 2, Core.LINE_AA, false);
						Core.putText (imgMat, "OpenCV", new OpenCVForUnity.Point (300, 300), face [5] | face [8], 1.2, new Scalar (100, 200, 255), 2, Core.LINE_AA, false);
						Core.putText (imgMat, "OpenCV", new OpenCVForUnity.Point (300, 350), face [6] | face [8], 1.2, new Scalar (100, 255, 200), 2, Core.LINE_AA, false);
						Core.putText (imgMat, "OpenCV", new OpenCVForUnity.Point (300, 400), face [7] | face [8], 1.2, new Scalar (255, 200, 100), 2, Core.LINE_AA, false);


		
		
						Texture2D texture = new Texture2D (imgMat.cols (), imgMat.rows (), TextureFormat.RGBA32, false);
						Utils.matToTexture2D (imgMat, texture);
		
						gameObject.GetComponent<Renderer> ().material.mainTexture = texture;
		
		
				}
	
				// Update is called once per frame
				void Update ()
				{
		
				}
	
				public void OnBackButton ()
				{
						Application.LoadLevel ("OpenCVForUnitySample");
				}
		}
}

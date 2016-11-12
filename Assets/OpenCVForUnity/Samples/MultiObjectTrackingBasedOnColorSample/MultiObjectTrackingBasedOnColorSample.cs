using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using OpenCVForUnity;

namespace OpenCVForUnitySample
{
    public class MultiObjectTrackingBasedOnColorSample : MonoBehaviour
    {
        const int MAX_NUM_OBJECTS = 2;
        const int MIN_OBJECT_AREA = 100 * 100;

        public bool isRed, isGreen, isBlue, isYellow;
        public bool shouldUseFrontFacing = false;       
        
		public Text txt;
        public WebCamTexture webCamTexture;
        private WebCamDevice webCamDevice;
        private Color32[] colors;
        private int width = 640;
        private int height = 480;
        private Mat rgbMat;
        private Texture2D texture;
        private bool initDone = false;
        private ScreenOrientation screenOrientation = ScreenOrientation.Unknown;
        private int MAX_OBJECT_AREA;
        private Mat thresholdMat;
        private Mat hsvMat;

		[Range(0, 256)]
		public int min1, min2, min3, max1, max2, max3;
		public bool debugColor = false;

        public Dictionary<String, ColorObject> MaxItems { get; set; }

        void Start()
        {
            MaxItems = new Dictionary<String, ColorObject>();
            MaxItems.Add("red", new ColorObject());
            MaxItems.Add("green", new ColorObject());
            MaxItems.Add("yellow", new ColorObject());
            MaxItems.Add("blue", new ColorObject());
            StartCoroutine(Init());
        }

        private IEnumerator Init()
        {
            if (webCamTexture != null)
            {
                webCamTexture.Stop();
                initDone = false;
                rgbMat.Dispose();
                thresholdMat.Dispose();
                hsvMat.Dispose();
            }

            // Checks how many and which cameras are available on the device
            for (int cameraIndex = 0; cameraIndex < WebCamTexture.devices.Length; cameraIndex++)
            {
                if (WebCamTexture.devices[cameraIndex].isFrontFacing == shouldUseFrontFacing)
                {
                    Debug.Log(cameraIndex + " name " + WebCamTexture.devices[cameraIndex].name + " isFrontFacing " + WebCamTexture.devices[cameraIndex].isFrontFacing);
                    webCamDevice = WebCamTexture.devices[cameraIndex];
                    webCamTexture = new WebCamTexture(webCamDevice.name, width, height);
                    break;
                }
            }

            if (webCamTexture == null)
            {
                webCamDevice = WebCamTexture.devices[0];
                webCamTexture = new WebCamTexture(webCamDevice.name, width, height);
            }

            Debug.Log("width " + webCamTexture.width + " height " + webCamTexture.height + " fps " + webCamTexture.requestedFPS);


            webCamTexture.Play();


            while (true)
            {

                //If you want to use webcamTexture.width and webcamTexture.height on iOS, you have to wait until webcamTexture.didUpdateThisFrame == 1, otherwise these two values will be equal to 16. (http://forum.unity3d.com/threads/webcamtexture-and-error-0x0502.123922/)
#if UNITY_IOS && !UNITY_EDITOR
			                if (webCamTexture.width > 16 && webCamTexture.height > 16) {
#else
                if (webCamTexture.didUpdateThisFrame)
                {
#if UNITY_IOS && !UNITY_EDITOR 
									while (webCamTexture.width <= 16) {
											webCamTexture.GetPixels32 ();
											yield return new WaitForEndOfFrame ();
									} 
#endif
#endif

                    Debug.Log("width " + webCamTexture.width + " height " + webCamTexture.height + " fps " + webCamTexture.requestedFPS);
                    Debug.Log("videoRotationAngle " + webCamTexture.videoRotationAngle + " videoVerticallyMirrored " + webCamTexture.videoVerticallyMirrored + " isFrongFacing " + webCamDevice.isFrontFacing);

                    colors = new Color32[webCamTexture.width * webCamTexture.height];
                    rgbMat = new Mat(webCamTexture.height, webCamTexture.width, CvType.CV_8UC3);
                    texture = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGBA32, false);


                    thresholdMat = new Mat();
                    hsvMat = new Mat();

                    MAX_OBJECT_AREA = (int)(webCamTexture.height * webCamTexture.width / 1.5);

                    gameObject.GetComponent<Renderer>().material.mainTexture = texture;
                    //cube.GetComponent<Renderer> ().material.mainTexture = texture;
                    Debug.LogWarning(gameObject.GetComponent<Renderer>().material != null);
                    Debug.LogWarning(texture.height + " and " + texture.width);

                    UpdateLayout();

                    screenOrientation = Screen.orientation;
                    initDone = true;
                    Debug.LogWarning("Init done");
                    break;
                }
                else
                {
                    yield return 0;
                }
            }
        }

        private void UpdateLayout()
        {
            gameObject.transform.localRotation = new Quaternion(0, 0, 0, 0);
            gameObject.transform.localScale = new Vector3(webCamTexture.width, webCamTexture.height, 1);

            if (webCamTexture.videoRotationAngle == 90 || webCamTexture.videoRotationAngle == 270)
            {
                gameObject.transform.eulerAngles = new Vector3(0, 0, -90);
            }
            float width = 0;
            float height = 0;
            if (webCamTexture.videoRotationAngle == 90 || webCamTexture.videoRotationAngle == 270)
            {
                width = gameObject.transform.localScale.y;
                height = gameObject.transform.localScale.x;
            }
            else if (webCamTexture.videoRotationAngle == 0 || webCamTexture.videoRotationAngle == 180)
            {
                width = gameObject.transform.localScale.x;
                height = gameObject.transform.localScale.y;
            }
            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale)
            {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
            }
            else
            {
                Camera.main.orthographicSize = height / 2;
            }
        }

        void Update()
        {
		//	if (Input.GetKeyDown(KeyCode.Space) || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
  		// 	initDone = !initDone;
			if (Input.GetKeyDown (KeyCode.P))
				isPause = !isPause;
			if (!initDone)
                return;


            if (screenOrientation != Screen.orientation)
            {
                screenOrientation = Screen.orientation;
                UpdateLayout();
            }


#if UNITY_IOS && !UNITY_EDITOR
				        if (webCamTexture.width > 16 && webCamTexture.height > 16) {
#else
            if (webCamTexture.didUpdateThisFrame)
            {
#endif

                Utils.webCamTextureToMat(webCamTexture, rgbMat, colors);

                if (webCamDevice.isFrontFacing)
                {
                    if (webCamTexture.videoRotationAngle == 0)
                    {
                        Core.flip(rgbMat, rgbMat, 1);
                    }
                    else if (webCamTexture.videoRotationAngle == 90)
                    {
                        Core.flip(rgbMat, rgbMat, 0);
                    }
                    if (webCamTexture.videoRotationAngle == 180)
                    {
                        Core.flip(rgbMat, rgbMat, 0);
                    }
                    else if (webCamTexture.videoRotationAngle == 270)
                    {
                        Core.flip(rgbMat, rgbMat, 1);
                    }
                }
                else
                {
                    if (webCamTexture.videoRotationAngle == 180)
                    {
                        Core.flip(rgbMat, rgbMat, -1);
                    }
                    else if (webCamTexture.videoRotationAngle == 270)
                    {
                        Core.flip(rgbMat, rgbMat, -1);
                    }
                }


				//if (Input.GetKey(KeyCode.Space) || Input.touchCount > 0)
				{
					
                if (isYellow)
                {
					ColorObject yellow = new ColorObject("yellow", min1, min2, min3, max1, max2, max3, debugColor);

                    Imgproc.cvtColor(rgbMat, hsvMat, Imgproc.COLOR_RGB2HSV);
                    Core.inRange(hsvMat, yellow.HSVmin, yellow.HSVmax, thresholdMat);
                    MorphOps(thresholdMat);
                    TrackFilteredObject(yellow, thresholdMat, hsvMat, rgbMat);
                }

                if (isRed)
                {
					ColorObject red = new ColorObject("red", min1, min2, min3, max1, max2, max3, debugColor);

                    Imgproc.cvtColor(rgbMat, hsvMat, Imgproc.COLOR_RGB2HSV);
                    Core.inRange(hsvMat, red.HSVmin, red.HSVmax, thresholdMat);
                    MorphOps(thresholdMat);
                    TrackFilteredObject(red, thresholdMat, hsvMat, rgbMat);
                }

                if (isGreen)
                {

					ColorObject green = new ColorObject("green", min1, min2, min3, max1, max2, max3, debugColor);

                    Imgproc.cvtColor(rgbMat, hsvMat, Imgproc.COLOR_RGB2HSV);
                    Core.inRange(hsvMat, green.HSVmin, green.HSVmax, thresholdMat);
                    MorphOps(thresholdMat);
                    TrackFilteredObject(green, thresholdMat, hsvMat, rgbMat);
                }

                if (isBlue)
                {
					ColorObject blue = new ColorObject("blue", min1, min2, min3, max1, max2, max3, debugColor);
                    Imgproc.cvtColor(rgbMat, hsvMat, Imgproc.COLOR_RGB2HSV);
                    Core.inRange(hsvMat, blue.HSVmin, blue.HSVmax, thresholdMat);
                    MorphOps(thresholdMat);
                    TrackFilteredObject(blue, thresholdMat, hsvMat, rgbMat);
                }
				}
				Utils.matToTexture2D(rgbMat, texture, colors);
            }
        }

        void OnDisable()
        {
            webCamTexture.Stop();
        }

        public void OnBackButton()
        {
			SceneManager.LoadScene ("Menu");
        }

        public void OnChangeCameraButton()
        {
            shouldUseFrontFacing = !shouldUseFrontFacing;
            StartCoroutine(Init());
        }
		public float MIN_MAX_DISTANCE = 100;
		public bool isPause = false;

        void DrawObject(List<ColorObject> theColorObjects, Mat frame, Mat temp, List<MatOfPoint> contours, Mat hierarchy)
        {

			for (int i = 0; i < contours.Count; i++) 
			{
				float tmpDist = 0;
				var conts = contours [i].toList ();
				for (int j = 0; j < conts.Count; j++) {
					for (int k = 0; k < conts.Count; k++) {
						float currentDist = Vector2.Distance (new Vector2 ((float)conts [j].x, (float)conts [j].y), new Vector2 ((float)conts [k].x, (float)conts [k].y));
						if (currentDist > tmpDist)
							tmpDist = currentDist;
					}
				}
				if (tmpDist > MIN_MAX_DISTANCE) 
				{
					isPause = true;
					txt.text = "ВАУ! ВЫ ПОЙМАЛИ ЖЕЛТУЮ ХУ*ТУ!!!";
				}
			}
            txt.text = "" + theColorObjects.Count + " con: " + contours.Count;
            for (int i = 0; i < theColorObjects.Count; i++)
            {
                Imgproc.drawContours(frame, contours, i, theColorObjects[i].Color, 4, 8, hierarchy, int.MaxValue, new OpenCVForUnity.Point());
				//Imgproc.drawContours(frame, contours, i, theColorObjects[i].Color, 3);
				Core.circle(frame, new OpenCVForUnity.Point(theColorObjects[i].XPos, theColorObjects[i].YPos), (int)(Math.Sqrt(theColorObjects[i].Area)/2), theColorObjects[i].Color, 4);
				//Core.putText(frame, theColorObjects[i].XPos + " , " + theColorObjects[i].YPos, new OpenCVForUnity.Point(theColorObjects[i].XPos, theColorObjects[i].YPos + 20), 1, 1, theColorObjects[i].Color, 2);
				Core.putText(frame, theColorObjects[i].ColorName + ": " + (int)(Math.Sqrt(theColorObjects[i].Area)), new OpenCVForUnity.Point(theColorObjects[i].XPos, theColorObjects[i].YPos - 40), 1, 2, theColorObjects[i].Color, 2);
            }
        }

        void MorphOps(Mat thresh)
        {

            //create structuring element that will be used to "dilate" and "erode" image.
            //the element chosen here is a 3px by 3px rectangle
            Mat erodeElement = Imgproc.getStructuringElement(Imgproc.MORPH_RECT, new Size(3, 3));
            //dilate with larger element so make sure object is nicely visible
            Mat dilateElement = Imgproc.getStructuringElement(Imgproc.MORPH_RECT, new Size(8, 8));

            Imgproc.erode(thresh, thresh, erodeElement);
            Imgproc.erode(thresh, thresh, erodeElement);

            Imgproc.dilate(thresh, thresh, dilateElement);
            Imgproc.dilate(thresh, thresh, dilateElement);
        }


        void TrackFilteredObject(ColorObject theColorObject, Mat threshold, Mat HSV, Mat cameraFeed)
        {
            MaxItems[theColorObject.ColorName] = new ColorObject();

            List<ColorObject> colorObjects = new List<ColorObject>();
            Mat temp = new Mat();
            threshold.copyTo(temp);
            List<MatOfPoint> contours = new List<MatOfPoint>();
            Mat hierarchy = new Mat();
            
            Imgproc.findContours(temp, contours, hierarchy, Imgproc.RETR_CCOMP, Imgproc.CHAIN_APPROX_SIMPLE);

            bool colorObjectFound = false;
            if (hierarchy.rows() > 0)
            {
                int numObjects = hierarchy.rows();

                //if number of objects greater than MAX_NUM_OBJECTS we have a noisy filter
                if (numObjects < MAX_NUM_OBJECTS)
                {
                    for (int i = 0; i >= 0; i = (int)hierarchy.get(0, i)[0])
                    {

                        Moments moment = Imgproc.moments(contours[i]);
                        float area = (float)moment.get_m00();

                        //if the area is less than MIN_OBJECT_AREA then it is probably just noise
                        if (area > MIN_OBJECT_AREA)
                        {

                            ColorObject colorObject = new ColorObject();
                            colorObject.Area = area;
                            colorObject.XPos = (int)(moment.get_m10() / area);
                            colorObject.YPos = (int)(moment.get_m01() / area);
                            colorObject.ColorName = theColorObject.ColorName;
                            colorObject.Color = theColorObject.Color;

                            colorObjects.Add(colorObject);
                            colorObjectFound = true;

                            if(area > MaxItems[theColorObject.ColorName].Area)
                            {
                                MaxItems[theColorObject.ColorName] = colorObject;
                            }
                        }
                        else
                        {
                            colorObjectFound = false;
                        }
                    }
                    //let user know you found an object
                    //if (colorObjectFound == true)
                    {
                        //draw object location on screen
                        DrawObject(colorObjects, cameraFeed, temp, contours, hierarchy);
                    }

                }
                else
                {
                    Core.putText(cameraFeed, "TOO MUCH NOISE!", new OpenCVForUnity.Point(5, cameraFeed.rows() - 10), Core.FONT_HERSHEY_SIMPLEX, 1.0, new Scalar(255, 255, 255, 255), 2, Core.LINE_AA, false);
                }
            }
        }

    }
}
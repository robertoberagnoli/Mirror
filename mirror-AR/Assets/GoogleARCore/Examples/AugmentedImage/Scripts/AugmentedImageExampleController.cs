//-----------------------------------------------------------------------
// <copyright file="AugmentedImageExampleController.cs" company="Google">
//
// Copyright 2018 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleARCore.Examples.AugmentedImage
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.InteropServices;
    using GoogleARCore;
    
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using UnityEngine.UI;
    using UnityEngine.Video;

    /// <summary>
    /// Controller for AugmentedImage example.
    /// </summary>
    public class AugmentedImageExampleController : MonoBehaviour
    {
        /// <summary>
        /// A prefab for visualizing an AugmentedImage.
        /// </summary>
        public AugmentedImageVisualizer AugmentedImageVisualizerPrefab;

        private bool fitFlag=true;
        private float fitTime = 0f;

        /// <summary>
        /// The overlay containing the fit to scan user guide.
        /// </summary>
        public GameObject FitToScanOverlay;

        private Dictionary<int, AugmentedImageVisualizer> m_Visualizers
            = new Dictionary<int, AugmentedImageVisualizer>();

        private List<AugmentedImage> m_TempAugmentedImages = new List<AugmentedImage>();
        public VideoPlayer videoPlayer;

        /// <summary>
        /// 
        /// 
        public void Start()
        {
            var path = Application.persistentDataPath + "/videoMirror2.mp4";
            var path2 = Application.persistentDataPath + "/videoMirror.mp4";
            AugmentedImageVisualizerPrefab.gameObject.SetActive(false);
            if (File.Exists(path))
            {
                if (File.Exists(path2))
                {
                    File.Delete(path2);
                }
                File.Copy(path, path2);
            }
            Invoke("fileSearchRoutine", 0);
            Invoke("mirrorRoutine", 5);
            
        }
        /// The Unity Update method.
        /// </summary>
        ///





        public void fileSearchRoutine()
        {



            StartCoroutine(fileSearch());

            

        }

        IEnumerator fileSearch()
        {
            var path = Application.persistentDataPath + "/videoMirror.mp4";
            var path2 = Application.persistentDataPath + "/videoMirror2.mp4";

            if (File.Exists(path))
            {
                //_debugText.text += "\nFile has been found!\n\n";
                //videoPlayer.Stop();

                videoPlayer.url = path;
                videoPlayer.Prepare();
                videoPlayer.Play();

            }
            else
            {
                if (File.Exists(path2))
                {
                    //videoPlayer.Stop();
                    videoPlayer.url = path2;
                }
                
                videoPlayer.Play();
                Invoke("fileSearchRoutine", 10);

            }
            yield return true;

        }




        public void mirrorRoutine()
        {
            StartCoroutine(getMirror());
            Invoke("mirrorRoutine", 300);//ERA 60
        }

        IEnumerator getMirror()
        {
             var www = new WWW("https://beragnoli.s3.eu-central-1.amazonaws.com/videoMirror.mp4");
            
            //var www = new WWW("http://135.122.67.125/v1/media/last/download");


          
            Debug.Log("Downloading!");
            yield return www;
            if (www.isDone)
            {
                if (www.error == null)
                {
                    File.WriteAllBytes(Application.persistentDataPath + "/videoMirror2.mp4", www.bytes);
                    Debug.Log("File Saved!");
                    //videoPlayer.Stop();
                    Invoke("fileSearchRoutine", 5);

                }
                else
                {
                   // Invoke("mirrorRoutine", 60);//era 30
                }
            }
            else
            {
               // Invoke("mirrorRoutine", 30);
            }
            /*
            //WWW www = new WWW("https://continuous-profile.herokuapp.com/tweets/trump");
            WWW www = new WWW("https://continuous-profile.herokuapp.com/tweets/");
            yield return www;

            if (www.isDone)
            {
                if (www.error == null)
                {
                    //CON L'IMMAGINE

                    //video = www.GetMovieTexture();
                    //tweetMesh.changeText("");

                    //CON IL TESTO:
                    // string tweetsJson = www.text;
                    //Tweet tweet = JsonUtility.FromJson<List<Tweet>>(tweetsJson);

                    //tweetMesh.changeText(Image.DatabaseIndex.ToString()+ TweetJson.tweet[0].text);
                }
                else
                {
                   // tweetMesh.changeText("");
                    //Plane.GetComponent<Renderer>().material.mainTexture = texture;
                }
            }
            */
        }


        public void Update()
        {

            // Exit the app when the 'back' button is pressed.
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }

            // Check that motion tracking is tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                return;
            }

            // Get updated augmented images for this frame.
            Session.GetTrackables<AugmentedImage>(m_TempAugmentedImages, TrackableQueryFilter.Updated);

            // Create visualizers and anchors for updated augmented images that are tracking and do not previously
            // have a visualizer. Remove visualizers for stopped images.
            foreach (var image in m_TempAugmentedImages)
            {
                AugmentedImageVisualizer visualizer = null;
                m_Visualizers.TryGetValue(image.DatabaseIndex, out visualizer);
                if (image.TrackingState == TrackingState.Tracking && visualizer == null)
                {
                    AugmentedImageVisualizerPrefab.gameObject.SetActive(true);
                    // Create an anchor to ensure that ARCore keeps tracking this augmented image.
                    Anchor anchor = image.CreateAnchor(image.CenterPose);
                    visualizer = (AugmentedImageVisualizer)Instantiate(AugmentedImageVisualizerPrefab, anchor.transform);
                    visualizer.Image = image;
                    m_Visualizers.Add(image.DatabaseIndex, visualizer);
                    fitFlag = false;
                }
                else if (image.TrackingState == TrackingState.Stopped && visualizer != null)
                {
                    m_Visualizers.Remove(image.DatabaseIndex);
                    GameObject.Destroy(visualizer.gameObject);
                    fitFlag = false;
                }
                
               
            }

            // Show the fit-to-scan overlay if there are no images that are Tracking.
            foreach (var visualizer in m_Visualizers.Values)
            {
                if (visualizer.Image.TrackingState == TrackingState.Tracking)
                {
                    FitToScanOverlay.SetActive(false);
                    return;
                }
            }

            if (!fitFlag)
            {
                fitTime = Time.time;
                fitFlag = true;
            }


            if ((Time.time  >= 30) && fitTime!=0)
            {
                SceneManager.LoadScene("AugmentedImage");
            }
            else
            {
                FitToScanOverlay.SetActive(true);
            }

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Scenario.Editor
{
    public class Images : EditorWindow
    {
        private static readonly ImagesUI ImagesUI = new();
    
        internal static List<ImageDataStorage.ImageData> imageDataList = new();

        /// <summary>
        /// Contains a token that is useful to get the next page of inferences
        /// </summary>
        private static string lastPageToken = string.Empty;

        [MenuItem("Window/Scenario/Images")]
        public static void ShowWindow()
        {
            lastPageToken = string.Empty;
            imageDataList.Clear();
            GetInferencesData();
        
            var images = (Images)GetWindow(typeof(Images));
            ImagesUI.Init(images);
        }

        private void OnGUI()
        {
            ImagesUI.OnGUI(this.position);
        }
    
        private void OnDestroy()
        {
            ImagesUI.CloseSelectedTextureSection();
        }
    
        public static void GetInferencesData(Action callback_OnDataGet = null) //why get inferences instead of getting the assets ??
        {
            string request = $"inferences";
            if (!string.IsNullOrEmpty(lastPageToken))
                request = $"inferences?paginationToken={lastPageToken}";

            ApiClient.RestGet(request, response =>
            {
                var inferencesResponse = JsonConvert.DeserializeObject<InferencesResponse>(response.Content);

                lastPageToken = inferencesResponse.nextPaginationToken;

                if (inferencesResponse.inferences[0].status == "failed")
                {
                    Debug.LogError("Api Response: Status == failed, Try Again..");
                }

                List<ImageDataStorage.ImageData> imageDataDownloaded = new List<ImageDataStorage.ImageData>();
                
                foreach (var inference in inferencesResponse.inferences)
                {
                    foreach (var image in inference.images)
                    {
                        imageDataDownloaded.Add(new ImageDataStorage.ImageData
                        {
                            Id = image.id,
                            Url = image.url,
                            InferenceId = inference.id,
                            Prompt = inference.parameters.prompt,
                            Steps = inference.parameters.numInferenceSteps,
                            Size = new Vector2(inference.parameters.width,inference.parameters.height),
                            Guidance = inference.parameters.guidance,
                            Scheduler = "Default", //TODO : change this to reflect the scheduler used for creating this image
                            Seed = image.seed,
                            CreatedAt = inference.createdAt,
                        });
                    }
                }

                imageDataList.AddRange(imageDataDownloaded);
                foreach (ImageDataStorage.ImageData imageData in imageDataList)
                {
                    FetchTextureFor(imageData);
                }
            });
        }

        /// <summary>
        /// Fetch a texture for a specific ImageData
        /// </summary>
        private static void FetchTextureFor(ImageDataStorage.ImageData _image, Action callback_OnTextureGet = null)
        {
            CommonUtils.FetchTextureFromURL(_image.Url, texture =>
            {
                _image.texture = texture;
                callback_OnTextureGet?.Invoke();
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_id">The id of the image you want to delete</param>
        public void DeleteImage(string _id)
        {
            ImageDataStorage.ImageData imageData = GetImageDataById(_id);
            string modelId = EditorPrefs.GetString("SelectedModelId");

            string url = $"models/{modelId}/inferences/{imageData.InferenceId}/images/{imageData.Id}";
            ApiClient.RestDelete(url,null);
            imageDataList.Remove(imageData);
            Repaint();
        }

        public static ImageDataStorage.ImageData GetImageDataById(string _id)
        {
            return imageDataList.Find(x => x.Id == _id);
        }

        public static Texture2D GetTextureByImageId(string _id)
        {
            return imageDataList.Find(x => x.Id == _id).texture;
        }

    }
}

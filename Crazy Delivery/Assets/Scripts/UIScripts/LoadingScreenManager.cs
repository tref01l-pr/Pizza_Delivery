using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIScripts
{
    public class LoadingScreenManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject _loadingCanvas;
    
        [SerializeField]
        private bool _isLoaded = false;
        
        [SerializeField]
        private bool _persistBetweenScenes = true;
        
        private void SetLoadingCanvas(bool isLoading)
        {
            _isLoaded = !isLoading;
            try
            {
                _loadingCanvas.gameObject.SetActive(isLoading);
                
            }
            catch (Exception e)
            {
                Destroy(gameObject);
            }
        }
        
        void Awake()
        {
            if (_persistBetweenScenes)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        public void ShowLoadingScreen()
        {
            SetLoadingCanvas(true);
        }
    
        public void HideLoadingScreen()
        {
            SetLoadingCanvas(false);
        }
    }
}
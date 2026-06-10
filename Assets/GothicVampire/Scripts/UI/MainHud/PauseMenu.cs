using GothicVampire.Game;
using GothicVampire.Player.Inputs;
using GothicVampire.Technologies;
using Sylpheed.Core;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GothicVampire.UI.MainHud
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private CameraMovement _cameraMovement;

        private void Awake()
        {
            Hide();
        }

        public void Show()
        {
            _panel.SetActive(true);
        }

        public void Hide()
        {
            _panel.SetActive(false);
        }

        private void Update()
        {
            // Check for ESC key
            if (!Keyboard.current.escapeKey.wasPressedThisFrame) return;
            
            // Show/hide based on panel state
            if (_panel.activeSelf) Hide();
            else Show();
        }

        public void Evt_QuitPressed() => Application.Quit();

        public void Evt_UnlockAllPressed()
        {
            var technologyManager = ServiceLocator.Get<World>().Player.GetService<TechnologyManager>();
            technologyManager.UnlockAll();
        }

        public void Evt_CenterCameraPressed()
        {
            _cameraMovement.CenterToMap();
        }
    }
}
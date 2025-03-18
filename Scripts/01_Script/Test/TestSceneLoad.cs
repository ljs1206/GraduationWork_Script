using System;
using BIS.Manager;
using BIS.Shared;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace LJS.Test
{
    public class TestSceneLoad : MonoBehaviour
    {
        private void Update()
        {
            if (Keyboard.current.lKey.wasPressedThisFrame)
            {
                Managers.Scene.LoadScene("BattleScene");
            }
        }
    }
}

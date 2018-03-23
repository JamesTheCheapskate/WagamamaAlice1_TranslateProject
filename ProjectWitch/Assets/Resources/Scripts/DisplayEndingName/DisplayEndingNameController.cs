using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace ProjectWitch
{
    public class DisplayEndingNameController : MonoBehaviour {

        [SerializeField]
        private Text mText = null;

        [SerializeField]
        private string[] mNames = { "ノーマルエンド",
                                    "トゥルーエンド",
                                    "トゥループラスエンド"};

        // Use this for initialization
        void Start()
        {
            var game = Game.GetInstance();
            var id = game.EndingID;

            try
            {
                mText.text = mNames[id];
            }
            catch (ArgumentException e)
            {
                mText.text = "トゥルーエンド";
            }
        }

        public void End()
        {
            var game = Game.GetInstance();
            StartCoroutine(game.CallClearRecord());
        }
    }
}
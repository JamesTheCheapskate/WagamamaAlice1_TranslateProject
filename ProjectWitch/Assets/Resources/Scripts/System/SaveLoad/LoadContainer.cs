using UnityEngine;
using System.IO;
using UnityEngine.UI;

namespace ProjectWitch.Sys
{
    public class LoadContainer :SaveLoadContainerBase
    {

        public override void Reset()
        {
            base.Reset();

            var path = GamePath.GameSaveFilePath(mFileIndex);
            
            //ロードファイルの存在をチェック
            if (!File.Exists(path))
            {
                mcButton.interactable = false;
            }

        }

        //ロードを実行
        public void Load()
        {
            var game = Game.GetInstance();
            game.Setup();

            if (!game.GameData.Load(mFileIndex)) return;
            
            StartCoroutine(game.CallField());
        }
    }
}
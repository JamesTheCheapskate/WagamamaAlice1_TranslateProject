using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace ProjectWitch
{
    public class Ending : MonoBehaviour
    {
        [SerializeField]
        private List<string> mEndingList = null;

        [SerializeField]
        private List<int> mNormalEndMovieList = null;

        [SerializeField]
        private List<int> mTrueEndMovieList = null;


        // Use this for initialization
        void Start()
        {
            var game = Game.GetInstance();
            game.HideNowLoading();

            StartCoroutine(CoEnding());

        }


        //イベント制御
        private IEnumerator CoEnding()
        {
            var game = Game.GetInstance();
            
            //実行するエンディングIDの取得
            var id = game.EndingID;

            //イベントデータの設定
            var ev = new EventDataFormat();

            if(mEndingList.Count > id)
            {
                ev.FileName = mEndingList[id];
                ev.NextA = -1;
                ev.NextB = -1;
            }
            else
            {
                ev.FileName = "s9996";
                ev.NextA = -1;
                ev.NextB = -1;
            }

            //スクリプト実行
            game.CallScript(ev);
            yield return null;
            while (game.IsTalk) yield return null;

            //ムービーシーンの再生
            if (mNormalEndMovieList.Contains(id))
                SceneManager.LoadScene("EndMovie0");
            else if (mTrueEndMovieList.Contains(id))
                SceneManager.LoadScene("EndMovie1");

            //スクリプトを実行したら終了
            //タイトルの呼び出し
            StartCoroutine(game.CallTitle());

            yield break;
        }

    }
}
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace ProjectWitch
{
    public class TitleController : MonoBehaviour
    {
        //表示する選択肢ウィンドウ
        [SerializeField]
        private GameObject mChoiseWindowA = null;
        [SerializeField]
        private GameObject mChoiseWindowB = null;

        //choise window B を表示するためのフラグ
        [SerializeField]
        private int mClearFlagID = 21;

        //タイトル絵への参照
        [SerializeField]
        private Image mTitleImage = null;

        //変更するタイトル絵
        [SerializeField]
        private Sprite mTitleIllustA = null;
        [SerializeField]
        private Sprite mTitleIllustB = null;

        //タイトル絵を変更するフラグ
        [SerializeField]
        private int mChangeTitleIllustFlagID = 22;

        //ロードウィンドウへの参照
        [SerializeField]
        private GameObject mLoadGame = null;

        //コンフィグウィンドウへの参照
        [SerializeField]
        private GameObject mConfig = null;

        // Use this for initialization
        void Start()
        {
            //gameクラス初期化
            var game = Game.GetInstance();

            //クリア後のためにシステムセーブをしておく
            game.SystemData.Save();

            //ゲームデータを初期化
            game.Setup();

            //フラグによって選択肢ウィンドウの表示を切り替える
            var isClear = game.SystemData.Memory[mClearFlagID] != 0;
            mChoiseWindowA.SetActive(!isClear);
            mChoiseWindowB.SetActive(isClear);

            //フラグによってタイトル絵を切り替える
            var isNewIllust = game.SystemData.Memory[mChangeTitleIllustFlagID] != 0;
            mTitleImage.sprite = (!isNewIllust) ? mTitleIllustA : mTitleIllustB;
            
            game.HideNowLoading();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetButtonDown("Cancel"))
            {
                if (mLoadGame.activeSelf) CloseLoadWindow();
                if (mConfig.activeSelf) CloseConfigWindow();
            }
        }

        public void OnClick_Load()
        {
            mLoadGame.SetActive(true);
        }

        public void OnClick_ClearBonus()
        {
            StartCoroutine(Game.GetInstance().CallClearBonus());
        }

        public void OnClick_Config()
        {
            mConfig.SetActive(true);
        }

        //ロードウィンドウを閉じる
        public void CloseLoadWindow()
        {
            var game = Game.GetInstance();
            game.SoundManager.PlaySE(SE.Cancel);

            mLoadGame.SetActive(false);
        }

        //コンフィグウィンドウを閉じる
        public void CloseConfigWindow()
        {
            var game = Game.GetInstance();
            game.SoundManager.PlaySE(SE.Cancel);

            game.SystemData.Save();

            mConfig.SetActive(false);
        }
    }
}
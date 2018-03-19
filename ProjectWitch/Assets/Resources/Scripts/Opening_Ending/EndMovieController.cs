using UnityEngine;
using UnityEngine.Video;
using System.Collections;
using UnityEngine.UI;

namespace ProjectWitch
{
    public class EndMovieController : MonoBehaviour
    {
        [SerializeField]
        private VideoPlayer mVideoPlayer=null;

        [SerializeField]
        private AudioSource mAudioSource = null;

        [SerializeField]
        private float mSkipTime = 2.0f;
        private float mSkipProgress = 0.0f;

        [SerializeField]
        private Canvas mSkipGUI = null;
        [SerializeField]
        private Image mSkipProgressImage = null;


        bool mIsEnd = false;

        void Start()
        {
            var game = Game.GetInstance();
            game.HideNowLoading();

            //skipGUI非表示
            mSkipGUI.enabled = false;

            //音量調整
            mAudioSource.volume = game.SystemData.Config.BGMVolume * game.SystemData.Config.MasterVolume;
        }
        public void Update()
        {
            if (Input.GetButton("Submit"))
            {
                mSkipProgress += Time.deltaTime;
                mSkipGUI.enabled = true;
                mSkipProgressImage.fillAmount = mSkipProgress / mSkipTime;
            }
            else
            {
                mSkipProgress = 0.0f;
                mSkipGUI.enabled = false;
            }

            if((long)mVideoPlayer.frameCount == mVideoPlayer.frame ||
                mSkipProgress >= mSkipTime)
            {
                End();
            }
        }

        public void End()
        {
            var game = Game.GetInstance();

            if (!mIsEnd)
            {
                StartCoroutine(game.CallDisplayEndingName());
                mIsEnd = true;
            }
        }
    }
}

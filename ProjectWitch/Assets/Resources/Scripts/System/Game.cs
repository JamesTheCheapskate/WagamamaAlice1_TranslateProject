using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using UnityEngine.Analytics;
using UnityEngine.UI;


namespace ProjectWitch
{
    public class Game : MonoBehaviour
    {
        #region 定数

        //シーン名
        private const string cSceneName_Title = "Title";
        private const string cSceneName_Battle = "Battle";
        private const string cSceneName_PreBattle = "PreBattle";
        private const string cSceneName_Field = "Field";
        private const string cSceneName_Menu = "Menu";
        private const string cSceneName_Save = "Save";
        private const string cSceneName_Load = "Load";
        private const string cSceneName_Talk = "Talk";
        private const string cSceneName_ToolShop = "ToolShop";
        private const string cSceneName_MagicShop = "MagicShop";
        private const string cSceneName_Town = "Town";
        private const string cSceneName_Opening = "Opening";
        private const string cSceneName_Ending = "Ending";
        private const string cSceneName_ClearBonus = "ClearBonus";
        private const string cSceneName_ClearRecord = "ClearRecord";
        private const string cSceneName_DisplayEndingName = "DisplayEndingName";

        //読み取り専用プロパティ
        public string SceneName_Title { get { return cSceneName_Title; } private set { } }
        public string SceneName_Battle { get { return cSceneName_Battle; } private set { } }
        public string SceneName_PreBattle { get { return cSceneName_PreBattle; } private set { } }
        public string SceneName_Field { get { return cSceneName_Field; } private set { } }
        public string SceneName_Menu { get { return cSceneName_Menu; } private set { } }
        public string SceneName_Save { get { return cSceneName_Save; } private set { } }
        public string SceneName_Load { get { return cSceneName_Load; } private set { } }
        public string SceneName_Talk { get { return cSceneName_Talk; } private set { } }
        public string SceneName_ToolShop { get { return cSceneName_ToolShop; } private set { } }
        public string SceneName_MagicShop { get { return cSceneName_MagicShop; } private set { } }
        public string SceneName_Town { get { return cSceneName_Town; } private set { } }
        public string SceneName_Opening { get { return cSceneName_Opening; } private set { } }
        public string SceneName_Ending { get { return cSceneName_Ending; } private set { } }
        public string SceneName_ClearBonus { get { return cSceneName_ClearBonus; } private set { } }
        public string SceneName_ClearRecord { get { return cSceneName_ClearRecord; } private set { } }
        public string SceneName_DisplayEndingName { get { return cSceneName_DisplayEndingName; } private set { } }

        //毎ターンのHP回復率
        [SerializeField]
        private float mHPRecoveryRate = 0.2f; //20%

        //エリの改革後に各地点のマナ増加量が変化する割合
        [SerializeField]
        private float mIncrementalManaRate_AfterErisReform = 3.0f;

        #endregion

        #region 外部システム

        //サウンドマネージャ
        [SerializeField]
        private SoundManager mSoundManager = null;
        public SoundManager SoundManager { get { return mSoundManager; } private set { } }

        //Talkシーンのコマンド
        [SerializeField]
        private TalkCommandHelper mTalkCommand = null;
        public TalkCommandHelper TalkCommand { get { return mTalkCommand; } private set { } }

        //ローディング画面
        [SerializeField]
        private GameObject mNowLoadingPrefab = null;

        //タイトル画面遷移
        [SerializeField]
        private GoTitleWindow mGoTitleWindow = null;
        public GoTitleWindow GoTitle { get { return mGoTitleWindow; } }

        //デバッグ画面
        [SerializeField]
        private Text mDebugWindow = null;

        #endregion

        #region ゲームデータ関連

        //実行中のゲーム内データ
        [SerializeField]
        private GameData mGameData = null;
        public GameData GameData { get { return mGameData; } set { mGameData = value; } }
        
        //アプリケーション全体のシステムデータ
        public SystemData SystemData { get; set; }

        #endregion

        #region シーン間データ関連

        public BattleDataIn BattleIn { get; set; }
        public BattleDataOut BattleOut { get; set; }

        public ScenarioDataIn ScenarioIn { get; set; }

        public MenuDataIn MenuDataIn { get; set; }

        public TownDataIn TownDataIn { get; set; }

        //ロード直後かどうかのフラグ（fieldシーンで使用）
        public bool IsJustLoaded { get; set; }

        public int EndingID { get; set; }

        #endregion

        #region 制御変数

        //ダイアログが開いているか
        public bool IsDialogShowd { get; set; }

        //戦闘中かどうか
        public bool IsBattle { get; set; }

        //スクリプト実行中かどうか
        public bool IsTalk { get; set; }

        //町イベント中かどうか
        public bool IsTown { get; set; }

        //編成画面から戦闘に入るかどうか
        public bool UsePreBattle { get; set; }

        //アプリケーションが閉じられるときに、確認画面を表示するか
        public bool IsEnableQuitWindow { get; set; }

        #endregion

        //Singleton
        private static Game mInst;
        public static Game GetInstance()
        {
            if (mInst == null)
            {
                GameObject gameObject = Resources.Load("Prefabs/System/Game") as GameObject;
                mInst = Instantiate(gameObject).GetComponent<Game>();
            }
            return mInst;
        }
        void Awake()
        {
            DontDestroyOnLoad(this.gameObject);

            if (mInst == null)
            {
                this.Setup();
                mInst = this;
            }
            else if (mInst != this)
            {
                Destroy(this.gameObject);
            }
        }

        //初期化処理
        public void Setup()
        {
            //制御変数初期化
            IsDialogShowd = false;
            IsBattle = false;
            IsTalk = false;
            UsePreBattle = true;
            
            //シーン間データの初期化
            BattleIn = new BattleDataIn();
            BattleOut = new BattleDataOut();
            ScenarioIn = new ScenarioDataIn();
            MenuDataIn = new MenuDataIn();
            TownDataIn = new TownDataIn();
            TownDataIn.Reset();
            MenuDataIn.Reset();
            IsJustLoaded = false;
            IsEnableQuitWindow = true;

            //ゲームデータ初期化
            GameData.Reset();

            //システムデータ初期化
            SystemData = new SystemData();
            SystemData.Reset();
            SystemData.Load();
            
        }

        //シーン間データをリセット
        public void ResetSceneInputData()
        {
            BattleIn.Reset();
            BattleOut.Reset();
            ScenarioIn.Reset();
            MenuDataIn.Reset();
            EndingID = 0;
        }

        //ダイアログを表示
        public void ShowDialog(string caption, string message)
        {
            if (IsDialogShowd) return;

            GameObject prefab = (GameObject)Resources.Load("Prefabs/UI/dialog");
            if (!prefab)
                Debug.Log("ダイアログのプレハブが見つかりません");

            //インスタンス化
            var inst = Instantiate(prefab);
            inst.GetComponent<DialogWindow>().Caption = caption;
            inst.GetComponent<DialogWindow>().Text = message;

            //ダイアログ表示
            IsDialogShowd = true;

        }

        #region レベル遷移系
        //タイトルへ戻る
        public IEnumerator CallTitle()
        {
            ShowNowLoading();
            yield return null;

            yield return SceneManager.LoadSceneAsync(cSceneName_Title);

            HideNowLoading();
        }

        //エンディングを呼び出す
        public IEnumerator CallEnding(int id)
        {
            ShowNowLoading();
            yield return null;

            EndingID = id;
            SceneManager.LoadScene(cSceneName_Ending);
        }

        //おまけ画面を呼び出す
        public IEnumerator CallClearBonus()
        {
            ShowNowLoading();
            yield return null;
            yield return SceneManager.LoadSceneAsync(SceneName_ClearBonus);
            HideNowLoading();
        }

        //幸福判定を呼び出す
        public IEnumerator CallClearRecord()
        {
            ShowNowLoading();
            yield return null;
            yield return SceneManager.LoadSceneAsync(SceneName_ClearRecord);
            HideNowLoading();
        }

        public IEnumerator CallDisplayEndingName()
        {
            yield return null;
            yield return SceneManager.LoadSceneAsync(SceneName_DisplayEndingName);
        }

        //フィールドの開始
        public IEnumerator CallField()
        {
            ShowNowLoading();
            yield return null;

            yield return SceneManager.LoadSceneAsync(cSceneName_Field);

            HideNowLoading();
        }

        //メニューの呼び出し
        public IEnumerator CallMenu()
        {
            SceneManager.LoadSceneAsync(cSceneName_Menu, LoadSceneMode.Additive);

            yield return null;
        }

        //戦闘の開始
        public IEnumerator CallPreBattle()
        {

            if (UsePreBattle)
            {
                ShowNowLoading();
                yield return null;

                yield return SceneManager.LoadSceneAsync(cSceneName_PreBattle, LoadSceneMode.Additive);
            }
            else
            {
                //戦闘準備画面を出さず直接戦闘
                UsePreBattle = true;
                yield return StartCoroutine(CallBattle());
            }
        }

        public IEnumerator CallBattle()
        {
            ShowNowLoading();
            yield return null;

            BattleIn.TimeOfDay = GameData.CurrentTime;

            yield return SceneManager.LoadSceneAsync(cSceneName_Battle, LoadSceneMode.Additive);
            if (SceneManager.GetSceneByName(cSceneName_PreBattle).IsValid())
                yield return SceneManager.UnloadSceneAsync(cSceneName_PreBattle);
        }

        //スクリプトの開始
        public void CallScript(EventDataFormat e)
        {
            ScenarioIn.FileName = e.FileName;

            ScenarioIn.NextA = e.NextA;
            ScenarioIn.NextB = e.NextB;

            ScenarioIn.IsTest = false;
            IsTalk = true;
            StartCoroutine(_CallScript());

            HideNowLoading();

            //スクリプト実行のタイミングで、アリスのレベルを監視してクラウドへ送る
            Analytics.CustomEvent("CallScript_" + ScenarioIn.FileName,
                new Dictionary<string, object>
                {
                    { "alice_lv", GameData.Unit[0].Level }
                });
        }
        private IEnumerator _CallScript()
        {
            yield return SceneManager.LoadSceneAsync(cSceneName_Talk, LoadSceneMode.Additive);
        }

        //町のロード
        public void CallTown(int areaID,Field.FieldController fController,bool fromTown)
        {
            TownDataIn.AreaID = areaID;
            TownDataIn.FController = fController;
            TownDataIn.FromTown = fromTown;
            IsTown = true;

            //BGM変更
            SoundManager.Play(GameData.TownBGM, SoundType.BGM);

            SceneManager.LoadScene(cSceneName_Town, LoadSceneMode.Additive);
        }
        public void CallToolShop(int areaID, Field.FieldController fController, bool fromTown)
        {
            TownDataIn.AreaID = areaID;
            TownDataIn.FController = fController;
            TownDataIn.FromTown = fromTown;
            IsTown = true;

            //BGM変更
            SoundManager.Play(GameData.ShopBGM, SoundType.BGM);

            SceneManager.LoadScene(cSceneName_ToolShop, LoadSceneMode.Additive);
        }
        public void CallMagicShop(int areaID, Field.FieldController fController, bool fromTown)
        {
            TownDataIn.AreaID = areaID;
            TownDataIn.FController = fController;
            TownDataIn.FromTown = fromTown;
            IsTown = true;

            //BGM変更
            SoundManager.Play(GameData.ShopBGM, SoundType.BGM);

            SceneManager.LoadScene(cSceneName_MagicShop, LoadSceneMode.Additive);
        }

        //ローディング画面を表示
        public void ShowNowLoading()
        {
            if (mNowLoadingPrefab)
                mNowLoadingPrefab.SetActive(true);
        }

        //ローディング画面を非表示
        public void HideNowLoading()
        {
            if (mNowLoadingPrefab)
                mNowLoadingPrefab.SetActive(false);
        }

        #endregion

        //オートセーブする
        public void AutoSave()
        {
            GameData.Save(-1); //0スロットはオートセーブ用スロット
            SystemData.Save();
        }

        //デバッグ機能
        public void PrintDebugMessage(string message)
        {
#if DEBUG
            mDebugWindow.text = "***DEBUG MESSAGE***\n\n" + message;
#endif
        }

        //各コマンド

        //地点の領主を変更
        //targetArea:変更する地点ＩＤ
        //newOwner:新しい領主ID
        public void ChangeAreaOwner(int targetArea, int newOwner)
        {
            //領地データのエリア番号を移し替える
            //var oldOwner = GameData.Area[targetArea].Owner;
            //GameData.Territory[oldOwner].AreaList.Remove(targetArea);
            //GameData.Territory[newOwner].AreaList.Add(targetArea);

            //地点の領主番号を更新
            GameData.Area[targetArea].Owner = newOwner;

        }

        //エリアのマナを回復量だけ回復
        public void RecoverMana()
        {
            foreach (var area in GameData.Area)
            {
                //IncrementalMana分だけ、マナを回復
                //エリの改革後は増える量が増加
                if (Game.GetInstance().GameData.IsAfter_ErisReform)
                    area.Mana += (int)Math.Floor(area.IncrementalMana * mIncrementalManaRate_AfterErisReform);
                else
                    area.Mana += area.IncrementalMana;
            }
        }

        //ユニットを回復量だけ回復
        public void RecoverUnit()
        {
            foreach (var unit in GameData.Unit)
            {
                //死んでいたらスルー
                if (!unit.IsAlive) continue;

                //ユニットの回復量分兵士数を回復
                //HPは固定比率回復

                //HP回復
                unit.HP += (int)((float)unit.MaxHP * mHPRecoveryRate);
                if (unit.HP > unit.MaxHP) unit.HP = unit.MaxHP;

                //兵数回復
                unit.SoldierNum += unit.Curative;
                if (unit.SoldierNum > unit.MaxSoldierNum) unit.SoldierNum = unit.MaxSoldierNum;

            }
        }

        //敵ユニットをパワーアップ
        public void PowerUpEnemyUnit()
        {
            //自領地はスキップする
            var terData = GameData.Territory;
            for(int i=1;i<terData.Count;i++)
            {
                //占領済みではない領地が対象
                if(terData[i].State != TerritoryDataFormat.TerritoryState.Dead)
                {
                    //レベルの指針を計算
                    //自領地のユニットの、レベル数上位10ユニットの平均を指針とする
                    var playerTer = terData[0];
                    var playerUnitIDs = GameData.Group[playerTer.GroupList[0]].UnitList;
                    var playerUnits = playerUnitIDs.Select((x,index)=>GameData.Unit[x]).ToList();               //IDリストからユニットデータを取得
                    playerUnits = playerUnits.OrderBy(x => x.Level).Where((x,index) => index < 10).ToList();    //レベル順にソート
                    int lvAvr = (int)playerUnits.Average(x => x.Level);                                         //平均の計算

                    //レベル平均以下のユニットがいたら、そのユニットのレベルを上昇させる
                    var enemyUnitIDs = new List<int>();
                    //グループリストからユニットリストを生成
                    foreach(var groupID in terData[i].GroupList)
                    {
                        var groupData = GameData.Group[groupID];
                        enemyUnitIDs.AddRange(groupData.UnitList);
                    }
                    enemyUnitIDs = enemyUnitIDs.Distinct().ToList();    //重複の削除
                    var enemyUnits = enemyUnitIDs.Select((x, index) => GameData.Unit[x]).ToList();  //ユニットデータを取得

                    //ユニットごとに平均と比較してそれ-5以下なら平均の値+3を入れる
                    foreach(var unit in enemyUnits)
                    {
                        if(unit.Level < lvAvr-5)
                        {
                            unit.Level = lvAvr+3;
                        }
                    }
                }
            }
        }


    }
}
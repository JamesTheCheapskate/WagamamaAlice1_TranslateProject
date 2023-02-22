//REVISION HISTORY:
//  *JRP 2/19/23 - Translated strings to English

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using ProjectWitch.Extention;
using System.Linq;
using UnityEngine.UI;

namespace ProjectWitch.Town
{
    public class TownMenu : MonoBehaviour
    {
        //トークイベントへの参照
        [SerializeField]
        private TownTalkEvent[] mTalkEvents = null;

        //トークイベントで取得できる経験値
        [SerializeField]
        private float mExperience = 1.0f;

        [Header("UIObject")]
        //表示非表示に影響するUIオブジェクトへの参照
        [SerializeField]
        private Canvas[] mUIObjects = null;
        [SerializeField]
        private Animator[] mUIObjects_Anim = null;  //アニメ依存になっているものはこっち

        [Header("IconObjects")]
        //表示非表示に影響するアイコンオブジェクトへの参照
        [SerializeField]
        private Button[] mIconObjects = null;

        //メッセージ表示ウィンドウのプレハブ
        [SerializeField]
        private GameObject mMes = null;
        
        //component 
        protected Animator mcAnim = null;

        //閉じれるかどうか
        public bool Closable { get; set; }

        // Use this for initialization
        protected virtual void Awake()
        {
            mcAnim = GetComponent<Animator>();
            Closable = true;
        }

        public void Start()
        {
            Open();
        }

        // Update is called once per frame
        protected virtual void Update()
        {
            if (mcAnim.GetBool("IsShow") && Closable)
            {
                if (Input.GetButtonDown("Cancel"))
                {

                    //キャンセル音再生
                    Game.GetInstance().SoundManager.PlaySE(SE.Cancel);
                    Close();
                }
            }

            //メニューを開けないようにする
            //var game = Game.GetInstance();
            //game.TownDataIn.FController.MenuClickable = false;
        }

        public void Open()
        {
            mcAnim.SetBool("IsShow", true);

            //BGM変更
            var game = Game.GetInstance();
            game.SoundManager.Play(game.GameData.TownBGM, SoundType.BGM);

        }


        public void Close()
        {

            var game = Game.GetInstance();

            //BGMを戻す
            game.SoundManager.Play(game.GameData.FieldBGM, SoundType.BGM);

            //終了アニメーションを再生
            mcAnim.SetTrigger("End");
            
        }
        public void Close_End()
        {
            var game = Game.GetInstance();

            //シーンを閉じる
            game.IsTown = false;
            SceneManager.UnloadSceneAsync(game.SceneName_Town);
        }

        public void Click_ToolShop()
        {
            var game = Game.GetInstance();
            
            Closable = false;
            game.CallToolShop(game.TownDataIn.AreaID, game.TownDataIn.FController, true);
        }

        public void Click_MagicShop()
        {
            var game = Game.GetInstance();

            Closable = false;
            game.CallMagicShop(game.TownDataIn.AreaID, game.TownDataIn.FController, true);
        }

        public void ExecuteEvent(EventDataFormat e)
        {
            Closable = false;
            StartCoroutine(_EvecuteEvent(e));
        }

        private IEnumerator _EvecuteEvent(EventDataFormat e)
        {
            var game = Game.GetInstance();

            //シナリオデータをリセット
            game.ScenarioIn.Reset();

            game.CallScript(e);
            yield return null;

            //アイコンの非表示
            HideIcon();

            //会話の終了まち
            while (game.IsTalk) yield return null;

            //戦闘フラグが立っていたら戦闘に入る
            if (game.BattleIn.IsEvent)
                yield return StartCoroutine(CallBattle());

            //アイテム取得エフェクトの表示、アイテムの取得
            if(e.ItemID != -1)
            {
                if(e.IsEquipment)
                {
                    var item = game.GameData.Equipment[e.ItemID];
                    game.GameData.Territory[0].EquipmentList[e.ItemID].Add(-1);
                    yield return StartCoroutine(ShowMessage("Got " + item.Name));  //*JRP 2/19/23 To English
                }
                else
                {
                    var item = game.GameData.Card[e.ItemID];
                    game.GameData.Group[game.GameData.Territory[0].GroupList[0]].CardList.Add(e.ItemID);
                    yield return StartCoroutine(ShowMessage("Got " + item.Name));  //*JRP 2/19/23 To English
                }
            }

            //経験値の取得
            var group = game.GameData.Group[game.GameData.Territory[0].GroupList[0]];
            var units = game.GameData.Unit.GetFromIndex(group.UnitList);
            foreach (var unit in units) unit.Experience += mExperience;

            //イベント実行済みフラグを折る
            game.GameData.TownEventEnable = false;

            //リセットをかける
            foreach (var te in mTalkEvents)
            {
                te.Reset();
            }
            yield return null;
            
            //終了
            Closable = true;

            //時間経過
            //game.GameData.CurrentTime++;

            //閉じる
            Close();

            yield return null;
        }


        //バトル呼び出し
        private IEnumerator CallBattle()
        {
            var game = Game.GetInstance();

            //戦闘情報の格納
            game.BattleIn.EnemyTerritory = 0;
            game.BattleIn.IsInvasion = true;

            //戦闘に入力する情報が正しいかのチェック
            //編成画面なしの場合、プレイヤーがセットされているかチェック、セットされていないならアリスをセット
            if (!game.UsePreBattle && game.BattleIn.PlayerUnits.Where(x => x != -1).ToList().Count == 0)
                game.BattleIn.PlayerUnits[0] = 0;
            //敵情報がセットされているかチェック、セットされていないなら自営団をセット
            var defaultUnit = game.GameData.Group[GroupDataFormat.GetDefaultID()].UnitList[0];
            if (game.BattleIn.EnemyUnits.Where(x => x != -1).ToList().Count == 0)
                game.BattleIn.EnemyUnits[0] = defaultUnit;

            //戦闘呼び出し
            yield return StartCoroutine(game.CallPreBattle());

            yield return null;

            //UIを隠す
            HideUI();

            //戦闘終了まで待機
            while (game.IsBattle) yield return null;

            //戦闘終了処理
            yield return StartCoroutine(AfterBattle());
        }

        //戦闘後処理
        public IEnumerator AfterBattle()
        {
            var game = Game.GetInstance();

            //ユニットの死亡処理
            foreach (var unit in game.BattleOut.DeadUnits)
            {
                game.GameData.Unit[unit].IsAlive = false;

                //すべての領地からユニットを除外
                foreach (var territory in game.GameData.Territory)
                {
                    territory.RemoveUnit(unit);
                }

                //すべてのグループからユニットを除外
                foreach (var group in game.GameData.Group)
                {
                    group.UnitList.Remove(unit);
                }
            }

            //ユニットの捕獲処理
            var dist = game.BattleOut.CapturedUnits.Distinct().ToList();
            foreach (var unit in dist)
            {
                //味方領地に追加
                var territory = game.GameData.Territory[game.BattleIn.PlayerTerritory];
                territory.AddUnit(unit);
            }

            //ユニットの逃走処理
            foreach (var unit in game.BattleOut.EscapedUnits)
            {
                //回復処理
                var udata = game.GameData.Unit[unit];
                udata.HP = (int)(udata.MaxHP * 0.3f);
                udata.SoldierNum = (int)(udata.MaxSoldierNum * 0.3f);
            }

            //経験値処理
            var enemyIdList = game.BattleIn.EnemyUnits.Where(x => x != -1).ToList();
            UnitDataFormat[] enemyList = game.GameData.Unit.GetFromIndex(enemyIdList).ToArray();
            foreach (var unitID in game.BattleIn.PlayerUnits)
            {
                if (unitID < 0) continue;
                var unit = game.GameData.Unit[unitID];
                unit.Experience += unit.CalcBattleExperience(enemyList, game.BattleOut.IsWin);
            }

            ////グループの消滅処理
            //foreach (var group in game.GameData.Group)
            //{
            //    //ユニットの残数が0なら消滅
            //    if (group.UnitList.Count == 0)
            //        group.Kill();


            //    //死守領地がない場合は続行
            //    if (group.DominationRoute.Count == 0) continue;

            //    //稼働状態で、死守領地が落ちたら消滅
            //    if (group.State == GroupDataFormat.GroupState.Active &&
            //        game.GameData.Area[group.DominationRoute[0]].Owner == game.BattleIn.PlayerTerritory)
            //    {
            //        group.Kill();
            //    }
            //}

            //カードの処理(自軍のときのみ有効)
            if (game.BattleIn.PlayerTerritory == 0)
            {
                foreach (var cardID in game.BattleOut.UsedCards)
                {
                    var group = game.GameData.Group[game.GameData.Territory[0].GroupList[0]];
                    group.CardList.Remove(cardID);
                }
            }

            //自営軍の復活
            var g = game.GameData.Group[GroupDataFormat.GetDefaultID()];
            g.Rebirth();    //グループの復活
            foreach (var unit in g.UnitList)
            {
                //ユニットの復活
                game.GameData.Unit[unit].Rebirth();
            }

            //アリスが死んでいたらゲームオーバー
            if (game.GameData.Unit[0].IsAlive == false)
            {
                StartCoroutine(game.CallEnding(14));
            }

            //アリスの館が落ちたらゲームオーバー
            if (game.GameData.Area[1].Owner != 0)
            {
                StartCoroutine(game.CallEnding(15));
            }

            //フィールドUIを再ロード
            ShowUI();
            yield return null;
            game.HideNowLoading();
            yield return null;

            //戦闘後スクリプトの開始
            if (game.BattleOut.IsWin)    //戦闘勝利時のスクリプト
            {
                var exescript = game.ScenarioIn.NextA;
                if (exescript >= 0)
                    game.CallScript(game.GameData.TownEvent[exescript]);
                yield return null;
            }
            else                        //戦闘敗北時のスクリプト
            {
                var exescript = game.ScenarioIn.NextB;
                if (exescript >= 0)
                    game.CallScript(game.GameData.TownEvent[exescript]);
                yield return null;

            }
            while (game.IsTalk) yield return null;

            game.BattleIn.Reset();
            game.BattleOut.Reset();
            game.ScenarioIn.Reset();

            yield return null;
        }

        //アイテム取得エフェクト
        public IEnumerator ShowMessage(string str)
        {
            //アイテムゲットウィンドウ
            var inst = Instantiate(mMes);
            inst.transform.SetParent(this.transform, false);
            var comp = inst.GetComponent<Field.Mana.Message>();
            comp.Text = str;

            inst = Instantiate(mMes);
            inst.transform.SetParent(this.transform, false);
            inst.transform.position += new Vector3(0.0f, -100.0f, 0.0f);
            comp = inst.GetComponent<Field.Mana.Message>();
            comp.Text = "Gained Experience！";  //*JRP 2/19/23 - To English

            yield return new WaitForSeconds(comp.LifeTime);
        }

        //UIを表示する
        public void ShowUI()
        {
            var game = Game.GetInstance();

            game.TownDataIn.FController.ShowUI();

            foreach (var c in mUIObjects)
            {
                c.enabled = true;
            }
            foreach (var c in mUIObjects_Anim)
            {
                c.SetBool("IsShow", true);
            }
        }

        //UIを非表示にする
        public void HideUI()
        {
            var game = Game.GetInstance();

            game.TownDataIn.FController.HideUI();

            foreach (var c in mUIObjects)
            {
                c.enabled = false;
            }
            foreach (var c in mUIObjects_Anim)
            {
                c.SetBool("IsShow", false);
            }
        }

        //Iconを非表示にする
        public void HideIcon()
        {
            foreach(var c in mIconObjects)
            {
                c.interactable=false;
            }
        }

    }
}
/*using MCM.Abstractions;
//using System.Configuration;
///MCMv3
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Settings.Base.Global;
//using Bannerlord.UIExtenderEx;
using MCM.Abstractions.Settings.Formats;*/
using MCM.Abstractions;
using MCM.Abstractions.Attributes;
using MCM.Abstractions.Attributes.v2;
using MCM.Abstractions.Settings.Base.Global;
using MCM.Abstractions.Settings.Formats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
//using MyQueue;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade.Source.Missions;

namespace Reinforcements
{

    public class Main : MBSubModuleBase
    {

        protected override void OnSubModuleLoad()
        {

        }
        protected override void OnGameStart(Game game, IGameStarter IGS)
        {
            base.OnGameStart(game, IGS);
            //InformationManager.DisplayMessage(new InformationMessage("Inside OnGameStart v2.0.0")); //initialize mod
            CampaignGameStarter temp = IGS as CampaignGameStarter;

            bool flag = !(game.GameType is Campaign);
            if (!flag)
            {
                try
                {
                    temp.AddBehavior(new campainSystems()); //adds campaignsystems behavior

                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Not campaign");
                }

            }


        }

        public override void OnCampaignStart(Game game, object starterObject)
        {
            base.OnCampaignStart(game, starterObject);

        }
        public override void OnMissionBehaviourInitialize(Mission mission)
        {

            base.OnMissionBehaviourInitialize(mission);
            //InformationManager.DisplayMessage(new InformationMessage("Inside OnMissionBehaviour inside Main"));
            //System.Diagnostics.Debug.WriteLine("Virus installed"); //talking to someone like a party also could be used when excatly at the fighting menu




        }




        private void AddBehaviours(CampaignGameStarter starter)
        {
            starter.AddBehavior(new campainSystems());


        }

    }

    public class CustomBattleEndLogic : MissionLogic
    {

        // Token: 0x170005AD RID: 1453
        // (get) Token: 0x06001C62 RID: 7266 RVA: 0x0006127C File Offset: 0x0005F47C
        public bool PlayerVictory
        {
            get
            {
                return this._isEnemySideRetreating || this._isEnemySideDepleted;
            }
        }

        // Token: 0x170005AE RID: 1454
        // (get) Token: 0x06001C63 RID: 7267 RVA: 0x0006128E File Offset: 0x0005F48E
        public bool EnemyVictory
        {
            get
            {
                return this._isPlayerSideDepleted;
            }
        }

        // Token: 0x170005AF RID: 1455
        // (get) Token: 0x06001C64 RID: 7268 RVA: 0x00061296 File Offset: 0x0005F496
        // (set) Token: 0x06001C65 RID: 7269 RVA: 0x0006129E File Offset: 0x0005F49E
        private bool _notificationsDisabled { get; set; }

        // Token: 0x06001C66 RID: 7270 RVA: 0x000612A8 File Offset: 0x0005F4A8
        public override bool MissionEnded(ref MissionResult missionResult)
        {
            bool flag = false;
            if (Mission.Current.PlayerEnemyTeam.ActiveAgents.Count() < 1)
            {
                missionResult = MissionResult.CreateSuccessful(base.Mission);
                flag = true;
            }
            else if (Mission.Current.PlayerTeam.ActiveAgents.Count() < 1)
            {
                missionResult = MissionResult.CreateDefeated(base.Mission);
                flag = true;
            }
            if (flag)
            {
                //this._missionAgentSpawnLogic.StopSpawner();
            }
            return flag;
        }

        // Token: 0x06001C67 RID: 7271 RVA: 0x00061300 File Offset: 0x0005F500
        public override void OnMissionTick(float dt)
        {
            if (base.Mission.IsMissionEnding)
            {
                if (this._notificationsDisabled)
                {
                    this._scoreBoardOpenedOnceOnMissionEnd = true;
                }
                if (this._missionEndedMessageShown && !this._scoreBoardOpenedOnceOnMissionEnd)
                {
                    if (this._checkRetreatingTimer.ElapsedTime > 7f)
                    {
                        this.CheckIsEnemySideRetreatingOrOneSideDepleted();
                        this._checkRetreatingTimer.Reset();
                        if (base.Mission.MissionResult != null && base.Mission.MissionResult.PlayerDefeated)
                        {
                            GameTexts.SetVariable("leave_key", Game.Current.GameTextManager.GetHotKeyGameText("CombatHotKeyCategory", 4));
                            InformationManager.AddQuickInformation(GameTexts.FindText("str_battle_lost_press_tab_to_view_results", null), 0, null, "");
                        }
                        else if (base.Mission.MissionResult != null && base.Mission.MissionResult.PlayerVictory)
                        {
                            if (this._isEnemySideDepleted)
                            {
                                GameTexts.SetVariable("leave_key", Game.Current.GameTextManager.GetHotKeyGameText("CombatHotKeyCategory", 4));
                                InformationManager.AddQuickInformation(GameTexts.FindText("str_battle_won_press_tab_to_view_results", null), 0, null, "");
                            }
                        }
                        else
                        {
                            GameTexts.SetVariable("leave_key", Game.Current.GameTextManager.GetHotKeyGameText("CombatHotKeyCategory", 4));
                            InformationManager.AddQuickInformation(GameTexts.FindText("str_battle_finished_press_tab_to_view_results", null), 0, null, "");
                        }
                    }
                }

                if (!this._victoryReactionsActivated)
                {
                    AgentVictoryLogic missionBehaviour = base.Mission.GetMissionBehaviour<AgentVictoryLogic>();
                    if (missionBehaviour != null)
                    {
                        this.CheckIsEnemySideRetreatingOrOneSideDepleted();
                        if (this._isEnemySideDepleted)
                        {
                            missionBehaviour.SetTimersOfVictoryReactions(base.Mission.PlayerTeam.Side);
                            this._victoryReactionsActivated = true;
                            return;
                        }
                        if (this._isPlayerSideDepleted)
                        {
                            missionBehaviour.SetTimersOfVictoryReactions(base.Mission.PlayerEnemyTeam.Side);
                            this._victoryReactionsActivated = true;
                            return;
                        }
                        if (this._isEnemySideRetreating && !this._victoryReactionsActivatedForRetreating)
                        {
                            missionBehaviour.SetTimersOfVictoryReactionsForRetreating(base.Mission.PlayerTeam.Side);
                            this._victoryReactionsActivatedForRetreating = true;
                            return;
                        }
                    }
                }
            }

        }

        // Token: 0x06001C68 RID: 7272 RVA: 0x00061604 File Offset: 0x0005F804
        public void ChangeCanCheckForEndCondition(bool canCheckForEndCondition)
        {
            this._canCheckForEndCondition = canCheckForEndCondition;
        }

        private bool CustomIsSideDepleted(BattleSideEnum side)
        {
            if (Mission.Current.AttackerTeam.Side == side)
                if (Mission.Current.AttackerTeam.ActiveAgents.Count() < 1)
                    return true;
                else
                    return false;
            else if (Mission.Current.DefenderTeam.Side == side)
                if (Mission.Current.DefenderTeam.ActiveAgents.Count() < 1)
                    return true;
                else
                    return false;
            return false;
        }

        // Token: 0x06001C69 RID: 7273 RVA: 0x00061610 File Offset: 0x0005F810
        private void CheckIsEnemySideRetreatingOrOneSideDepleted()
        {
            if (!this._canCheckForEndConditionSiege)
            {
                this._canCheckForEndConditionSiege = (base.Mission.GetMissionBehaviour<SiegeDeploymentHandler>() == null);
                return;
            }
            if (this._canCheckForEndCondition)
            {
                BattleSideEnum side = base.Mission.PlayerTeam.Side;
                this._isPlayerSideDepleted = CustomIsSideDepleted(side);
                this._isEnemySideDepleted = CustomIsSideDepleted(side.GetOppositeSide());
                if (this._isEnemySideDepleted || this._isPlayerSideDepleted)
                {
                    return;
                }
                if (base.Mission.GetMissionBehaviour<HideoutPhasedMissionController>() != null)
                {
                    return;
                }
                bool flag = true;
                foreach (Team team in base.Mission.Teams)
                {
                    if (team.IsEnemyOf(base.Mission.PlayerTeam))
                    {
                        using (List<Agent>.Enumerator enumerator2 = team.ActiveAgents.GetEnumerator())
                        {
                            while (enumerator2.MoveNext())
                            {
                                if (!enumerator2.Current.IsRunningAway)
                                {
                                    flag = false;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (!flag)
                {
                    this._enemiesNotYetRetreatingTime = MissionTime.Now;
                }
                if (this._enemiesNotYetRetreatingTime.ElapsedSeconds > 3f)
                {
                    this._isEnemySideRetreating = true;
                }
            }
        }

        // Token: 0x06001C6A RID: 7274 RVA: 0x00061760 File Offset: 0x0005F960
        public BattleEndLogic.ExitResult TryExit()
        {
            if (GameNetwork.IsClientOrReplay)
            {
                return BattleEndLogic.ExitResult.False;
            }
            if ((base.Mission.MainAgent != null && base.Mission.MainAgent.IsActive() && base.Mission.IsPlayerCloseToAnEnemy(5f)) || (!base.Mission.MissionEnded() && (this.PlayerVictory || this.EnemyVictory)))
            {
                return BattleEndLogic.ExitResult.False;
            }
            if (!base.Mission.MissionEnded() && !this._isEnemySideRetreating)
            {
                return BattleEndLogic.ExitResult.NeedsPlayerConfirmation;
            }
            base.Mission.EndMission();
            return BattleEndLogic.ExitResult.True;
        }

        // Token: 0x06001C6B RID: 7275 RVA: 0x000617E9 File Offset: 0x0005F9E9
        public override void OnBehaviourInitialize()
        {
            base.OnBehaviourInitialize();
            this._checkRetreatingTimer = new BasicTimer(MBCommon.TimeType.Mission);
            //this._missionAgentSpawnLogic = base.Mission.GetMissionBehaviour<IMissionAgentSpawnLogic>();
        }

        // Token: 0x06001C6C RID: 7276 RVA: 0x00061810 File Offset: 0x0005FA10
        protected override void OnEndMission()
        {
            if (this._isEnemySideRetreating)
            {
                foreach (Agent agent in base.Mission.PlayerEnemyTeam.ActiveAgents)
                {
                    IAgentOriginBase origin = agent.Origin;
                    if (origin != null)
                    {
                        origin.SetRouted();
                    }
                }
            }
        }

        // Token: 0x170005B0 RID: 1456
        // (get) Token: 0x06001C6D RID: 7277 RVA: 0x00061880 File Offset: 0x0005FA80
        public bool IsEnemySideRetreating
        {
            get
            {
                return this._isEnemySideRetreating;
            }
        }

        // Token: 0x06001C6E RID: 7278 RVA: 0x00061888 File Offset: 0x0005FA88
        public void SetNotificationDisabled(bool value)
        {
            this._notificationsDisabled = value;
        }

        // Token: 0x04000A5C RID: 2652

        // Token: 0x04000A5D RID: 2653
        private MissionTime _enemiesNotYetRetreatingTime;

        // Token: 0x04000A5E RID: 2654
        private BasicTimer _checkRetreatingTimer;

        // Token: 0x04000A5F RID: 2655
        private bool _isEnemySideRetreating;

        // Token: 0x04000A60 RID: 2656
        private bool _isEnemySideDepleted;

        // Token: 0x04000A61 RID: 2657
        private bool _isPlayerSideDepleted;

        // Token: 0x04000A62 RID: 2658
        private bool _canCheckForEndCondition = true;

        // Token: 0x04000A63 RID: 2659
        private bool _canCheckForEndConditionSiege;

        // Token: 0x04000A64 RID: 2660
        private bool _missionEndedMessageShown;

        // Token: 0x04000A65 RID: 2661
        private bool _victoryReactionsActivated;

        // Token: 0x04000A66 RID: 2662
        private bool _victoryReactionsActivatedForRetreating;

        // Token: 0x04000A67 RID: 2663
        private bool _scoreBoardOpenedOnceOnMissionEnd;

        // Token: 0x02000511 RID: 1297
        public enum ExitResult
        {
            // Token: 0x040019F5 RID: 6645
            False,
            // Token: 0x040019F6 RID: 6646
            NeedsPlayerConfirmation,
            // Token: 0x040019F7 RID: 6647
            True
        }

        /*public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            GameTexts.SetVariable("leave_key", Game.Current.GameTextManager.GetHotKeyGameText("CombatHotKeyCategory", 4));
            InformationManager.AddQuickInformation(GameTexts.FindText("str_battle_lost_press_tab_to_view_results", null), 0, null, "");
        }
        public void checkIfEnd()
        {
            if (Mission.Current.PlayerTeam.ActiveAgents.Count() < 1)
            {
                GameTexts.SetVariable("leave_key", Game.Current.GameTextManager.GetHotKeyGameText("CombatHotKeyCategory", 4));
                InformationManager.AddQuickInformation(GameTexts.FindText("str_battle_lost_press_tab_to_view_results", null), 0, null, "");
                MissionResult.CreateDefeated(Mission.Current);
                MissionLogic logicB = Mission.Current.MissionLogics.ElementAt(5);
                BattleEndLogic logicA = logicB as BattleEndLogic;
                logicA.TryExit();
                
            }
        }*/
    }


    static class Extensions
    {
        public static List<T> ICLone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }
    }


    /// MCMv3
    /// 
    public interface IYamlSettingsFormat : ISettingsFormat, IDependency
    {

    }

    internal sealed class MCMUISettings : AttributeGlobalSettings<MCMUISettings> // AttributePerCharacterSettings<MCMUISettings>
    {
        private bool _useStandardOptionScreen = false;

        public override string Id => "AIReinforcements";
        public override string DisplayName => $"AI Reinforcements. {typeof(MCMUISettings).Assembly.GetName().Version.ToString(3)}";
        public override string FolderName => "Reinforcements";
        public override string Format => "json";

        [SettingPropertyBool("Use Standard Option Screen", Order = 1, RequireRestart = false, HintText = "Use standard Options screen instead of using an external.")]
        [SettingPropertyGroup("General")]
        public bool UseStandardOptionScreen
        {
            get => _useStandardOptionScreen;
            set
            {
                if (_useStandardOptionScreen != value)
                {
                    _useStandardOptionScreen = value;
                    OnPropertyChanged();
                }
            }
        }

        [SettingPropertyBool("Allow Minor Party Reinforcements", Order = 1, RequireRestart = false, HintText = "Minor parties such as looters and bandits")]
        [SettingPropertyGroup("Main Settings/Options/Reinforcements")]
        public bool SettingAllowMinorParties { get; set; } = false;

        [SettingPropertyBool("Allow Siege Reinforcements", Order = 2, RequireRestart = false, HintText = "Minor parties such as looters and bandits")]
        [SettingPropertyGroup("Main Settings/Options/Reinforcements")]

        public bool SettingAllowSeigeReinforcements { get; set; } = false;
        [SettingPropertyBool("Allow Player Caravan Reinforcements", Order = 3, RequireRestart = false, HintText = "Setting explanation.")]
        [SettingPropertyGroup("Main Settings/Options/Reinforcements")]
        public bool SettingPlayerCaravanReinforcements { get; set; } = false;


        [SettingPropertyBool("Allow Enemy Caravan Reinforcements", Order = 4, RequireRestart = false, HintText = "Setting explanation.")]
        [SettingPropertyGroup("Main Settings/Options/Reinforcements")]
        public bool SettingEnemyCaravanReinforcements { get; set; } = true;


        [SettingPropertyFloatingInteger("Setting PlayerJoinRatio", 0f, 2f, "#0%", Order = 2, RequireRestart = false, HintText = "Setting explanation.")]
        [SettingPropertyGroup("Main Settings/Options/Ratios")]
        public float SettingPlayerJoinRatio { get; set; } = 0.80f;

        [SettingPropertyFloatingInteger("Setting EnemyJoinRatio", 0f, 2f, "#0%", Order = 2, RequireRestart = false, HintText = "Setting explanation.")]
        [SettingPropertyGroup("Main Settings/Options/Ratios")]
        public float SettingEnemyJoinRatio { get; set; } = 1.10f;

        // Value is displayed as "X Denars"


        // Value is displayed as a percentage
        [SettingPropertyFloatingInteger("Speed parties join from distance", 0f, 4f, "#0%", Order = 2, RequireRestart = false, HintText = "0 = imediatly, 2 = normal, 4 = double the time")]
        [SettingPropertyGroup("Main Settings/Options/Ratios")]
        public float SettingTimeToSpawnRatio { get; set; } = 2.0f;


    }


    public class campainSystems : CampaignBehaviorBase
    {
        //non original reinforcement variables
        static List<MobileParty> AllyNearbyParties = new List<MobileParty>();
        static List<MobileParty> EnemyNearbyParties = new List<MobileParty>();
        static List<float> EnemydistancePartyList = new List<float>();
        static List<float> AllydistancePartyList = new List<float>();
        static List<PartyBase> OriginalInvolvedParties = new List<PartyBase>();
        static MBReadOnlyList<Agent> EnemyactiveAgents;
        static MBReadOnlyList<Agent> AllyactiveAgents;
        static List<IAgentOriginBase> jList = new List<IAgentOriginBase>();
        static List<IAgentOriginBase> AList = new List<IAgentOriginBase>(); //ally troops

        static List<IAgentOriginBase> EnemiesInQueue = new List<IAgentOriginBase>();
        static List<IAgentOriginBase> AlliesInQueue = new List<IAgentOriginBase>();
        static bool lockEnemySpawnNewReinforcements = false;
        static bool lockAllySpawnNewReinforcements = false;
        static bool calculateNextPartyLock = false;


        //original reinforcement variables
        static List<IAgentOriginBase> OriginalAllyReinforcements = new List<IAgentOriginBase>(); //ally and enemy org reinforcements
        static List<IAgentOriginBase> OriginalEnemyReinforcements = new List<IAgentOriginBase>();
        static int InitialEnemyTroopCount = new int();
        static int InitialAllyTroopCount = new int();
        static bool stopRepeat = false;

        //caravans
        static MobileParty EnemyPartyMainMobileParty;


        static Agent holdEnemyAgent = null;
        static Agent holdPlayerAgent = null;

        static bool lordParty = false;
        static bool modActive = false;
        static bool maxTroopReached = false;
        static bool gameFinished = false;


        static List<TextObject> partiesInFight = new List<TextObject>();

        static float BigSpawnTimer = 9999999.9f;
        static Queue<PartyBase> partyQueue = new Queue<PartyBase>();

        IEnumerable<PartyBase> currentInvolvedParties;
        static List<PartyGroupTroopSupplier> listPGTS = new List<PartyGroupTroopSupplier>();


        static bool testing = false;

        /*class AgentPartyOriginList
        {
            private static AgentPartyOriginList _AgentPartyOriginList = new AgentPartyOriginList();
            public static AgentPartyOriginList partyList
            {
                get { return _AgentPartyOriginList; }
            }
            private List<IAgentOriginBase> PartyOriginList = new List<IAgentOriginBase>();
            private AgentPartyOriginList(List<IAgentOriginBase> newlist)
            {
                PartyOriginList = newlist;
            }

        }*/


        public override void RegisterEvents()
        {

            /*CampaignEvents.HourlyTickEvent.AddNonSerializedListener(this, new Action(this.printBS));*/

            //InformationManager.DisplayMessage(new InformationMessage("Inside RegisterEvents")); //



            /*CampaignEvents.PartyEncounteredEvent.AddNonSerializedListener(this, new Action<PartyBase, PartyBase>((AIParty, PlayerParty) =>
            {
                InformationManager.DisplayMessage(new InformationMessage("Party Encounter"));
                System.Diagnostics.Debug.WriteLine(" Party Encounter method");

            }));*/


            /*CampaignEvents.PlayerMetCharacter.AddNonSerializedListener(this, new Action<Hero>(characterHero =>
            {
                InformationManager.DisplayMessage(new InformationMessage("PlayerMetCharacter")); //meets for the first time

            }));*/

            



            CampaignEvents.MapEventStarted.AddNonSerializedListener(this, new Action<MapEvent, PartyBase, PartyBase>((One, two, three) =>
            {
                //InformationManager.DisplayMessage(new InformationMessage("MapEventStarted inside campaignsystem")); //idk spammed all the time

                //System.Diagnostics.Debug.WriteLine(MapEvent.PlayerMapEvent.ToString()); does not work on already involved fights



                if (MapEvent.PlayerMapEvent == null)
                {
                    return;
                }

                // a lock?
                //TryToStartMod();


            }));

            /// Every tick this code runs
            //MapEvent.PlayerMapEvent.HasWinner
            CampaignEvents.MissionTickEvent.AddNonSerializedListener(this, new Action<float>(one =>
            { //mission tick

                /*if (modActive == false)
                    return;*/

                if (Mission.Current.IsFieldBattle)
                {
                    //MapEvent.PlayerMapEvent.DefenderSide.RecalculateMemberCountOfSide();

                    if ((Mission.Current.PlayerTeam.ActiveAgents.Count() < 1 || Mission.Current.PlayerEnemyTeam.ActiveAgents.Count() < 1) && stopRepeat == false)
                    {
                        //MapEvent.PlayerMapEvent.CheckIfOneSideHasLost();
                        stopRepeat = true;
                        CustomBattleEndLogic temp = new CustomBattleEndLogic();
                        //temp.checkIfEnd();
                    }
                }

                try
                {
                    if (Mission.Current.Time > 10.0f && Mission.Current.PlayerTeam.ActiveAgents.Count() > 1 && Mission.Current.IsFieldBattle)
                    {
                        // respawns units based on if the count is < 10 of original & current logic maintains the original troop proportions
                        if (InitialAllyTroopCount - 9 > Mission.Current.PlayerTeam.ActiveAgents.Count() && !OriginalAllyReinforcements.IsEmpty())
                            spawnOriginalReinforcements(OriginalAllyReinforcements, true);
                        if (InitialEnemyTroopCount - 9 > Mission.Current.PlayerEnemyTeam.ActiveAgents.Count() && !OriginalEnemyReinforcements.IsEmpty())
                            spawnOriginalReinforcements(OriginalEnemyReinforcements, false);
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Caught exception at MissionTick of spawn OG Reinforcements");
                    resetMod();
                    return;
                }

                if (!EnemiesInQueue.IsEmpty() && lockEnemySpawnNewReinforcements == false)
                {
                    lockEnemySpawnNewReinforcements = true;
                    spawnNewReinforcements(EnemiesInQueue, false, Mission.Current.PlayerEnemyTeam);
                }
                if (!AlliesInQueue.IsEmpty() && lockAllySpawnNewReinforcements == false)
                {
                    lockAllySpawnNewReinforcements = true;
                    spawnNewReinforcements(AlliesInQueue, true, Mission.Current.PlayerTeam);
                }
                /*if(Mission.Current.Time >= 10.0f && testing == false)
                {
                    tryMissionAgentSpawnLogic();
                    Mission currMis = Mission.Current;
                    int h2 = 2;
                }*/


                try
                {
                    if (Mission.Current.Time >= BigSpawnTimer && calculateNextPartyLock == false)
                    {


                        calculateNextPartyLock = true;
                        calculateNextParty();
                    }
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Caught exception at calculating time and big spawn timer to calculate nextParty");
                    resetMod();
                    return;
                }
                //InformationManager.DisplayMessage(new InformationMessage(Mission.Current.Time.ToString())); //inside the mission use this to wait to spawn


                //System.Diagnostics.Debug.WriteLine((Mission.Current.PlayerTeam.ActiveAgents.Count() + Mission.Current.PlayerEnemyTeam.ActiveAgents.Count()).ToString() + " " + BannerlordConfig.BattleSize.ToString());

            }));



            CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener(this, new Action<IMission>(one =>
            {


            }));

            //CampaignEvents.SetupPreConversationEvent.AddNonSerializedListener(this, new Action(this.setup)); //before convo works on looters



            // checks if mission neds to happen pt 2
            CampaignEvents.MissionStarted.AddNonSerializedListener(this, new Action<IMission>((one) =>
            {

                
                System.Diagnostics.Debug.WriteLine("Hello Mission Started");
                resetMod();
                if (EnemyNearbyParties.IsEmpty() && AllyNearbyParties.IsEmpty() && Mission.Current.IsFieldBattle)//double check to make sure code is run
                {
                    BeginModFromMissionStarted();
                }


            }));
        }

        private async void BeginModFromMissionStarted()
        {
            await TryToStartMod();

            if (lordParty == true && (!EnemyNearbyParties.IsEmpty() || !AllyNearbyParties.IsEmpty()) && Mission.Current.IsFieldBattle)
            {

                InformationManager.DisplayMessage(new InformationMessage("You hear the echo of " + (EnemyNearbyParties.Count() + AllyNearbyParties.Count()) + " parties in the distance!")); //debug
                System.Diagnostics.Debug.WriteLine("Calculating first bigTimer");

                calculateFirstTime();
            }
            else
                System.Diagnostics.Debug.WriteLine("Mission started Empty party list exiting or not lord party");
        }
        private async Task TryToStartMod()
        {
            if (MapEvent.PlayerMapEvent.IsFieldBattle == true)
            {
                if (String.IsNullOrEmpty(MapEvent.PlayerMapEvent.GetName().ToString()))
                    throw new Exception("name parameter must contain a value!");

                System.Diagnostics.Debug.WriteLine("Mod attempted to start");

                if (MapEvent.PlayerMapEvent.PlayerSide == (BattleSideEnum)1) //1 = attacker 0 = defender
                {
                    await storeNearbyArmiesCS(MapEvent.PlayerMapEvent.GetLeaderParty((BattleSideEnum)0)); //store enemy side initial party
                }
                else
                    await storeNearbyArmiesCS(MapEvent.PlayerMapEvent.GetLeaderParty((BattleSideEnum)1)); //store enemy side

            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Not field battle");
                return;
            }
            return;
        }

        public Task storeNearbyArmiesCS(PartyBase enemyParty)
        {

            EnemyPartyMainMobileParty = enemyParty.MobileParty; //hnaldes cases in the future

            if (!enemyParty.MobileParty.IsLordParty && !enemyParty.MobileParty.IsCaravan) //exempts looters and other minor factions that aren't lord parties
            {
                System.Diagnostics.Debug.WriteLine("Not lord party exiting storenearbyArmies");
                return Task.CompletedTask;
            }
            if (enemyParty.MobileParty.IsCaravan && MCMUISettings.Instance.SettingEnemyCaravanReinforcements == false && MCMUISettings.Instance.SettingPlayerCaravanReinforcements == false)
            { //deals with allow caravan reinforcements
                System.Diagnostics.Debug.WriteLine("Caravan but player disabled caravan reinforcments");
                return Task.CompletedTask;
            }

                

            resetMod();
            lordParty = true;
            System.Diagnostics.Debug.WriteLine("Storing neraby Armies");


            //await TaskDelay(1500); //give time for involved parties
            //System.Diagnostics.Debug.WriteLine("Storing nearby armies");
            //System.Diagnostics.Debug.WriteLine(enemyParty.ToString()); //correct even though when watching party it shows main party weird
            PartyBase thisParty = enemyParty;
            try
            {
                currentInvolvedParties = MapEvent.PlayerMapEvent.InvolvedParties;

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Caught exception getting involved parties");
                return Task.CompletedTask;
            }
            IEnumerable<MobileParty> nearyParties = MobileParty.FindPartiesAroundPosition(MobileParty.MainParty.GetPosition2D, MobileParty.MainParty.SeeingRange);

            for (int i = 0; i <= nearyParties.Count() - 1; i++)
            {
                MobileParty temp = nearyParties.ElementAt(i);
                if ((temp.MapFaction == enemyParty.MapFaction) && IsWithinInvoledParties(currentInvolvedParties, temp) == false)
                {
                    if (temp.IsLordParty && temp.MapEvent == null)
                    {
                        System.Diagnostics.Debug.WriteLine(temp.Name.ToString() + " " + temp.GetTrackDistanceToMainAgent());

                        EnemyNearbyParties.Add(temp);
                    }

                }
                else if (temp.MapFaction == MobileParty.MainParty.MapFaction && IsWithinInvoledParties(currentInvolvedParties, temp) == false)
                {
                    if (temp.IsLordParty && temp.MapEvent == null)
                    {
                        System.Diagnostics.Debug.WriteLine(temp.Name.ToString() + " " + temp.GetTrackDistanceToMainAgent());
                        AllyNearbyParties.Add(temp);
                    }
                }
            }

            sortArmyListByDistance();
            System.Diagnostics.Debug.WriteLine("End storing parties " + EnemyNearbyParties.Count() + " enemy parties available");
            System.Diagnostics.Debug.WriteLine("End storing parties " + AllyNearbyParties.Count() + " ally parties available");
            return Task.CompletedTask;



        }

        private static bool IsWithinInvoledParties(IEnumerable<PartyBase> partyList, MobileParty singeParty)
        {
            for (int i = 0; i < partyList.Count(); i++)
            {
                if (singeParty == partyList.ElementAt(i).MobileParty)
                {
                    return true;
                }
            }
            return false;
        }

        private void sortArmyListByDistance()
        {
            EnemyNearbyParties.Sort((a, b) => (a.GetTrackDistanceToMainAgent().CompareTo(b.GetTrackDistanceToMainAgent())));
            AllyNearbyParties.Sort((a, b) => (a.GetTrackDistanceToMainAgent().CompareTo(b.GetTrackDistanceToMainAgent())));

            for (int i = 0; i <= EnemyNearbyParties.Count() - 1; i++) //store distance values becuz they somehow change later..
            {
                EnemydistancePartyList.Add(EnemyNearbyParties.ElementAt(i).GetTrackDistanceToMainAgent());
            }
            for (int i = 0; i <= AllyNearbyParties.Count() - 1; i++) //store distance values becuz they somehow change later..
            {
                AllydistancePartyList.Add(AllyNearbyParties.ElementAt(i).GetTrackDistanceToMainAgent());
            }
        }

        private async void calculateFirstTime() //calculates the first time of when to check if a party should spawn
        {
            System.Diagnostics.Debug.WriteLine("Inside CalculateFirstTime");

            await populateTroopList();
            if (EnemydistancePartyList.IsEmpty())
                BigSpawnTimer = getDelaySpawnTime(AllydistancePartyList.ElementAt(0));
            else if (AllydistancePartyList.IsEmpty())
                BigSpawnTimer = getDelaySpawnTime(EnemydistancePartyList.ElementAt(0));
            else if (getDelaySpawnTime(EnemydistancePartyList.ElementAt(0)) < getDelaySpawnTime(AllydistancePartyList.ElementAt(0)))
                BigSpawnTimer = getDelaySpawnTime(EnemydistancePartyList.ElementAt(0));
            else
                BigSpawnTimer = getDelaySpawnTime(AllydistancePartyList.ElementAt(0));

            CustomBattleEndLogic newBattleLogic = new CustomBattleEndLogic();
            Mission.Current.AddMissionBehaviour(newBattleLogic); //add our own custom battle end logic
            Mission.Current.RemoveMissionBehaviour(Mission.Current.GetMissionBehaviour<BattleEndLogic>()); //removes Original battleEndlogic
            //Mission.Current.Mission
            System.Diagnostics.Debug.WriteLine("Finished calculating first time with time " + BigSpawnTimer);

            if (EnemyPartyMainMobileParty.IsCaravan) //handles MCM settings for caravans
            {
                if(MCMUISettings.Instance.SettingPlayerCaravanReinforcements == false)
                {
                    AllyNearbyParties.Clear();
                    AllydistancePartyList.Clear();
                }
                if (MCMUISettings.Instance.SettingEnemyCaravanReinforcements == false)
                {
                    EnemyNearbyParties.Clear();
                    EnemydistancePartyList.Clear();
                }
            }


        }

        private async Task populateTroopList()
        {

            while (Mission.Current.PlayerTeam.ActiveAgents == null || Mission.Current.PlayerTeam.ActiveAgents.Count() <= 0)
            {
                await Task.Delay(500);
            }
            await Task.Delay(500);
            for (int i = 0; i < MapEvent.PlayerMapEvent.InvolvedParties.Count(); i++) //gets original parties
            {
                OriginalInvolvedParties.Add(MapEvent.PlayerMapEvent.InvolvedParties.ElementAt(i)); //now create a function that goes through all lists
            }
            //gets original parties
            // initially spawned enemy and allied troops - needed when supplier is recreated
            AllyactiveAgents = Mission.Current.PlayerTeam.ActiveAgents;
            EnemyactiveAgents = Mission.Current.PlayerEnemyTeam.ActiveAgents;

            //list of OG troops and their party
            List<List<Agent>> RosterPartyList = new List<List<Agent>>(); //lists of lists of agents OG spawned agents
            List<MobileParty> RosterNamePartyList = new List<MobileParty>();
            for (int k = 0; k < OriginalInvolvedParties.Count(); k++)
            {
                //List<IAgentOriginBase> OGInvolvedTempTeamRoster = newAgentsOfParty(OriginalInvolvedParties.ElementAt(k), jList); //returns a list of Jlist with OG list only

                Team CurrentTeam = Mission.Current.PlayerEnemyTeam;
                if (OriginalInvolvedParties.ElementAt(k).MapFaction == MobileParty.MainParty.MapFaction)
                    CurrentTeam = Mission.Current.PlayerTeam;
                //get Original parties

                System.Diagnostics.Debug.WriteLine("Populating Troop List");


                List<Agent> saveLives = new List<Agent>();
                for (int y = 0; y < CurrentTeam.ActiveAgents.Count(); y++)
                {
                    PartyGroupAgentOrigin tempAgent = CurrentTeam.ActiveAgents.ElementAt(y).Origin as PartyGroupAgentOrigin;

                    if (tempAgent.Party == OriginalInvolvedParties.ElementAt(k).MobileParty.Party)
                        saveLives.Add(CurrentTeam.ActiveAgents.ElementAt(y));
                }
                RosterPartyList.Add(saveLives);
                RosterNamePartyList.Add(OriginalInvolvedParties.ElementAt(k).MobileParty);

            }

            // add the parties as participants to the battle - create the agent origin
            await addAllParties(); //move this before jlist add all parties to the game as involved need to remove them later
            //created a list of lists of agents to cycle and readd their origin here below

            // create the supplier for enemy parties list
            jList = createPartySupplier(2000, Mission.Current.PlayerEnemyTeam.Side).ToList(); //calling this makes current members null
            // stores the origins for all allies regardless of party - allied list
            AList = createPartySupplier(2000, Mission.Current.PlayerTeam.Side).ToList(); //calling this makes current members null



            // assigns the origin obbject to the agents already on the battlefield (giving their souls back)
            for (int omega = 0; omega < RosterPartyList.Count(); omega++)
            {
                List<Agent> currentListToRestore = RosterPartyList.ElementAt(omega);
                //use this list below for quicker
                List<IAgentOriginBase> ListSide = jList;
                if (RosterNamePartyList.ElementAt(omega).MapFaction == MobileParty.MainParty.MapFaction) //if ally
                {
                    ListSide = AList;
                }


                List<IAgentOriginBase> restoreOGParties = getOGTroopsFromAORJList(RosterNamePartyList.ElementAt(omega).Name, ListSide); // a list of first party to restrore
                for (int R = 0; R < restoreOGParties.Count(); R++)
                {
                    PartyGroupAgentOrigin tempOriginInAJlist = restoreOGParties.ElementAt(R) as PartyGroupAgentOrigin;


                    Agent item = currentListToRestore.FirstOrDefault(o => o.Character.Name == tempOriginInAJlist.Troop.Name);
                    try
                    {
                        item.Origin = restoreOGParties.ElementAt(R);
                        currentListToRestore.Remove(item);
                    }
                    catch (Exception e)
                    {
                        //System.Diagnostics.Debug.WriteLine(e);
                        System.Diagnostics.Debug.WriteLine("R + " + R);
                        //System.Diagnostics.Debug.WriteLine(item.Name);
                    }

                }
            }


            //allocate reinforcements lists
            getOriginalReinforcementsFirst(AllyactiveAgents, AList, true); //ally first
            getOriginalReinforcementsFirst(EnemyactiveAgents, jList, false); //enemy second


            InitialAllyTroopCount = Mission.Current.PlayerTeam.ActiveAgents.Count();
            InitialEnemyTroopCount = Mission.Current.PlayerEnemyTeam.ActiveAgents.Count();
            removeAllParties();

            System.Diagnostics.Debug.WriteLine("Finished Populating Troop List");

        }

        private async Task addAllParties() //needs to wait for this to finish
        {
            System.Diagnostics.Debug.WriteLine("Adding all parties");

            await Task.Delay(1500);

            for (int i = 0; i < EnemyNearbyParties.Count(); i++)
                MapEvent.PlayerMapEvent.AddInvolvedParty(EnemyNearbyParties.ElementAt(i).Party, Mission.Current.PlayerEnemyTeam.Side);

            System.Diagnostics.Debug.WriteLine("Finished Involving all parties");
            for (int i = 0; i < AllyNearbyParties.Count(); i++)
            {
                MapEvent.PlayerMapEvent.AddInvolvedParty(AllyNearbyParties.ElementAt(i).Party, Mission.Current.PlayerTeam.Side);
            }

        }

        private void removeAllParties()
        {
            System.Diagnostics.Debug.WriteLine("Removing non Original parties");
            for (int i = 0; i < MapEvent.PlayerMapEvent.InvolvedParties.Count(); i++)
            {
                if (!OriginalInvolvedParties.Contains(MapEvent.PlayerMapEvent.InvolvedParties.ElementAt(i)))
                {
                    System.Diagnostics.Debug.WriteLine("Removed party " + MapEvent.PlayerMapEvent.InvolvedParties.ElementAt(i).Name.ToString());

                    MapEvent.PlayerMapEvent.RemoveInvolvedParty(MapEvent.PlayerMapEvent.InvolvedParties.ElementAt(i));
                    i = 0;
                }
            }
        }


        /// 
        private static List<IAgentOriginBase> createPartySupplier(int troops, BattleSideEnum side)
        {
            System.Diagnostics.Debug.WriteLine("inside createpartyuispplier");

            //List<Dictionary<CharacterObject, int>> listDiction = new List<Dictionary<CharacterObject, int>>();
            IEnumerable<IAgentOriginBase> timKimKim;
            List<IAgentOriginBase> timKim2 = new List<IAgentOriginBase>();
            //4 parties
            /*for (int i = 0; i < trueNearbyPartties.Count(); i++)
            {
                Dictionary<CharacterObject, int> Tempdiction = new Dictionary<CharacterObject, int>(trueNearbyPartties.ElementAt(i).MemberRoster.Count());
                for (int j = 0; j < trueNearbyPartties.ElementAt(i).MemberRoster.Count(); j++)
                {
                    Tempdiction.Add(trueNearbyPartties.ElementAt(i).MemberRoster.GetCharacterAtIndex(j), 1);//j could mean amount
                }
                listDiction.Add(Tempdiction);
            }*/


            if (MapEvent.PlayerMapEvent.PlayerSide == side) //1 = attacker 0 = defender
            {//Alist
                PartyGroupTroopSupplier partyGroupTroopSupplier = new PartyGroupTroopSupplier(MapEvent.PlayerMapEvent, side);
                timKimKim = partyGroupTroopSupplier.SupplyTroops(troops);
            }
            else
            {//enemy Jlist
                PartyGroupTroopSupplier partyGroupTroopSupplier = new PartyGroupTroopSupplier(MapEvent.PlayerMapEvent, Mission.Current.PlayerEnemyTeam.Side);
                timKimKim = partyGroupTroopSupplier.SupplyTroops(troops);
            }


            for (int i = 0; i < timKimKim.Count(); i++)
            {
                PartyGroupAgentOrigin tempOrigin = timKimKim.ElementAt(i) as PartyGroupAgentOrigin;
                timKim2.Add(tempOrigin);
            }
            System.Diagnostics.Debug.WriteLine("returning partysupplier");

            return timKim2;
            int ho = 1;
        }


        private List<IAgentOriginBase> getOGTroopsFromAORJList(TextObject party, List<IAgentOriginBase> jList)
        {
            List<IAgentOriginBase> newList = new List<IAgentOriginBase>();
            for (int i = 0; i < jList.Count(); i++)
            {
                PartyGroupAgentOrigin tempOrigin = jList.ElementAt(i) as PartyGroupAgentOrigin;
                if (tempOrigin.Party.Name == party)
                    newList.Add(jList.ElementAt(i));
            }
            return newList;
        }


        private static void getOriginalReinforcementsFirst(MBReadOnlyList<Agent> agentList, List<IAgentOriginBase> OriginListAJ, bool playerSide)
        {
            //check if current AJList unit is within involved parties
            System.Diagnostics.Debug.WriteLine("Getting Original Reinforcements"); //

            List<IAgentOriginBase> ALLOGAgents = new List<IAgentOriginBase>();


            for (int i = 0; i < OriginListAJ.Count(); i++)
            {
                PartyGroupAgentOrigin temporigin = OriginListAJ.ElementAt(i) as PartyGroupAgentOrigin;
                if (OriginalInvolvedParties.Contains(temporigin.Party))
                {
                    ALLOGAgents.Add(OriginListAJ.ElementAt(i));
                }
            }
            //create this new AJ list

            for (int j = 0; j < agentList.Count(); j++)
            {
                if (ALLOGAgents.Contains(agentList.ElementAt(j).Origin))
                {
                    ALLOGAgents.Remove(agentList.ElementAt(j).Origin);
                }
            }

            if (playerSide == true)
            {
                OriginalAllyReinforcements = ALLOGAgents;
            }
            else
            {
                OriginalEnemyReinforcements = ALLOGAgents;
            }
            System.Diagnostics.Debug.WriteLine(" Finished Original Reinforcements"); //

            //if this list contains agents within AgentList pop them out of list

        }

        private async void calculateNextParty()
        {
            try
            {
                if (Mission.Current.MissionEnded())
                {
                    return;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Caught exception at calculateNextParty");
                resetMod();
                return;
            }


            if (AllyNearbyParties.IsEmpty() && EnemyNearbyParties.IsEmpty())
            {
                System.Diagnostics.Debug.WriteLine(" Both lists emptty for calculateParty"); //

                BigSpawnTimer = 9999999999;
                return;
            }




            //check both list see which one is closest
            int currentLocalCycle = 0;
            int StopRepeating = 0;

            while (!EnemyNearbyParties.IsEmpty() || !AllyNearbyParties.IsEmpty())
            {

                int BreakCount = 0;
                StopRepeating = 0;
                //enemy
                if (currentLocalCycle < EnemyNearbyParties.Count() && Mission.Current.Time >= getDelaySpawnTime(EnemydistancePartyList.ElementAt(currentLocalCycle)))
                {

                    if (enoughMen(currentLocalCycle, EnemyNearbyParties, Mission.Current.PlayerEnemyTeam))
                    {
                        System.Diagnostics.Debug.WriteLine("spawning enemy " + EnemyNearbyParties.ElementAt(0).Name.ToString()); // not current but element 0
                        await Task.Delay(3009);
                        await CalculatingMisc(EnemyNearbyParties, Mission.Current.PlayerEnemyTeam, EnemydistancePartyList);
                    }
                    else
                    {
                        BreakCount++;
                        StopRepeating++;
                    }

                }
                else // and not empty?
                {
                    BreakCount++;
                }

                //ally
                if (currentLocalCycle < AllyNearbyParties.Count() && Mission.Current.Time >= getDelaySpawnTime(AllydistancePartyList.ElementAt(currentLocalCycle)))
                {

                    if (enoughMen(currentLocalCycle, AllyNearbyParties, Mission.Current.PlayerTeam))
                    {
                        System.Diagnostics.Debug.WriteLine("spawning ally " + AllyNearbyParties.ElementAt(0).Name.ToString());
                        await Task.Delay(3009);
                        await CalculatingMisc(AllyNearbyParties, Mission.Current.PlayerTeam, AllydistancePartyList);
                    }
                    else
                    {
                        BreakCount++;
                        StopRepeating++;
                    }

                }
                else
                {
                    BreakCount++;
                }



                if (BreakCount == 2) //if both list can't spawn then just break
                    break;
                currentLocalCycle++;


            }

            calculateNextPartyTime(); //allows it to check every so often



            calculateNextPartyLock = false; //unlock calculate
            System.Diagnostics.Debug.WriteLine("END CalculateNextParty");


        }

        private static bool enoughMen(int currentPartyInList, List<MobileParty> whichSide, Team thisteam) //if the current reinforcements arent enough to help the army then
        {
            Team OppositeTeam;
            int inQueue = 0;
            if (thisteam == Mission.Current.PlayerTeam)
            {
                OppositeTeam = Mission.Current.PlayerEnemyTeam;
                inQueue = AlliesInQueue.Count();
            }
            else
            {
                OppositeTeam = Mission.Current.PlayerTeam;
                inQueue = EnemiesInQueue.Count();
            }

            float ratio = MCMUISettings.Instance.SettingEnemyJoinRatio;
            if (thisteam == Mission.Current.PlayerTeam)
            {
                //ratio = 0.8f;
                ratio = MCMUISettings.Instance.SettingPlayerJoinRatio;
                System.Diagnostics.Debug.WriteLine("ratio " + ratio);

            }


            int currentActiveAgents = thisteam.ActiveAgents.Count();
            int totalNewMen = 0;
            for (int i = 0; i <= currentPartyInList; i++)
            {
                totalNewMen += whichSide.ElementAt(i).Party.NumberOfHealthyMembers;
            }
            if (OppositeTeam.ActiveAgents.Count() * ratio < currentActiveAgents + totalNewMen + inQueue)
            {
                System.Diagnostics.Debug.WriteLine("enoguh men is true " + (currentActiveAgents + totalNewMen + inQueue));
                return true;
            }
            System.Diagnostics.Debug.WriteLine("enoguh men is false " + (currentActiveAgents + totalNewMen + inQueue));
            return false;
        }

        private async Task CalculatingMisc(List<MobileParty> list, Team team, List<float> distanceList)
        {

            try
            {
                if (Mission.Current.MissionEnded())
                {
                    return;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Caught exception at calculateNextParty");
                resetMod();
                return;
            }
            standYourGround(team.Side);
            System.Diagnostics.Debug.WriteLine("Calcualting misk for " + list.ElementAt(0).Name.ToString());
            modActive = true;
            changeAmountMenAllowed();
            MapEvent.PlayerMapEvent.AddInvolvedParty(list.ElementAt(0).Party, team.Side);
            moveEnemyPartiesRegardless(0, MapEvent.PlayerMapEvent.GetLeaderParty(team.Side).Position2D, list); //move AI party to their friendly neighbor position
            MobileParty newMobileParty = new MobileParty();
            newMobileParty = list.ElementAt(0);
            list.RemoveAt(0);
            distanceList.RemoveAt(0);
            await spawnTroopsBeta(newMobileParty, team);
            fixTime();
            //add involved
            //teleport party
        }

        private void changeAmountMenAllowed()
        {
            while ((InitialAllyTroopCount + InitialEnemyTroopCount) < BannerlordConfig.BattleSize)
            {
                InitialEnemyTroopCount += 10;
                InitialAllyTroopCount += 10;
            }
        }

        private static void moveEnemyPartiesRegardless(int partyInList, Vec2 partylocation, List<MobileParty> list) //moves a party to the player map
        {

            System.Diagnostics.Debug.WriteLine("Added involved party and moved them " + list.ElementAt(partyInList).Name.ToString());
            MobileParty curretParty = list.ElementAt(partyInList); //moves the whole army

            if (curretParty.CurrentSettlement != null) //cant get this to work move a party out of settlement
            {
                /*list.ElementAt(partyInList).CurrentSettlement.RemoveParty(curretParty);
                list.ElementAt(partyInList).CurrentSettlement = null;
                
                list.ElementAt(partyInList).Position2D = partylocation;*/
                System.Diagnostics.Debug.WriteLine("This party is in Garrison " + list.ElementAt(partyInList).Name.ToString());

            }
            if (curretParty.Army != null)
            {
                //find army leader checks to see if the army lead is in the vicinity
                if(curretParty.Army.LeaderParty.MapEvent == null && curretParty.Army.LeaderParty.GetTrackDistanceToMainAgent() <= MobileParty.MainParty.SeeingRange)
                    curretParty.Army.LeaderParty.Position2D = partylocation; //moves leader first causes problem if they hvnt joined the leader yet
                for (int i = 0; i < list.Count(); i++)
                {
                    if (list.ElementAt(i).Army == curretParty.Army)
                    {
                        list.ElementAt(i).Party.MobileParty.Position2D = partylocation;

                    }
                }
            }
            else
                list.ElementAt(partyInList).Party.MobileParty.Position2D = partylocation;

            

        }

        private Task spawnTroopsBeta(MobileParty currentParty, Team team)
        {
            InformationManager.AddQuickInformation(new TextObject(currentParty.Name.ToString() + " has joined the battle!"));
            Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/ui/mission/horns/attack"), Mission.Current.PlayerEnemyTeam.ActiveAgents.GetRandomElement().Position, true, true, 0, 0);
            List<IAgentOriginBase> currentPartyRoster = new List<IAgentOriginBase>();

            if (team == Mission.Current.PlayerTeam)
            {
                currentPartyRoster = newAgentsOfParty(currentParty.Party, AList); // ally list
                for (int i = 0; i < currentPartyRoster.Count(); i++)
                {
                    AlliesInQueue.Add(currentPartyRoster.ElementAt(i));
                }
            }

            else
            {
                currentPartyRoster = newAgentsOfParty(currentParty.Party, jList); // enemy add
                for (int i = 0; i < currentPartyRoster.Count(); i++)
                {
                    EnemiesInQueue.Add(currentPartyRoster.ElementAt(i));
                }
            }


            for (int i = 0; i < currentInvolvedParties.Count(); i++) //feature I need to add later
            {

                partiesInFight.Add(currentInvolvedParties.ElementAt(i).Name);
            }


            //MBReadOnlyList<Agent> hello2 = Mission.Current.PlayerEnemyTeam.ActiveAgents;

            /*for (int i = 0; i < currentPartyRoster.Count(); i++) //gets all nearby parties
            {
                while(troopLimit-2 < team.ActiveAgents.Count())
                {
                    await nightNight(1500);
                }
                if (gameFinished == true)
                    return;
                
                //partiesInFight.Add(trueNearbyPartties.ElementAt(i).Party.Name);
                try
                {

                        *//*PartyGroupAgentOrigin partyGroupAgentOrigin =
                                (PartyGroupAgentOrigin)typeof(PartyGroupAgentOrigin).GetConstructor(BindingFlags.NonPublic |
                                BindingFlags.Instance, null, new Type[] { typeof(PartyGroupTroopSupplier), typeof(UniqueTroopDescriptor),
                                    typeof(int) }, null).Invoke(new object[] { listPGTS.ElementAt(i), new UniqueTroopDescriptor(27492338), j });*//*
                        //IAgentOriginBase timKimspawn = createPartySupplier().ElementAt(i);
                        //timKimKim = listPGTS.ElementAt(i).SupplyTroops(30);
                        int timho = 1;
                        Agent GTSFAgent = Mission.Current.SpawnTroop(currentPartyRoster.ElementAt(i), playerSide, true,
                        true, true, false, 0, 1, true, true, true);
                        //Agent currentAgent = Mission.Current.SpawnAgent(new AgentBuildData(jList.ElementAt(j)));
                        //currentAgent.SetTeam(Mission.Current.PlayerEnemyTeam, true);
                        //MapEvent.PlayerMapEvent.DefenderSide.RecalculateMemberCountOfSide();

                        *//*Agent currentAgent = Mission.Current.SpawnTroop(createPartySupplier().ElementAt(i), playerSide, true,
                        false, true, false, 0, 1, true, true, true);*//*
                        System.Diagnostics.Debug.WriteLine("It worked! " + i);

                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Caught exception failed at " + i);
                    System.Diagnostics.Debug.WriteLine(e);

                }



                if (i == 0)
                {
                }
                MBReadOnlyList<Agent> hello = Mission.Current.PlayerEnemyTeam.ActiveAgents;

                System.Diagnostics.Debug.WriteLine("finished spawning " + i + " Agent");


            }*/

            System.Diagnostics.Debug.WriteLine("Closing Spawning Troops");
            return Task.CompletedTask;

        }

        private List<IAgentOriginBase> newAgentsOfParty(PartyBase party, List<IAgentOriginBase> list)
        {
            List<IAgentOriginBase> newList = new List<IAgentOriginBase>();
            for (int i = 0; i < list.Count(); i++)
            {
                PartyGroupAgentOrigin tempVar = list.ElementAt(i) as PartyGroupAgentOrigin;
                if (tempVar.Party == party)
                {
                    newList.Add(list.ElementAt(i));
                }
            }
            return newList;
        }

        private bool calculateNextPartyTime()
        {
            //returns true if next available time and false if no more times
            float currentBigSpawnTimer = BigSpawnTimer;
            float tempFloat = 99999999.9f;
            float newBigSpawnTimer = tempFloat;

            for (int i = 0; i < AllydistancePartyList.Count(); i++)
            {
                if (getDelaySpawnTime(AllydistancePartyList.ElementAt(i)) > currentBigSpawnTimer)
                {
                    newBigSpawnTimer = getDelaySpawnTime(AllydistancePartyList.ElementAt(i));
                    break;
                }
            }
            for (int i = 0; i < EnemydistancePartyList.Count(); i++)
            {
                if (getDelaySpawnTime(EnemydistancePartyList.ElementAt(i)) > currentBigSpawnTimer && getDelaySpawnTime(EnemydistancePartyList.ElementAt(i)) < newBigSpawnTimer)
                {
                    newBigSpawnTimer = getDelaySpawnTime(EnemydistancePartyList.ElementAt(i));
                    break;
                }
            }

            //System.Diagnostics.Debug.WriteLine("New Big Spawn Timer " + BigSpawnTimer);

            if (currentBigSpawnTimer < newBigSpawnTimer && newBigSpawnTimer != tempFloat)
            {
                BigSpawnTimer = newBigSpawnTimer;
                System.Diagnostics.Debug.WriteLine("New Big Spawn Timer " + BigSpawnTimer);
                return true;
            }
            else if (!RemainingListsHaveSameTime())
            {
                BigSpawnTimer += 30.0f;
                System.Diagnostics.Debug.WriteLine("New Big Spawn Timer +30");
                return false;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("No change big spwan timer");
                return false;
            }
        }

        private bool RemainingListsHaveSameTime() //checks to see if it might have the same time stamps
        {
            int count = 0;
            for (int i = 0; i < AllydistancePartyList.Count(); i++)
            {
                if (getDelaySpawnTime(AllydistancePartyList.ElementAt(i)) == BigSpawnTimer)
                {
                    count++;
                }
            }
            for (int i = 0; i < EnemydistancePartyList.Count(); i++)
            {
                if (getDelaySpawnTime(EnemydistancePartyList.ElementAt(i)) == BigSpawnTimer)
                {
                    count++;
                }
            }
            if (count >= 2)
                return true;
            return false;
        }

        private void spawnOriginalReinforcements(List<IAgentOriginBase> OGReinforcementList, bool playerSide)
        {
            int size = OGReinforcementList.Count();
            if (size >= 10)
                size = 10;
            for (int i = 0; i < size; i++)
            {
                IAgentOriginBase rand = OGReinforcementList.GetRandomElement();

                Agent GTSFAgent = Mission.Current.SpawnTroop(rand, playerSide, true,
                            rand.Troop.HasMount(), true, false, rand.Troop.DefaultFormationGroup, 1, true, true);
                OGReinforcementList.Remove(rand);
            }
        }

        /// This function actually adds the characters to the battle based on what is passed in the queue list
        private async void spawnNewReinforcements(List<IAgentOriginBase> QueueList, bool playerSide, Team team)
        {
            int troopLimit;
            if (!playerSide)
            {

                troopLimit = InitialEnemyTroopCount;
            }
            else
            {
                troopLimit = InitialAllyTroopCount;
            }


            for (int i = 0; i < QueueList.Count(); i++)
            {
                while (troopLimit - 2 < team.ActiveAgents.Count())
                {
                    await nightNight(1500);
                }
                // helps delay the spawn time 
                if (i % 3 == 0)
                {
                    await nightNight(400);
                }

                try
                {
                    IAgentOriginBase rand = QueueList.GetRandomElement();
                    PartyAgentOrigin tempOrigin = rand as PartyAgentOrigin;

                    Agent GTSFAgent = Mission.Current.SpawnTroop(rand, playerSide, true,
                            rand.Troop.HasMount(), true, false, 1, rand.Troop.DefaultFormationGroup, true, true);
                    QueueList.Remove(rand);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Caught exception within spawn new reinforcements");
                    resetMod();
                    return;
                }

            }
            // these locks prevent multiple instances of spawn from running for either side
            if (playerSide == true)
            {
                lockAllySpawnNewReinforcements = false;
            }
            else
            {
                lockEnemySpawnNewReinforcements = false;
            }
        }

        private static async Task<int> nightNight(int time)//time
        {
            //await Task.Delay(time); //delay time
            for (int i = 0; i < 3; i++)
            {

                await Task.Delay(time / 3);

            }
            return 1;
        }

        async Task TaskDelay(int delayTime)
        {
            await Task.Delay(delayTime);
        }


        private static float getDelaySpawnTime(float distance)//returns minute time in seconds example 56 seconds
        {

            //return (((distance / 2.3f) * 90000.0f) / 1000.0f) / 1.9f;
            return distance * (MCMUISettings.Instance.SettingTimeToSpawnRatio*10);
        }
        private void makeEnemyAgentUnsafe()
        {
            System.Diagnostics.Debug.WriteLine("makeunsafe enemy");
            holdEnemyAgent.SetInvulnerable(false);
            holdEnemyAgent.SetIsAIPaused(false);
            holdEnemyAgent.Controller = (Agent.ControllerType)1;
            holdEnemyAgent.Retreat();
        }


        private void makeEnemyAgentSafe()
        {


            holdEnemyAgent = Mission.Current.PlayerEnemyTeam.ActiveAgents.GetRandomElement();
            holdEnemyAgent.SetInvulnerable(true);
            holdEnemyAgent.SetIsAIPaused(true);
            holdEnemyAgent.Controller = 0;
            Vec3 tempVec = new Vec3(-300, -300, 50);
            holdEnemyAgent.TeleportToPosition(tempVec);
        }

        private void makePlayerAgentUnsafe()
        {
            System.Diagnostics.Debug.WriteLine("makeunsafe player");
            holdPlayerAgent.SetInvulnerable(false);
            holdPlayerAgent.SetIsAIPaused(false);
            holdPlayerAgent.Controller = (Agent.ControllerType)1;
            holdPlayerAgent.Retreat();
        }


        private void makePlayerAgentSafe()
        {
            holdPlayerAgent = Mission.Current.PlayerTeam.ActiveAgents.GetRandomElement();
            while (holdPlayerAgent == Mission.Current.MainAgent)
            {
                holdPlayerAgent = Mission.Current.PlayerTeam.ActiveAgents.GetRandomElement();
            }
            holdPlayerAgent.SetInvulnerable(true);
            holdPlayerAgent.SetIsAIPaused(true);
            holdPlayerAgent.Controller = 0;
            Vec3 tempVec = new Vec3(-300, -300, 50);
            holdPlayerAgent.TeleportToPosition(tempVec);
        }



        private static void tryMissionAgentSpawnLogic()
        {
            MissionLogic currentLogic = Mission.Current.MissionLogics.ElementAt(10);
            MissionAgentSpawnLogic currentSpawnLogic = currentLogic as MissionAgentSpawnLogic;
            currentSpawnLogic.CheckReinforcement(100);
            int k = currentSpawnLogic.NumberOfActiveAttackerTroops;
            int l = currentSpawnLogic.NumberOfActiveDefenderTroops;
            currentSpawnLogic.IsSideDepleted(Mission.Current.PlayerTeam.Side);
            currentSpawnLogic.CheckReinforcement(100);




            BattleEndLogic newEL = new BattleEndLogic();
            //ewEL.
            //currentSpawnLogic.


            MissionLogic End = Mission.Current.MissionLogics.ElementAt(5);
            BattleEndLogic end2 = End as BattleEndLogic;
            //end2.


            //MissionAgentSpawnLogic newMASL = new MissionAgentSpawnLogic(newSuplier, Mission.Current.PlayerTeam.Side);
            /*Mission.Current.AddMissionBehaviour(newMASL);
            newMASL.InitWithSinglePhase(10,10,10,10,true,true);
            newMASL.SetSpawnTroops(Mission.Current.PlayerEnemyTeam.Side, true);*/
            //testing = true;
            int h0 = 2;
            //newMASL.
        }


        private bool withinPartiesFighting(TextObject party)
        {
            for (int i = 0; i < MapEvent.PlayerMapEvent.InvolvedParties.Count(); i++)
            {
                if (party == MapEvent.PlayerMapEvent.InvolvedParties.ElementAt(i).Name)
                    return true;
            }
            return false;
        }



        private static void fixTime()
        {
            try
            {
                Mission.Current.NextCheckTimeEndMission = 10.0f;

            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Caught exception fix time");
                return;
            }

        }

        // START HERE - declare war
        /**
        Get the armies that are potential candidates for joining the battle; adds them to two distinct lists
        
        */

        private static void standYourGround(BattleSideEnum side)
        {
            System.Diagnostics.Debug.WriteLine("Stand your ground");
            if (Mission.Current.PlayerTeam.Side == side)
            {
                int sizePlayerTeam = Mission.Current.PlayerTeam.ActiveAgents.Count();

                for (int i = 0; i < sizePlayerTeam; i++)
                {
                    if (Mission.Current.PlayerTeam.ActiveAgents.ElementAt(i).IsRetreating())
                    {
                        Mission.Current.PlayerTeam.ActiveAgents.ElementAt(i).SetMorale(99999);
                        Mission.Current.PlayerTeam.ActiveAgents.ElementAt(i).StopRetreatingMoraleComponent();
                        Mission.Current.PlayerTeam.MasterOrderController.SetOrderWithAgent((OrderType)4, Mission.Current.PlayerTeam.ActiveAgents.ElementAt(i));
                    }
                }
            }
            else
            {
                int sizeEnemyTeam = Mission.Current.PlayerEnemyTeam.ActiveAgents.Count();
                for (int i = 0; i < sizeEnemyTeam; i++)
                {
                    if (Mission.Current.PlayerEnemyTeam.ActiveAgents.ElementAt(i).IsRetreating())
                    {
                        Mission.Current.PlayerEnemyTeam.ActiveAgents.ElementAt(i).SetMorale(99999);
                        Mission.Current.PlayerEnemyTeam.ActiveAgents.ElementAt(i).StopRetreatingMoraleComponent();
                        Mission.Current.PlayerEnemyTeam.MasterOrderController.SetOrderWithAgent((OrderType)4, Mission.Current.PlayerEnemyTeam.ActiveAgents.ElementAt(i));
                        //Mission.Current.PlayerEnemyTeam.ActiveAgents.ElementAt(i).GetMovementDirection(Mission.Current.Play))
                    }
                }
            }
        }

        private static void retreatingBug() //theres a bug that enemies retreat forever
        {

            if (Mission.Current.PlayerTeam != null)
            {
                for (int i = 0; i < Mission.Current.PlayerTeam.ActiveAgents.Count(); i++)
                {
                    if (!Mission.Current.IsPositionInsideBoundaries(Mission.Current.PlayerTeam.ActiveAgents.ElementAt(i).Position.AsVec2) && Mission.Current.PlayerTeam.ActiveAgents.ElementAt(i) != holdPlayerAgent)
                    {
                        Mission.Current.PlayerTeam.ActiveAgents.ElementAt(i).TeleportToPosition(Mission.Current.GetClosestBoundaryPosition(Mission.Current.PlayerTeam.ActiveAgents.ElementAt(i).Position.AsVec2).ToVec3());
                        //Mission.Current.PlayerEnemyTeam.MasterOrderController.SetOrderWithAgent((OrderType)4, Mission.Current.PlayerEnemyTeam.ActiveAgents.ElementAt(i));

                        System.Diagnostics.Debug.WriteLine("Retreated Friendly Agent");
                    }
                }
            }
            else { System.Diagnostics.Debug.WriteLine("PlayerTeam is null"); }


            if (Mission.Current.PlayerEnemyTeam != null)
            {
                for (int i = 0; i < Mission.Current.PlayerEnemyTeam.ActiveAgents.Count(); i++)
                {
                    if (!Mission.Current.IsPositionInsideBoundaries(Mission.Current.PlayerEnemyTeam.ActiveAgents.ElementAt(i).Position.AsVec2) && Mission.Current.PlayerEnemyTeam.ActiveAgents.ElementAt(i) != holdEnemyAgent)
                    {
                        Mission.Current.PlayerEnemyTeam.ActiveAgents.ElementAt(i).TeleportToPosition(Mission.Current.GetClosestBoundaryPosition(Mission.Current.PlayerEnemyTeam.ActiveAgents.ElementAt(i).Position.AsVec2).ToVec3());
                        //Mission.Current.PlayerEnemyTeam.MasterOrderController.SetOrderWithAgent((OrderType)4, Mission.Current.PlayerEnemyTeam.ActiveAgents.ElementAt(i));
                        System.Diagnostics.Debug.WriteLine("Retreated Enemy Agent");
                    }
                }
            }
            else { System.Diagnostics.Debug.WriteLine("PlayerEnemyTeam is null"); }
            //System.Diagnostics.Debug.WriteLine("end retrreatingBug");
        }


        private static void resetMod()
        {
            BigSpawnTimer = 999999999.0f;
            if (!EnemyNearbyParties.IsEmpty())
                EnemyNearbyParties.Clear();
            if (!EnemydistancePartyList.IsEmpty())
                EnemydistancePartyList.Clear();

            if (!AllyNearbyParties.IsEmpty())
                AllyNearbyParties.Clear();
            if (!AllydistancePartyList.IsEmpty())
                AllydistancePartyList.Clear();
            if (!EnemydistancePartyList.IsEmpty())
                EnemydistancePartyList.Clear();
            if (!AllydistancePartyList.IsEmpty())
                AllydistancePartyList.Clear();
            if (!OriginalInvolvedParties.IsEmpty())
                OriginalInvolvedParties.Clear();

            if (!jList.IsEmpty())
                jList.Clear();
            if (!AList.IsEmpty())
                AList.Clear();

            if (!EnemiesInQueue.IsEmpty())
                EnemiesInQueue.Clear();
            if (!AlliesInQueue.IsEmpty())
                AlliesInQueue.Clear();

            lockEnemySpawnNewReinforcements = false;
            lockAllySpawnNewReinforcements = false;
            calculateNextPartyLock = false;

            if (!OriginalAllyReinforcements.IsEmpty())
                OriginalAllyReinforcements.Clear();
            if (!OriginalEnemyReinforcements.IsEmpty())
                OriginalEnemyReinforcements.Clear();

            InitialEnemyTroopCount = 0;
            InitialAllyTroopCount = 0;
            EnemyPartyMainMobileParty = null; //to check if caravan or villager

            holdEnemyAgent = null;
            holdPlayerAgent = null;
            lordParty = false;
            modActive = false;
            gameFinished = false;
            System.Diagnostics.Debug.WriteLine("Clear list");

        }
        /*private void BeforeMissionOpenedL()
        {
            InformationManager.DisplayMessage(new InformationMessage("before Mission Opened method"));

        }

        private void setup()
        {
            InformationManager.DisplayMessage(new InformationMessage("SetupPreConvEvent"));

        }*/



        public override void SyncData(IDataStore dataStore)
        {
        }
    }






}

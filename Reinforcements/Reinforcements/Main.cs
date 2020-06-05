using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Engine;
using NetworkMessages.FromServer;
using TaleWorlds.Network;
using JetBrains.Annotations;
//using MyQueue;
using System.Net;
using TaleWorlds.Library;
using TaleWorlds.CampaignSystem.SandBox;
using TaleWorlds.ObjectSystem;
using TaleWorlds.MountAndBlade.MissionSpawnHandlers;
using TaleWorlds.MountAndBlade.Source.Missions;
using System.Collections;
using System.Xml;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers;
using TaleWorlds.CampaignSystem.LogEntries;
using System.Runtime.InteropServices;
using System.Reflection;

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
            //InformationManager.DisplayMessage(new InformationMessage("Inside OnGameStart v2.2")); //initialize mod
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



    static class Extensions
    {
        public static List<T> ICLone<T>(this IList<T> listToClone) where T : ICloneable
        {
            return listToClone.Select(item => (T)item.Clone()).ToList();
        }
    }

    public class campainSystems : CampaignBehaviorBase
    {
        //non original reinforcement variables
        static List<MobileParty> AllyNearbyParties = new List<MobileParty>();
        static List<MobileParty> EnemyNearbyParties = new List<MobileParty>();
        static List<float> EnemydistancePartyList = new List<float>();
        static List<float> AllydistancePartyList = new List<float>();
        static bool SpawnLock = false;
        static List<PartyBase> OriginalInvolvedParties = new List<PartyBase>();
        static MBReadOnlyList<Agent> EnemyactiveAgents;
        static MBReadOnlyList<Agent> AllyactiveAgents;
        static List<IAgentOriginBase> jList = new List<IAgentOriginBase>();
        static List<IAgentOriginBase> AList = new List<IAgentOriginBase>(); //ally troops

        static List<IAgentOriginBase> EnemiesInQueue = new List<IAgentOriginBase>();
        static List<IAgentOriginBase> AlliesInQueue = new List<IAgentOriginBase>();
        bool lockEnemySpawnNewReinforcements = false;
        bool lockAllySpawnNewReinforcements = false;
        bool calculateNextPartyLock = false;


        //original reinforcement variables
        static List<IAgentOriginBase> OriginalAllyReinforcements = new List<IAgentOriginBase>(); //ally and enemy org reinforcements
        static List<IAgentOriginBase> OriginalEnemyReinforcements = new List<IAgentOriginBase>();
        static int InitialEnemyTroopCount = new int();
        static int InitialAllyTroopCount = new int();


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


                if (MapEvent.PlayerMapEvent.IsFieldBattle == true)
                {
                    if (String.IsNullOrEmpty(MapEvent.PlayerMapEvent.GetName().ToString()))
                        throw new Exception("name parameter must contain a value!");

                    System.Diagnostics.Debug.WriteLine("Mod attempted to start");

                    if (MapEvent.PlayerMapEvent.PlayerSide == (BattleSideEnum)1) //1 = attacker 0 = defender
                    {
                        storeNearbyArmiesCS(MapEvent.PlayerMapEvent.GetLeaderParty((BattleSideEnum)0)); //store enemy side initial party
                    }
                    else
                        storeNearbyArmiesCS(MapEvent.PlayerMapEvent.GetLeaderParty((BattleSideEnum)1)); //store enemy side

                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Not field battle");

                }


            }));

/// Every tick this code runs
            //MapEvent.PlayerMapEvent.HasWinner
            CampaignEvents.MissionTickEvent.AddNonSerializedListener(this, new Action<float>(one => { //mission tick

                /*if (modActive == false)
                    return;*/
                if (Mission.Current.Time > 10.0f)
                {
                    // respawns units based on if the count is < 10 of original & current logic maintains the original troop proportions
                    if (InitialAllyTroopCount - 9 > Mission.Current.PlayerTeam.ActiveAgents.Count() && !OriginalAllyReinforcements.IsEmpty())
                        spawnOriginalReinforcements(OriginalAllyReinforcements, true);
                    if (InitialEnemyTroopCount - 9 > Mission.Current.PlayerEnemyTeam.ActiveAgents.Count() && !OriginalEnemyReinforcements.IsEmpty())
                        spawnOriginalReinforcements(OriginalEnemyReinforcements, false);
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

                if (Mission.Current.Time >= BigSpawnTimer && calculateNextPartyLock == false)
                {
                    calculateNextPartyLock = true;
                    calculateNextParty();
                }
                //InformationManager.DisplayMessage(new InformationMessage(Mission.Current.Time.ToString())); //inside the mission use this to wait to spawn


                //System.Diagnostics.Debug.WriteLine((Mission.Current.PlayerTeam.ActiveAgents.Count() + Mission.Current.PlayerEnemyTeam.ActiveAgents.Count()).ToString() + " " + BannerlordConfig.BattleSize.ToString());
                
            }));



            CampaignEvents.OnMissionEndedEvent.AddNonSerializedListener(this, new Action<IMission>(one =>
            {
                if(modActive == true)
                    resetMod();

            }));

            //CampaignEvents.SetupPreConversationEvent.AddNonSerializedListener(this, new Action(this.setup)); //before convo works on looters



            // checks if mission neds to happen pt 2
            CampaignEvents.MissionStarted.AddNonSerializedListener(this, new Action<IMission>((one) =>
            {
                //InformationManager.DisplayMessage(new InformationMessage("Mission started inside campaignSystem")); //inside the mission use this to wait to spawn
                if (lordParty == true && (!EnemyNearbyParties.IsEmpty()) || !AllyNearbyParties.IsEmpty())
                {
                    InformationManager.DisplayMessage(new InformationMessage("You hear the echo of " + (EnemyNearbyParties.Count()+AllyNearbyParties.Count()) + " parties in the distance!")); //debug
                    System.Diagnostics.Debug.WriteLine("Calculating first bigTimer");
                    //
                    calculateFirstTime();
                    //
                }
                else
                    System.Diagnostics.Debug.WriteLine("Mission started Empty party list exiting or not lord party");

            }));
        }


        public async void storeNearbyArmiesCS(PartyBase enemyParty)
        {

            if (!enemyParty.MobileParty.IsLordParty) //exempts looters and other minor factions that aren't lord parties
            {
                System.Diagnostics.Debug.WriteLine("Not lord party exiting storenearbyArmies");
                return;
            }
            resetMod();
            lordParty = true;
            System.Diagnostics.Debug.WriteLine("Storing neraby Armies");


            await TaskDelay(1500); //give time for involved parties
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
                return;
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
                        System.Diagnostics.Debug.WriteLine(temp.MapEvent + " mapevent");

                        EnemyNearbyParties.Add(temp);
                    }

                }
                else if (temp.MapFaction == MobileParty.MainParty.MapFaction && IsWithinInvoledParties(currentInvolvedParties, temp) == false)
                {
                    System.Diagnostics.Debug.WriteLine(temp.Name.ToString() + " " + temp.GetTrackDistanceToMainAgent());
                    System.Diagnostics.Debug.WriteLine(temp.MapEvent + " mapevent");
                    if (temp.IsLordParty && temp.MapEvent == null)
                    {
                        AllyNearbyParties.Add(temp);
                    }
                }
            }

            sortArmyListByDistance();
            System.Diagnostics.Debug.WriteLine("End storing parties " + EnemyNearbyParties.Count() + " enemy parties available");
            System.Diagnostics.Debug.WriteLine("End storing parties " + AllyNearbyParties.Count() + " ally parties available");



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

        private static void sortArmyListByDistance()
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


            System.Diagnostics.Debug.WriteLine("Finished calculating first time with time " + BigSpawnTimer);


        }

        private async Task populateTroopList()
        {
            await Task.Delay(3000);
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
                        System.Diagnostics.Debug.WriteLine(e);
                        System.Diagnostics.Debug.WriteLine(item);
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

            System.Diagnostics.Debug.WriteLine("Finished Populating Troop List");

        }

        private async Task addAllParties() //needs to wait for this to finish
        {
            System.Diagnostics.Debug.WriteLine("Adding all parties");

            await Task.Delay(1000);

            for (int i = 0; i < EnemyNearbyParties.Count(); i++)
                MapEvent.PlayerMapEvent.AddInvolvedParty(EnemyNearbyParties.ElementAt(i).Party, Mission.Current.PlayerEnemyTeam.Side);
            /*for (int i = 0; i < AllyNearbyParties.Count(); i++)
                MapEvent.PlayerMapEvent.AddInvolvedParty(AllyNearbyParties.ElementAt(i).Party, Mission.Current.PlayerEnemyTeam.Side);*/
            System.Diagnostics.Debug.WriteLine("Finished Involving all parties");
            for (int i = 0; i < AllyNearbyParties.Count(); i++)
            {
                MapEvent.PlayerMapEvent.AddInvolvedParty(AllyNearbyParties.ElementAt(i).Party, Mission.Current.PlayerTeam.Side);
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
                modActive = false;
                return;
            }


            if (AllyNearbyParties.IsEmpty() && EnemyNearbyParties.IsEmpty())
            {
                System.Diagnostics.Debug.WriteLine(" Both lists emptty for calculateParty"); //

                BigSpawnTimer = 9999999999;
                return;
            }


            if (AllyNearbyParties.IsEmpty()) //allies is empty so try to spawn enemies
            {
                System.Diagnostics.Debug.WriteLine("Spawn Enemy ally list empty");
                int currentLocalCycle = 0;
                while (true)
                {
                    if (currentLocalCycle >= EnemyNearbyParties.Count())
                    {
                        InformationManager.DisplayMessage(new InformationMessage("Enemy parties have decided not to intervene!"));

                        //change timer
                        EnemyNearbyParties.RemoveRange(0, EnemyNearbyParties.Count());
                        break;
                    }
                    if (enoughMen(currentLocalCycle, EnemyNearbyParties, Mission.Current.PlayerEnemyTeam) && Mission.Current.Time >= BigSpawnTimer)
                    {
                        System.Diagnostics.Debug.WriteLine("enough men and timer for enemu in ally list empty");
                        await CalculatingMisc(EnemyNearbyParties, Mission.Current.PlayerEnemyTeam);
                        break;
                    }
                    else if (enoughMen(currentLocalCycle, EnemyNearbyParties, Mission.Current.PlayerEnemyTeam))
                    {
                        System.Diagnostics.Debug.WriteLine("Doing next cycile " + currentLocalCycle);
                        InformationManager.DisplayMessage(new InformationMessage("Waiting for more reinforcements!"));
                        BigSpawnTimer = getDelaySpawnTime(EnemydistancePartyList.ElementAt(currentLocalCycle));
                    }

                    currentLocalCycle++;
                }

            }
            else if (EnemyNearbyParties.IsEmpty()) //enemies empty so try to spawn allies
            {
                System.Diagnostics.Debug.WriteLine("Spawn Ally enemy list empty");
                int currentLocalCycle = 0;
                while (true)
                {
                    if (currentLocalCycle >= AllyNearbyParties.Count())
                    {
                        InformationManager.DisplayMessage(new InformationMessage("Ally parties have decided not to intervene!"));

                        //change timer
                        //remove
                        AllyNearbyParties.RemoveRange(0, AllyNearbyParties.Count());
                        break;
                    }
                    if (enoughMen(currentLocalCycle, AllyNearbyParties, Mission.Current.PlayerTeam) && Mission.Current.Time >= BigSpawnTimer)
                    {
                        System.Diagnostics.Debug.WriteLine("enough men and timer for ally in enemy list empty");
                        await CalculatingMisc(AllyNearbyParties, Mission.Current.PlayerTeam);
                        break;
                    }
                    else if (enoughMen(currentLocalCycle, AllyNearbyParties, Mission.Current.PlayerTeam))
                    {
                        System.Diagnostics.Debug.WriteLine("Doing next cycile " + currentLocalCycle);
                        InformationManager.DisplayMessage(new InformationMessage("Waiting for more reinforcements!"));
                        BigSpawnTimer = getDelaySpawnTime(AllydistancePartyList.ElementAt(currentLocalCycle));
                    }

                    currentLocalCycle++;
                }
            }
            else //decide who to spawn
            {
                //check both list see which one is closest
                int currentLocalCycle = 0;
                while (true)
                {
                    /*if(currentLocalCycle > AllyNearbyParties.Count())
                    {
                        //remove reinforcements
                        System.Diagnostics.Debug.WriteLine("list reached max in else ally");
                        AllyNearbyParties.RemoveRange(0,AllyNearbyParties.Count());
                    }
                    if(currentLocalCycle > EnemyNearbyParties.Count())
                    {
                        //remove reinforcements for enemy
                        System.Diagnostics.Debug.WriteLine("list reached max in else for enemy");
                        EnemyNearbyParties.RemoveRange(0, EnemyNearbyParties.Count());
                    }*/

                    int count = 0;
                    //enemy
                    if (currentLocalCycle < EnemyNearbyParties.Count() && enoughMen(currentLocalCycle, EnemyNearbyParties, Mission.Current.PlayerEnemyTeam))
                    {
                        if (Mission.Current.Time >= getDelaySpawnTime(EnemydistancePartyList.ElementAt(currentLocalCycle)))
                        {
                            System.Diagnostics.Debug.WriteLine("spawning enemy in else");
                            await CalculatingMisc(EnemyNearbyParties, Mission.Current.PlayerEnemyTeam);
                            break;

                        }
                        else
                        {
                            count++;
                        }

                    }
                    //ally
                    if (currentLocalCycle < AllyNearbyParties.Count() && enoughMen(currentLocalCycle, AllyNearbyParties, Mission.Current.PlayerTeam))
                    {
                        if (Mission.Current.Time >= getDelaySpawnTime(AllydistancePartyList.ElementAt(currentLocalCycle)))
                        {
                            System.Diagnostics.Debug.WriteLine("spawning ally in else");

                            await CalculatingMisc(AllyNearbyParties, Mission.Current.PlayerTeam);
                            break;
                        }
                        else
                        {
                            count++;
                        }

                    }


                    currentLocalCycle++;

                    if (count == 2)
                        break;
                }
                if (calculateNextPartyTime() == false)
                {
                    //remove all lists
                    AllyNearbyParties.RemoveRange(0, AllyNearbyParties.Count());
                    EnemyNearbyParties.RemoveRange(0, EnemyNearbyParties.Count());
                }

            }

            calculateNextPartyLock = false; //unlock calculate
            System.Diagnostics.Debug.WriteLine("SKIPPED ALL SPAWN PATHS");


        }

        private static bool enoughMen(int currentPartyInList, List<MobileParty> whichSide, Team thisteam) //if the current reinforcements arent enough to help the army then
        {
            Team OppositeTeam;
            if (thisteam == Mission.Current.PlayerTeam)
            {
                OppositeTeam = Mission.Current.PlayerEnemyTeam;
            }
            else
            {
                OppositeTeam = Mission.Current.PlayerTeam;
            }

            float ratio = 1.1f;
            if (thisteam == Mission.Current.PlayerTeam)
            {
                ratio = 0.8f;
            }


            int currentActiveAgents = thisteam.ActiveAgents.Count();
            int totalNewMen = 0;
            for (int i = 0; i <= currentPartyInList; i++)
            {
                totalNewMen += whichSide.ElementAt(i).Party.NumberOfHealthyMembers;
            }
            if (OppositeTeam.ActiveAgents.Count() * ratio < currentActiveAgents + totalNewMen)
            {
                System.Diagnostics.Debug.WriteLine("enoguh men is true " + (currentActiveAgents + totalNewMen));
                return true;
            }
            System.Diagnostics.Debug.WriteLine("enoguh men is false " + (currentActiveAgents + totalNewMen));
            return false;
        }

        private async Task CalculatingMisc(List<MobileParty> list, Team team)
        {

            System.Diagnostics.Debug.WriteLine("Calcualting misk for " + team.ToString());
            changeAmountMenAllowed();
            moveEnemyPartiesRegardless(0, MapEvent.PlayerMapEvent.GetLeaderParty(team.Side).Position2D, list); //move AI party to their friendly neighbor position
            MobileParty newMobileParty = new MobileParty();
            newMobileParty = list.ElementAt(0);
            list.RemoveAt(0);
            await spawnTroopsBeta(newMobileParty, team);
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
            if (curretParty.Army != null)
            {
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
            Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/ui/mission/horns/attack"), Mission.Current.PlayerEnemyTeam.ActiveAgents.Last().Position, true, true, 0, 0);

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
            float newBigSpawnTimer = 0;

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
            if (BigSpawnTimer < newBigSpawnTimer)
            {
                BigSpawnTimer = newBigSpawnTimer;
                return true;
            }
            else
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
                        true, true, false, 0, 1, true, true, true);
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
                    await nightNight(500);
                }

                try
                {
                    IAgentOriginBase rand = QueueList.GetRandomElement();

                    Agent GTSFAgent = Mission.Current.SpawnTroop(rand, playerSide, true,
                            true, true, false, 0, 1, true, true, true);
                    QueueList.Remove(rand);
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("Caught exception within spawn new reinforcements");
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

            return (((distance / 2.3f) * 90000.0f) / 1000.0f) / 5;
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
            //currentSpawnLogic.

            //List<Dictionary<CharacterObject, int>> listDiction = new List<Dictionary<CharacterObject, int>>();
            IEnumerable<IAgentOriginBase> timKimKim;
            List<IAgentOriginBase> timKim2 = new List<IAgentOriginBase>();
            //4 parties

                Dictionary<CharacterObject, int> Tempdiction = new Dictionary<CharacterObject, int>();
                for (int j = 0; j < EnemyNearbyParties.ElementAt(0).MemberRoster.Count(); j++)
                {
                    Tempdiction.Add(EnemyNearbyParties.ElementAt(0).MemberRoster.GetCharacterAtIndex(j), 1);//j could mean amount
                }
            PartyGroupTroopSupplier partyGroupTroopSupplier = new PartyGroupTroopSupplier(MapEvent.PlayerMapEvent, Mission.Current.PlayerEnemyTeam.Side, Tempdiction);
            //partyGroupTroopSupplier.
            PartyGroupAgentOrigin partyGroupAgentOrigin =
                                (PartyGroupAgentOrigin)typeof(PartyGroupAgentOrigin).GetConstructor(BindingFlags.NonPublic |
                                BindingFlags.Instance, null, new Type[] { typeof(PartyGroupTroopSupplier), typeof(UniqueTroopDescriptor),
                                    typeof(int) }, null).Invoke(new object[] { partyGroupTroopSupplier, new UniqueTroopDescriptor(27492338), 1 }); 

             IMissionTroopSupplier[] newSuplier = new IMissionTroopSupplier[2];
            newSuplier[0] = partyGroupTroopSupplier;
            newSuplier[1] = partyGroupTroopSupplier;


            MissionAgentSpawnLogic newMASL = new MissionAgentSpawnLogic(newSuplier, Mission.Current.PlayerTeam.Side);
            Mission.Current.AddMissionBehaviour(newMASL);
            newMASL.InitWithSinglePhase(10,10,10,10,true,true);
            newMASL.SetSpawnTroops(Mission.Current.PlayerEnemyTeam.Side, true);
            testing = true;
            int h0 = 2;
            //newMASL.
        }
        

        private bool withinPartiesFighting(TextObject party)
        {
            for(int i =0; i< MapEvent.PlayerMapEvent.InvolvedParties.Count(); i++)
            {
                if (party == MapEvent.PlayerMapEvent.InvolvedParties.ElementAt(i).Name)
                    return true;
            }
            return false;
        }

        

        

        private static void unitStuckFix()
        {

        }
        private static async Task<int> maxTroop()
        {
            while (maxTroopReached == true)
            {
                await nightNight(5000);

            }
            return 1;
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
                    Mission.Current.PlayerTeam.ActiveAgents.ElementAt(i).SetMorale(99999);
                    Mission.Current.PlayerTeam.ActiveAgents.ElementAt(i).StopRetreatingMoraleComponent();
                    Mission.Current.PlayerTeam.MasterOrderController.SetOrderWithAgent((OrderType)4, Mission.Current.PlayerTeam.ActiveAgents.ElementAt(i));
                }
            }
            else
            {
                int sizeEnemyTeam = Mission.Current.PlayerEnemyTeam.ActiveAgents.Count();
                for (int i = 0; i < sizeEnemyTeam; i++)
                {
                    Mission.Current.PlayerEnemyTeam.ActiveAgents.ElementAt(i).SetMorale(99999);
                    Mission.Current.PlayerEnemyTeam.ActiveAgents.ElementAt(i).StopRetreatingMoraleComponent();
                    Mission.Current.PlayerEnemyTeam.MasterOrderController.SetOrderWithAgent((OrderType)4, Mission.Current.PlayerEnemyTeam.ActiveAgents.ElementAt(i));
                    //Mission.Current.PlayerEnemyTeam.ActiveAgents.ElementAt(i).GetMovementDirection(Mission.Current.Play))
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
            if (!EnemyNearbyParties.IsEmpty())
                EnemyNearbyParties.Clear();
            if (!EnemydistancePartyList.IsEmpty())
                EnemydistancePartyList.Clear();

            if (!AllyNearbyParties.IsEmpty())
                AllyNearbyParties.Clear();
            if (!AllydistancePartyList.IsEmpty())
                AllydistancePartyList.Clear();
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

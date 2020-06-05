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
using System.Threading;
using System.Diagnostics;
using JetBrains.Annotations;

public class MyQueue
{
    public Queue<PartyBase> playerQueue { get; }
    public Queue<PartyBase> enemyQueue { get; }


    public MyQueue(Queue<PartyBase> playerAllies, Queue<PartyBase> enemyAllies)
    {
        playerQueue = playerAllies;
        enemyQueue = enemyAllies;
    }

    private void playerQueueAdd(PartyBase party)
    {
        playerQueue.Enqueue(party);
    }
    private void enemyQueueAdd(PartyBase party)
    {
        playerQueue.Enqueue(party);
    }

    private void findNextParty()
    {

    }



}

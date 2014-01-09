using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.realm;
using wServer.realm.entities;

namespace wServer.logic
{
    class ChatEvent : ConditionalBehavior
    {
        String[] chat;
        Behavior[] behaves;
        Behavior[] falseBehaves = null;
        bool adminOnly = false;

        public ChatEvent(params Behavior[] behaves)
        {
            this.behaves = behaves;
        }

        public ChatEvent SetChats(params String[] chat)
        {
            this.chat = chat;
            return this;
        }

        public ChatEvent FalseEvent(params Behavior[] falseBehaves)
        {
            this.falseBehaves = falseBehaves;
            return this;
        }

        public ChatEvent AdminOnly()
        {
            this.adminOnly = true;
            return this;
        }

        public override BehaviorCondition Condition
        {
            get { return BehaviorCondition.OnChat; }
        }

        protected override void BehaveCore(BehaviorCondition cond, RealmTime? time, object state, Object[] args)
        {
            if (args.Length != 2)
                return;
            string msg = args[0] as string;
            Player player = args[1] as Player;
            if (!adminOnly || player.Client.Account.Rank >= 2)
            {
                foreach (var s in chat)
                {
                    if (msg.ToLower() == s.ToLower())
                    {
                        foreach (var i in behaves)
                        {
                            i.Tick(Host, (RealmTime)time);
                        }
                        return;
                    }
                }
                if (falseBehaves != null)
                {
                    foreach (var f in falseBehaves)
                    {
                        f.Tick(Host, (RealmTime)time);
                    }
                }
            }
        }
    }
    class NPCChatEvent : ConditionalBehavior
    {
        String[] chat;
        Behavior[] behaves;
        Behavior[] falseBehaves = null;
        bool adminOnly = false;
        bool setNPCFollowing = false;

        public NPCChatEvent(params Behavior[] behaves)
        {
            this.behaves = behaves;
        }

        public NPCChatEvent SetChats(params String[] chat)
        {
            this.chat = chat;
            return this;
        }

        public NPCChatEvent FalseEvent(params Behavior[] falseBehaves)
        {
            this.falseBehaves = falseBehaves;
            return this;
        }

        public NPCChatEvent SetFollower()
        {
            this.setNPCFollowing = true;
            return this;
        }

        public NPCChatEvent AdminOnly()
        {
            this.adminOnly = true;
            return this;
        }

        public override BehaviorCondition Condition
        {
            get { return BehaviorCondition.OnChat; }
        }

        protected override void BehaveCore(BehaviorCondition cond, RealmTime? time, object state, Object[] args)
        {
            if (args.Length != 3)
                return;
            string msg = args[0] as string;
            Player player = args[1] as Player;
            if (!adminOnly || player.Client.Account.Rank >= 2)
            {
                foreach (var s in chat)
                {
                    if (msg.ToLower().Contains(s.ToLower()))
                    {
                        foreach (var i in behaves)
                        {
                            i.Tick(Host, (RealmTime)time);
                        }
                        if (setNPCFollowing)
                            (Host.Self as NPC).NPCFollowing = player;
                        return;
                    }
                }
                if (falseBehaves != null)
                {
                    foreach (var f in falseBehaves)
                    {
                        f.Tick(Host, (RealmTime)time);
                    }
                }
            }
        }
    }
}

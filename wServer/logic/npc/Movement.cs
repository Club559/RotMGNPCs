using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.realm;
using wServer.realm.entities;
using wServer.svrPackets;
using Mono.Game;

namespace wServer.logic.npc
{
    class NPCRemoveFollowing : Behavior
    {
        public NPCRemoveFollowing()
        {
        }

        protected override bool TickCore(RealmTime time)
        {
            (Host.Self as NPC).NPCFollowing = null;
            return true;
        }
    }
    class NPCFollowingPresent : Behavior
    {
        float radius;
        private NPCFollowingPresent(float radius)
        {
            this.radius = radius;
        }
        static readonly Dictionary<Tuple<float>, NPCFollowingPresent> instances = new Dictionary<Tuple<float>, NPCFollowingPresent>();
        public static NPCFollowingPresent Instance(float radius)
        {
            var key = new Tuple<float>(radius);
            NPCFollowingPresent ret;
            if (!instances.TryGetValue(key, out ret))
                ret = instances[key] = new NPCFollowingPresent(radius);
            return ret;
        }

        Random rand = new Random();
        protected override bool TickCore(RealmTime time)
        {
            float dist = radius;
            return GetNearestEntities(dist, null).Contains((Host.Self as NPC).NPCFollowing);
        }
    }
    class NPCFollowPlayer : Behavior
    {
        float speed;
        float radius;
        float targetRadius;
        private NPCFollowPlayer(float speed, float radius, float targetRadius)
        {
            this.speed = speed;
            this.radius = radius;
            this.targetRadius = targetRadius;
        }
        static readonly Dictionary<Tuple<float, float, float>, NPCFollowPlayer> instances = new Dictionary<Tuple<float, float, float>, NPCFollowPlayer>();
        public static NPCFollowPlayer Instance(float speed, float radius, float targetRadius)
        {
            var key = new Tuple<float, float, float>(speed, radius, targetRadius);
            NPCFollowPlayer ret;
            if (!instances.TryGetValue(key, out ret))
                ret = instances[key] = new NPCFollowPlayer(speed, radius, targetRadius);
            return ret;
        }

        Random rand = new Random();
        protected override bool TickCore(RealmTime time)
        {
            if (Host.Self.HasConditionEffect(ConditionEffects.Paralyzed)) return true;
            if ((Host.Self as NPC).NPCFollowing == null) return true;
            var speed = this.speed * GetSpeedMultiplier(Host.Self);

            float dist = radius;
            Entity entity = (Host.Self as NPC).NPCFollowing;
            if (entity != null && dist > targetRadius)
            {
                var tx = entity.X;
                var ty = entity.Y;
                if (tx != Host.Self.X || ty != Host.Self.Y)
                {
                    var x = Host.Self.X;
                    var y = Host.Self.Y;
                    Vector2 vect = new Vector2(tx, ty) - new Vector2(Host.Self.X, Host.Self.Y);
                    vect.Normalize();
                    vect *= (speed / 1.5f) * (time.thisTickTimes / 1000f);
                    ValidateAndMove(Host.Self.X + vect.X, Host.Self.Y + vect.Y);
                    Host.Self.UpdateCount++;
                }
                return true;
            }
            else return false;
        }
    }
}

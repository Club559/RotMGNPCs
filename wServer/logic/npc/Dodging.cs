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
    class NPCDodging : Behavior
    {
        float radius;
        int predict;

        private NPCDodging(float radius, int predict) { this.radius = radius; this.predict = predict; }

        static readonly Dictionary<Tuple<float, int>, NPCDodging> instances = new Dictionary<Tuple<float, int>, NPCDodging>();
        public static NPCDodging Instance(float radius, int predict)
        {
            var key = new Tuple<float, int>(radius, predict);
            NPCDodging ret;
            if (!instances.TryGetValue(key, out ret))
                ret = instances[key] = new NPCDodging(radius, predict);
            return ret;
        }

        Random rand = new Random();
        protected override bool TickCore(RealmTime time)
        {
            float dist = radius;
            Entity entity = GetNearestEntity(ref dist, 1);
            if (entity != null)
            {
                Projectile proj = entity as Projectile;
                long elapsedTicks = time.tickTimes - proj.BeginTime;
                Position newPos = proj.GetPosition(elapsedTicks + predict);
                var colMap = Host.Self.ObjectDesc.Enemy ? Host.Self.Owner.EnemiesCollision : Host.Self.Owner.PlayersCollision;
                if (colMap.HitTest(newPos.X, newPos.Y, 2).Contains(Host.Self))
                {
                    return true;
                }
            }
            return false;
        }
    }
}

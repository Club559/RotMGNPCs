using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.svrPackets;
using wServer.logic;
using wServer.realm;

namespace wServer.realm.entities
{
    public class NPC : Enemy, IPlayer
    {
        public Entity NPCFollowing { get; set; }

        public NPC(short objType)
            : base(objType)
        {
            isNPC = true;
        }

        public void Damage(int dmg, Character chr)
        {
            if (HasConditionEffect(ConditionEffects.Paused) ||
                HasConditionEffect(ConditionEffects.Stasis) ||
                HasConditionEffect(ConditionEffects.Invincible))
                return;

            dmg = (int)StatsManager.GetDefenseDamage(this, dmg, ObjectDesc.Defense);
            if (!HasConditionEffect(ConditionEffects.Invulnerable))
                HP -= dmg;
            UpdateCount++;
            Owner.BroadcastPacket(new DamagePacket()
            {
                TargetId = this.Id,
                Effects = 0,
                Damage = (ushort)dmg,
                Killed = HP <= 0,
                BulletId = 0,
                ObjectId = chr.Id
            }, null);

            string enName = chr.Name == "" ? null : chr.Name;
            string killer = enName ??
                    chr.ObjectDesc.DisplayId ??
                    chr.ObjectDesc.ObjectId;
            if (HP <= 0) Death(killer);
        }

        public override bool HitByProjectile(Projectile projectile, RealmTime time)
        {
            if (HasConditionEffect(ConditionEffects.Invincible))
                return false;
            bool projOwnerTest = false;
            if (ObjectDesc.Enemy)
                if (projectile.ProjectileOwner is Player)
                    projOwnerTest = true;
                else
                    projOwnerTest = false;
            else
                if ((projectile.ProjectileOwner is Enemy) && !(projectile.ProjectileOwner is NPC))
                    projOwnerTest = true;
                else
                    projOwnerTest = false;
            if (projectile.ProjectileOwner is NPC)
                if ((projectile.ProjectileOwner as NPC).ObjectDesc.Enemy != ObjectDesc.Enemy)
                    projOwnerTest = true;
            if (projOwnerTest &&
                !HasConditionEffect(ConditionEffects.Paused) &&
                !HasConditionEffect(ConditionEffects.Stasis))
            {
                var def = this.ObjectDesc.Defense;
                if (projectile.Descriptor.ArmorPiercing)
                    def = 0;
                int dmg = (int)StatsManager.GetDefenseDamage(this, projectile.Damage, def);
                if (!HasConditionEffect(ConditionEffects.Invulnerable))
                    HP -= dmg;
                ApplyConditionEffect(projectile.Descriptor.Effects);
                Owner.BroadcastPacket(new DamagePacket()
                {
                    TargetId = this.Id,
                    Effects = projectile.ConditionEffects,
                    Damage = (ushort)dmg,
                    Killed = HP < 0,
                    BulletId = projectile.ProjectileId,
                    ObjectId = projectile.ProjectileOwner.Self.Id
                }, projectile.ProjectileOwner is Player ? projectile.ProjectileOwner as Player : null);

                foreach (var i in CondBehaviors)
                    if ((i.Condition & BehaviorCondition.OnHit) != 0)
                        i.Behave(BehaviorCondition.OnHit, this, time, projectile);

                if (projectile.ProjectileOwner is Player) DamageCounter.HitBy(projectile.ProjectileOwner as Player, projectile, dmg);

                if (HP < 0)
                {
                    Entity chr = projectile.ProjectileOwner.Self;
                    string enName = chr.Name == "" ? null : chr.Name;
                    string killer = enName ??
                            chr.ObjectDesc.DisplayId ??
                            chr.ObjectDesc.ObjectId;
                    if (HP <= 0) Death(killer);
                }
                UpdateCount++;
                return true;
            }
            return false;
        }

        public bool IsVisibleToEnemy()
        {
            if (HasConditionEffect(ConditionEffects.Paused))
                return false;
            if (HasConditionEffect(ConditionEffects.Invisible))
                return false;
            return true;
        }

        void GenerateGravestone()
        {
            StaticObject obj = new StaticObject(0x0725, 5 * 60 * 1000, true, true, false);
            obj.Move(X, Y);
            obj.Name = this.Name;
            Owner.EnterWorld(obj);
        }

        public void Death(string killer)
        {
            foreach (var i in CondBehaviors)
                if ((i.Condition & BehaviorCondition.OnDeath) != 0)
                    i.Behave(BehaviorCondition.OnDeath, this, null, DamageCounter);
            DamageCounter.Death();
            GenerateGravestone();
            Owner.BroadcastPacket(new TextPacket()
            {
                BubbleTime = 0,
                Stars = -1,
                Name = "",
                Text = Name + " died at Level 20, killed by " + killer
            }, null);
            if (Owner != null)
                Owner.LeaveWorld(this);
        }
    }
}

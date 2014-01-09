using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace wServer.realm.entities
{
    public abstract class Character : Entity
    {
        public wRandom Random { get; private set; }
        protected Character(short objType, wRandom rand)
            : base(objType)
        {
            this.Random = rand;

            if (ObjectDesc != null)
            {
                //Name = ObjectDesc.DisplayId ?? "";
                if (ObjectDesc.SizeStep != 0)
                {
                    var step = Random.Next(0, (ObjectDesc.MaxSize - ObjectDesc.MinSize) / ObjectDesc.SizeStep + 1) * ObjectDesc.SizeStep;
                    Size = ObjectDesc.MinSize + step;
                }
                else
                    Size = ObjectDesc.MinSize;

                HP = ObjectDesc.MaxHP;
                Stars = ObjectDesc.Stars;
                isNPC = ObjectDesc.Class == "NPC";
            }
            else
            {
                Stars = 0;
                isNPC = false;
            }
        }

        protected override void ImportStats(StatsType stats, object val)
        {
            base.ImportStats(stats, val);
            switch (stats)
            {
                case StatsType.Stars: Stars = (int)val; break;
            }
        }

        protected override void ExportStats(IDictionary<StatsType, object> stats)
        {
            base.ExportStats(stats);
            stats[StatsType.Stars] = Stars;
        }

        public int HP { get; set; }
        public int Stars { get; set; }
        public bool isNPC { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using wServer.realm;
using wServer.logic.attack;
using wServer.logic.movement;
using wServer.logic.loot;
using wServer.logic.taunt;
using wServer.logic.cond;
using wServer.logic.npc;

namespace wServer.logic
{
    partial class BehaviorDb
    {
        static _ NPC = Behav()
            .Init(0x1960, Behaves("Rogue NPC",
                If.Instance(IsEntityPresent.Instance(8, 0),
                    IfEqual.Instance(-1, 0,
                        new RunBehaviors( //Attacking Enemy
                            IfNot.Instance(NPCDodging.Instance(3, 5),
                                MaintainDist.Instance(14, 9, 3, 0)
                            ),
                            Cooldown.Instance(100, NPCSimpleAttack.Instance(6))
                        ),
                        new RunBehaviors( //Escaping
                            Escaping.Instance(7, 14, 5000, 0)
                        )
                    ),
                    new RunBehaviors( //Passive Mode
                        new State("idle",
                            IfEqual.Instance(-1, 0,
                                new QueuedBehavior(
                                    Timed.Instance(4000, SimpleWandering.Instance(5)),
                                    Cooldown.Instance(4000, NullBehavior.Instance)
                                )
                            )
                        ),
                        new State("follow",
                            IfEqual.Instance(-1, 0,
                                IfNot.Instance(NPCFollowingPresent.Instance(1),
                                    NPCFollowPlayer.Instance(14, 200, 1)
                                )
                            )
                        )
                    )
                ),
                new RunBehaviors( //Projectile dodging
                    If.Instance(NPCDodging.Instance(3, 5),
                        Circling.Instance(10, 10, 14, 0)
                    ),
                    MaintainDist.Instance(7, 3, 3, 1)
                ),
                new RunBehaviors( //Misc
                    HpLesserPercent.Instance(0.2f, new RunBehaviors( //Flashing red & heal taunts
                        new QueuedBehavior( Flashing.Instance(500, 0xFFFF0000) ),
                        Cooldown.Instance(15000, Rand.Instance(
                            new NPCSimpleTaunt("Heal please!"),
                            new NPCSimpleTaunt("Flashing red over here."),
                            new NPCSimpleTaunt("I'm about to die!")
                        )),
                        new SetKey(-1, 1)
                    ), new SetKey(-1, 0)),
                    Cooldown.Instance(15000, // Random Invisiblity
                        If.Instance(IsEntityPresent.Instance(6, 0),
                            IfNot.Instance(CheckConditionEffects.Instance(new ConditionEffects[] { ConditionEffects.Invisible }),
                                new SetConditionEffectTimed(ConditionEffectIndex.Invisible, 5000)
                            )
                        )
                    ),
                    Cooldown.Instance(40000, Heal.Instance(1f, 100, 0x1960)) //Random Health
                ),
                condBehaviors: new ConditionalBehavior[] {
                    new NPCChatEvent(
                        new NotState("follow",
                            new NPCSimpleTaunt("Right behind you."),
                            SetState.Instance("follow")
                        )
                    ).SetChats("follow me", "come here", "come over here", "let's go", "lets go").SetFollower(),
                    new NPCChatEvent(
                        new State("follow",
                            new NPCSimpleTaunt("I'll keep watch here."),
                            SetState.Instance("idle")
                        )
                    ).SetChats("drop off","stay here","stay there","keep watch","keep guard","wander")
                }
            ))
            .Init(0x1961, Behaves("Pirate NPC Evil",
                new RunBehaviors(
                    If.Instance(Chasing.Instance(5, 1, 5, null), Cooldown.Instance(200, SimpleAttack.Instance(5)),
                        new QueuedBehavior(
                            Timed.Instance(2000, SimpleWandering.Instance(2)),
                            Cooldown.Instance(4000, NullBehavior.Instance)
                        )
                    ),
                    Cooldown.Instance(15000, new NPCSimpleTaunt("I am an evil pirate. I will kill you, {PLAYER}!"))
                )
            ));
    }
}

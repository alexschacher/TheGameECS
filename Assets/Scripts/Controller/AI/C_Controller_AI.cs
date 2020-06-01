using System;
using Unity.Entities;

public enum AIBehavior { Wander, RunFromEntity, ChaseAndAttackEntity, FollowEntity }

[Serializable]
public struct C_Controller_AI : IComponentData
{
    public AIBehavior idleBehavior;
    public AIBehavior reactionBehavior;
    public AIBehavior currentBehavior;

    public bool hasTarget;
    public Entity targetEntity;
}
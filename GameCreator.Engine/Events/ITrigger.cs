﻿using GameCreator.Resources.Api;

namespace GameCreator.Engine
{
    public interface ITrigger : IIndexedResource
    {
        bool Trigger(GameInstance instance);
    }
}
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Data;

namespace RTSEngine.Interfaces {
    public interface ISquadController {
        // The Entity That This Controller is Controlling
        ISquad Squad { get; }

        // Will Set Once And Then Fail On Later Occurences
        void SetSquad(ISquad s);
    }

    public interface ITargettingController : ISquadController {
        // Find A Target For This Controller's Entity
        void FindTarget(GameState g, float dt);

        // Set A Target For This Controller's Entity
        void ChangeTarget(GameState g, float dt);
    }
}
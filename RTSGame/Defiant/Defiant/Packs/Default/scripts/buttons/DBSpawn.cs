﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Controllers;
using RTSEngine.Interfaces;
using Microsoft.Xna.Framework;

namespace RTS.Default.Buttons.Spawn {
    public class Unit : ACBuildingButtonController {
        public int UnitType {
            get;
            private set;
        }
        public override int QueueTime {
            get { return building.Team.Race.Units[UnitType].BuildTime; }
        }
        public override string IconKey {
            get { return building.Team.Race.Units[UnitType].IconKey; }
        }

        public override void Init(GameState s, GameplayController c, object args) {
            UnitType = (int)args;
        }

        public override bool CanFinish(GameState g) {
            return
                building.Team.Capital >= building.Team.Race.Units[UnitType].CapitalCost &&
                building.Team.Units.Count < building.Team.PopulationCap;
        }
        public override void OnQueueFinished(GameState s) {
            building.Team.Input.AddEvent(new SpawnUnitEvent(
                building.Team.Index, UnitType, building.GridPosition));
        }
        public override void OnClick() {
            if (building.ActionController.ButtonQueue.Count < building.ActionController.QueueCap)
                Enqueue();
        }

        public override void DecideAction(GameState s, float dt) {
        }
        public override void ApplyAction(GameState s, float dt) {
        }

        public override void Serialize(BinaryWriter s) {
        }
        public override void Deserialize(BinaryReader s) {
        }
    }
}
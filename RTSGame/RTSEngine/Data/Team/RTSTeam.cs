﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RTSEngine.Data.Team {
    public class RTSTeam {
        // This Is All The Units In The Team
        private List<RTSUnitInstance> units;
        public IEnumerable<RTSUnitInstance> Units {
            get { return units; }
        }
        public int UnitCount {
            get { return units.Count; }
        }

        public RTSTeam() {
            units = new List<RTSUnitInstance>();
        }

        public void AddUnit(RTSUnit data, Vector3 pos) {
            RTSUnitInstance rui = new RTSUnitInstance(this, data, pos);
        }

        public void RemoveUnit(RTSUnitInstance u) {
            units.Remove(u);
        }
        public void RemoveAll(Predicate<RTSUnitInstance> f) {
            units.RemoveAll(f);
        }
    }
}
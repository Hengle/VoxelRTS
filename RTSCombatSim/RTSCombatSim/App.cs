using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using RTSCS.Gameplay;
using RTSCS.Graphics;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSCS.Controllers;

namespace RTSCS {
    public class GameRestartArgs {
        public RTSTeam[] Teams;
        public RTSUnit[] Units;
    };

    public class App : Microsoft.Xna.Framework.Game {
        const int MAX_TEAMS = 3;
        const int MAX_UNITS = 3;
        const int MAX_INSTANCES_PER_UNIT = 100;

        static readonly Color[] teamColors = new Color[]{
            Color.Red,
            Color.Blue,
            Color.Green
        };

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Renderer renderer;
        CombatMap map;

        // The Game State
        public GameState GameState {
            get;
            private set;
        }
        public RTSUnit[] Units {
            get;
            private set;
        }
        public RTSTeam[] Teams {
            get { return GameState.teams; }
        }

        // Used To Render Units
        UnitGeometry[] unitGeometry;

        public App() {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";

            // Setup Teams
            RTSTeam[] teams = new RTSTeam[MAX_TEAMS];
            for(int i = 0; i < teams.Length; i++)
                teams[i] = new RTSTeam();

            // Setup Units
            Units = new RTSUnit[MAX_UNITS];
            for(int i = 0; i < Units.Length; i++) {
                Units[i].BaseCombatData.Armor = 0;
                Units[i].BaseCombatData.AttackDamage = 10;
                Units[i].BaseCombatData.AttackTimer = 1f;
                Units[i].BaseCombatData.CriticalChance = 0.5;
                Units[i].BaseCombatData.CriticalDamage = 20;
                Units[i].BaseCombatData.MaxRange = 20;
                Units[i].BaseCombatData.MinRange = 0;
                Units[i].Health = 100;
                Units[i].ICollidableShape = new CollisionCircle(15, Vector2.Zero);
                Units[i].MovementSpeed = 2f;
            }

            // Create Game State
            GameState = new GameState(teams);
            foreach(var uD in Units)
                GameState.AddRTSUnit(uD);

        }

        protected override void Initialize() {
            base.Initialize();
        }
        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            renderer = new Renderer(GraphicsDevice, XNAEffect.Compile(GraphicsDevice, "Content\\FX\\Unit.fx"));
            renderer.View = Matrix.CreateLookAt(new Vector3(0, 0, -1f), Vector3.Zero, Vector3.Up);
            renderer.Projection = Matrix.CreateOrthographic(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 2f);

            map = new CombatMap(GraphicsDevice, @"Content\Textures\Smoke.png");
            map.Tiling = Vector2.One * 10f;
            map.Scaling = Vector2.One * 200f;
            map.Translation = Vector3.Zero;

            for(int i = 0; i < Units.Length; i++) {
                unitGeometry[i] = new UnitGeometry(GraphicsDevice, new VertexPositionColor[] {
                    new VertexPositionColor(new Vector3(-1, 1, 0), Color.White),
                    new VertexPositionColor(new Vector3(1, 1, 0), Color.White),
                    new VertexPositionColor(new Vector3(-1, -1, 0), Color.White),
                    new VertexPositionColor(new Vector3(1, -1, 0), Color.White)
                }, new int[]{
                    0, 1, 2,
                    2, 1, 3
                }, MAX_INSTANCES_PER_UNIT, Units[i]);
            }
        }
        protected override void UnloadContent() {
            renderer.Dispose();
            map.Dispose();
        }

        protected override void Update(GameTime gameTime) {
            if(DataForm.Instance == null) {
                Exit();
                return;
            }

            StepGame((float)gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            renderer.RenderMap(GraphicsDevice, map);
            renderer.BeginUnitPass();
            foreach(UnitGeometry ug in unitGeometry) {
                ug.InstanceUnits();
                ug.SetBuffers(GraphicsDevice);
                ug.DrawUnits(GraphicsDevice);
            }

            base.Draw(gameTime);
        }

        static bool IsDead(RTSUnitInstance u) {
            return u.Health < 0;
        }
        public void StepGame(float dt) {
            // Find Decisions
            foreach(RTSTeam team in GameState.teams) {
                foreach(RTSUnitInstance unit in team.Units) {
                    unit.ActionController.DecideAction(GameState, dt);
                }
            }

            // Apply Controllers
            foreach(RTSTeam team in GameState.teams) {
                foreach(RTSUnitInstance unit in team.Units) {
                    unit.ActionController.ApplyAction(GameState, dt);
                }
            }

            // Kill Dead Units
            foreach(RTSTeam team in GameState.teams) {
                team.RemoveAll(IsDead);
            }
        }

        public void AddNewUnit(RTSUnitInstance u, Color c) {
            foreach(var ug in unitGeometry) {
                if(ug.UnitData == u.UnitData)
                    ug.AddUnit(u, c);
            }
        }

        static void Main(string[] args) {
            using(App game = new App()) {
                DataForm form = null;

                // Create Form Thread
                Thread t = new Thread(() => {
                    using(form = new DataForm()) {
                        form.OnGameRestart += (rA) => {
                            game.rArgs = rA;
                        };
                        form.ShowDialog();
                    }
                });
                t.SetApartmentState(ApartmentState.STA);
                t.Priority = ThreadPriority.Lowest;
                t.IsBackground = true;
                t.Start();

                // Wait For Max 1 Second To Initialize The Form Else Exit
                int trials = 10;
                while(trials > 0) {
                    if(DataForm.Instance != null) {
                        trials = 10;
                        break;
                    }
                    Thread.Sleep(100);
                    trials--;
                }

                // Run The Simulator
                game.Run();

                // Stop The Form Thread
                if(DataForm.Instance != null && form != null) {
                    form.Invoke(form.Closer);
                    while(DataForm.Instance != null) {
                        Thread.Sleep(500);
                        Console.WriteLine("Waiting For Form Thread To Stop");
                    }
                }
            }
        }
    }
}
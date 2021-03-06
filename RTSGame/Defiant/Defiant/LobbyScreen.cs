﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using BlisterUI;
using BlisterUI.Input;
using BlisterUI.Widgets;
using RTSEngine.Data.Team;
using RTSEngine.Controllers;
using RTSEngine.Data;
using RTSEngine.Data.Parsers;
using System.Text.RegularExpressions;
using RTSEngine.Interfaces;

namespace RTS {
    public struct GamePreset {
        public string Name;
        public string LoadImage;
        public string GameType;
        public string Map;
        public string[] PlayerTypes;
        public string[] InputTypes;
        public object[] InputInitArgs;
        public string[] Races;
        public string[] Colors;
    }

    public class TeamInitWidget : IDisposable {
        public BaseWidget Parent {
            set {
                BackRect.Parent = value;
            }
        }

        public RectWidget BackRect {
            get;
            private set;
        }

        public TextWidget TextIndex {
            get;
            private set;
        }
        public TextWidget TextUser {
            get;
            private set;
        }

        public RectButton ButtonPlayerType {
            get;
            private set;
        }
        public TextWidget TextPlayerType {
            get;
            private set;
        }
        public string PlayerType {
            get { return TextPlayerType.Text; }
            set { TextPlayerType.Text = value; }
        }

        public RectButton ButtonRace {
            get;
            private set;
        }
        public TextWidget TextRace {
            get;
            private set;
        }
        public string Race {
            get { return TextRace.Text; }
            set { TextRace.Text = value; }
        }

        public RectButton ButtonScheme {
            get;
            private set;
        }
        public TextWidget TextScheme {
            get;
            private set;
        }
        public string Scheme {
            get { return TextScheme.Text; }
            set { TextScheme.Text = value; }
        }

        private string[] lRaces, lSchemes, lTypes;
        private int ri, si, ti;

        public TeamInitWidget(WidgetRenderer wr, int w, int h, int buf, Color cBack, ButtonHighlightOptions bh1, ButtonHighlightOptions bh2, Color cText) {
            BackRect = new RectWidget(wr);
            BackRect.Width = w;
            BackRect.Height = h;
            BackRect.Offset = new Point(0, 0);
            BackRect.OffsetAlignY = Alignment.BOTTOM;
            BackRect.Color = cBack;
            BackRect.LayerDepth = 1f;

            int wh = h - buf * 2;

            TextIndex = new TextWidget(wr);
            TextIndex.Offset = new Point(buf, 0);
            TextIndex.OffsetAlignX = Alignment.LEFT;
            TextIndex.OffsetAlignY = Alignment.MID;
            TextIndex.AlignX = Alignment.LEFT;
            TextIndex.AlignY = Alignment.MID;
            TextIndex.Height = wh;
            TextIndex.Color = cText;
            TextIndex.Parent = BackRect;
            TextIndex.LayerDepth = 0.3f;

            TextUser = new TextWidget(wr);
            TextUser.Offset = new Point(buf, 0);
            TextUser.OffsetAlignX = Alignment.RIGHT;
            TextUser.OffsetAlignY = Alignment.MID;
            TextUser.AlignX = Alignment.LEFT;
            TextUser.AlignY = Alignment.MID;
            TextUser.Height = wh;
            TextUser.Color = cText;
            TextUser.Parent = TextIndex;
            TextUser.LayerDepth = 0.3f;

            ButtonScheme = new RectButton(wr, bh1, bh2);
            ButtonScheme.Offset = new Point(-buf, 0);
            ButtonScheme.OffsetAlignX = Alignment.RIGHT;
            ButtonScheme.OffsetAlignY = Alignment.MID;
            ButtonScheme.AlignX = Alignment.RIGHT;
            ButtonScheme.AlignY = Alignment.MID;
            ButtonScheme.Parent = BackRect;
            ButtonScheme.LayerDepth = 0.3f;
            TextScheme = new TextWidget(wr);
            TextScheme.Height = bh1.Height;
            TextScheme.Text = "Default";
            TextScheme.Offset = new Point(0, 0);
            TextScheme.OffsetAlignX = Alignment.MID;
            TextScheme.OffsetAlignY = Alignment.MID;
            TextScheme.AlignX = Alignment.MID;
            TextScheme.AlignY = Alignment.MID;
            TextScheme.Parent = ButtonScheme;
            TextScheme.Color = cText;
            TextScheme.LayerDepth = 0f;

            ButtonRace = new RectButton(wr, bh1, bh2);
            ButtonRace.Offset = new Point(-buf, 0);
            ButtonRace.OffsetAlignX = Alignment.LEFT;
            ButtonRace.OffsetAlignY = Alignment.MID;
            ButtonRace.AlignX = Alignment.RIGHT;
            ButtonRace.AlignY = Alignment.MID;
            ButtonRace.Parent = ButtonScheme;
            ButtonRace.LayerDepth = 0.3f;
            TextRace = new TextWidget(wr);
            TextRace.Height = bh1.Height;
            TextRace.Text = "Race";
            TextRace.Offset = new Point(0, 0);
            TextRace.OffsetAlignX = Alignment.MID;
            TextRace.OffsetAlignY = Alignment.MID;
            TextRace.AlignX = Alignment.MID;
            TextRace.AlignY = Alignment.MID;
            TextRace.Parent = ButtonRace;
            TextRace.Color = cText;
            TextRace.LayerDepth = 0f;

            ButtonPlayerType = new RectButton(wr, bh1, bh2);
            ButtonPlayerType.Offset = new Point(-buf, 0);
            ButtonPlayerType.OffsetAlignX = Alignment.LEFT;
            ButtonPlayerType.OffsetAlignY = Alignment.MID;
            ButtonPlayerType.AlignX = Alignment.RIGHT;
            ButtonPlayerType.AlignY = Alignment.MID;
            ButtonPlayerType.Parent = ButtonRace;
            ButtonPlayerType.LayerDepth = 0.3f;
            TextPlayerType = new TextWidget(wr);
            TextPlayerType.Height = bh1.Height;
            TextPlayerType.Text = "None";
            TextPlayerType.Offset = new Point(0, 0);
            TextPlayerType.OffsetAlignX = Alignment.MID;
            TextPlayerType.OffsetAlignY = Alignment.MID;
            TextPlayerType.AlignX = Alignment.MID;
            TextPlayerType.AlignY = Alignment.MID;
            TextPlayerType.Parent = ButtonPlayerType;
            TextPlayerType.Color = cText;
            TextPlayerType.LayerDepth = 0f;

            ButtonScheme.Hook();
            ButtonRace.Hook();
            ButtonPlayerType.Hook();

            ButtonScheme.OnButtonPress += ButtonScheme_OnButtonPress;
            ButtonRace.OnButtonPress += ButtonRace_OnButtonPress;
            ButtonPlayerType.OnButtonPress += ButtonPlayerType_OnButtonPress;
        }

        void ButtonScheme_OnButtonPress(RectButton obj, Vector2 m) {
            si = (si + 1) % lSchemes.Length;
            Scheme = lSchemes[si];
        }
        void ButtonRace_OnButtonPress(RectButton obj, Vector2 m) {
            ri = (ri + 1) % lRaces.Length;
            Race = lRaces[ri];
        }
        void ButtonPlayerType_OnButtonPress(RectButton obj, Vector2 m) {
            ti = (ti + 1) % lTypes.Length;
            PlayerType = lTypes[ti];
        }

        public void Dispose() {
            ButtonScheme.OnButtonPress -= ButtonScheme_OnButtonPress;
            ButtonRace.OnButtonPress -= ButtonRace_OnButtonPress;
            ButtonPlayerType.OnButtonPress -= ButtonPlayerType_OnButtonPress;

            BackRect.Dispose();
            TextUser.Dispose();
            ButtonPlayerType.Dispose();
            TextPlayerType.Dispose();
            ButtonRace.Dispose();
            TextRace.Dispose();
            ButtonScheme.Dispose();
            TextScheme.Dispose();
        }

        public void Set(string[] pTypes, Dictionary<string, FileInfo> races, Dictionary<string, RTSColorScheme> schemes) {
            lRaces = races.Keys.ToArray();
            lSchemes = schemes.Keys.ToArray();
            lTypes = pTypes;

            ri = 0;
            Race = lRaces[ri];
            si = 0;
            Scheme = lSchemes[si];
            ti = 0;
            PlayerType = lTypes[ti];
        }
    }



    public class LobbyScreen : GameScreen<App> {
        private static readonly Regex rgxLoadGame = RegexHelper.GenerateFile("load");
        private const string PRESET_DIR = @"Packs\presets";

        public override int Next {
            get { return game.LoadScreen.Index; }
            protected set { }
        }
        public override int Previous {
            get { return game.MenuScreen.Index; }
            protected set { }
        }

        // Init Info Helper
        public Dictionary<string, FileInfo> Races {
            get;
            private set;
        }
        Dictionary<string, RTSColorScheme> schemes;
        List<GamePreset> gPresets;

        private EngineLoadData eld;
        public EngineLoadData InitInfo {
            get { return eld; }
        }

        WidgetRenderer wr;
        TeamInitWidget[] widgets;
        TextWidget textGTController, textMap;
        ScrollMenu menuPresets;
        string[] inputTypes;
        object[] inputInitArgs;

        public override void Build() {
            gPresets = new List<GamePreset>();
            inputTypes = new string[GameState.MAX_PLAYERS];
            inputInitArgs = new object[GameState.MAX_PLAYERS];
            var di = new DirectoryInfo(PRESET_DIR);
            foreach(var fi in di.GetFiles()) {
                if(!fi.Extension.EndsWith(@"game")) continue;
                GamePreset gp = (GamePreset)ZXParser.ParseFile(fi.FullName, typeof(GamePreset));
                gPresets.Add(gp);
            }
        }
        public override void Destroy(GameTime gameTime) {
        }

        public override void OnEntry(GameTime gameTime) {
            // Load All The Races And Schemes
            Races = new Dictionary<string, FileInfo>();
            schemes = new Dictionary<string, RTSColorScheme>();
            GameEngine.SearchAllInitInfo(new DirectoryInfo("Packs"), Races, schemes);
            if(schemes.Count < 1)
                schemes.Add("Default", RTSColorScheme.Default);
            string defScheme = "Default";
            foreach(var kvp in schemes) {
                defScheme = kvp.Key;
                break;
            }

            // Set Init Data To Be Nothing
            game.LoadScreen.LoadFile = null;
            eld = new EngineLoadData();
            eld.Teams = new TeamInitOption[GameState.MAX_PLAYERS];
            for(int i = 0; i < eld.Teams.Length; i++) {
                eld.Teams[i].InputType = RTSInputType.None;
                eld.Teams[i].Race = null;
                eld.Teams[i].PlayerName = null;
                eld.Teams[i].Colors = schemes[defScheme];
            }
            game.LoadScreen.LoadData = eld;

            wr = new WidgetRenderer(G, game.Content.Load<SpriteFont>(@"Fonts\Impact32"));
            widgets = new TeamInitWidget[eld.Teams.Length];
            ButtonHighlightOptions bh1 = new ButtonHighlightOptions(120, 36, Color.Black);
            ButtonHighlightOptions bh2 = new ButtonHighlightOptions(120, 36, Color.DarkGray);
            string[] pt = { "None", "Player", "Computer", "Environment" };
            for(int i = 0; i < widgets.Length; i++) {
                widgets[i] = new TeamInitWidget(wr, 600, 44, 8, new Color(8, 8, 8), bh1, bh2, Color.Lime);
                if(i > 0) {
                    widgets[i].Parent = widgets[i - 1].BackRect;
                }
                widgets[i].TextIndex.Text = (i + 1).ToString();
                widgets[i].TextUser.Text = "Unknown";
                widgets[i].Set(pt, Races, schemes);
            }
            widgets[0].TextUser.Text = UserConfig.UserName;

            menuPresets = new ScrollMenu(wr,
                game.Window.ClientBounds.Width - widgets[0].BackRect.Width - 20,
                24,
                game.Window.ClientBounds.Height / 24,
                20,
                40
                );
            menuPresets.BaseColor = UserConfig.MainScheme.WidgetBase;
            menuPresets.TextColor = UserConfig.MainScheme.Text;
            menuPresets.ScrollBarBaseColor = UserConfig.MainScheme.WidgetInactive;
            menuPresets.HighlightColor = UserConfig.MainScheme.WidgetActive;
            menuPresets.Widget.Anchor = new Point(game.Window.ClientBounds.Width - 20, 0);
            menuPresets.Widget.AlignX = Alignment.RIGHT;
            menuPresets.Build((from gp in gPresets select gp.Name).ToArray());
            menuPresets.Hook();

            textMap = new TextWidget(wr);
            textMap.Color = UserConfig.MainScheme.Text;
            textMap.Offset = new Point(0, 5);
            textMap.Height = 32;
            textMap.OffsetAlignY = Alignment.BOTTOM;
            textMap.Parent = widgets[widgets.Length - 1].BackRect;

            textGTController = new TextWidget(wr);
            textGTController.Color = UserConfig.MainScheme.Text;
            textGTController.Offset = new Point(0, 5);
            textGTController.Height = 32;
            textGTController.OffsetAlignY = Alignment.BOTTOM;
            textGTController.Parent = textMap;

            SetWidgetData(gPresets[0]);

            DevConsole.OnNewCommand += DevConsole_OnNewCommand;
            KeyboardEventDispatcher.OnKeyPressed += OnKeyPressed;
            MouseEventDispatcher.OnMousePress += OnMP;
        }
        public override void OnExit(GameTime gameTime) {
            MouseEventDispatcher.OnMousePress -= OnMP;
            DevConsole.OnNewCommand -= DevConsole_OnNewCommand;
            KeyboardEventDispatcher.OnKeyPressed -= OnKeyPressed;
            DevConsole.Deactivate();
            if(game.LoadScreen.LoadFile == null) {
                BuildELDFromWidgets();
                game.LoadScreen.LoadData = eld;
            }

            menuPresets.Dispose();
            foreach(var w in widgets) w.Dispose();
            wr.Dispose();

            // Clear Init Info
            schemes.Clear();
            schemes = null;
        }

        public void SetUserPlayer(int team, string name) {

        }

        public override void Update(GameTime gameTime) {

        }
        public override void Draw(GameTime gameTime) {
            G.Clear(Color.Black);

            wr.Draw(SB);

            game.DrawDevConsole();
            game.DrawMouse();
        }

        private void BuildELDFromWidgets() {
            for(int i = 0; i < widgets.Length; i++) {
                eld.Teams[i].Colors = schemes[widgets[i].Scheme];
                eld.Teams[i].Race = widgets[i].Race;
                eld.Teams[i].PlayerName = widgets[i].TextUser.Text;
                switch(widgets[i].TextPlayerType.Text.ToLower()) {
                    case "player":
                        eld.Teams[i].InputType = RTSInputType.Player;
                        break;
                    case "computer":
                        eld.Teams[i].InputType = RTSInputType.AI;
                        break;
                    case "environment":
                        eld.Teams[i].InputType = RTSInputType.Environment;
                        break;
                    default:
                        eld.Teams[i].InputType = RTSInputType.None;
                        break;
                }
                eld.Teams[i].InputController = inputTypes[i];
                eld.Teams[i].InputInitArgs = inputInitArgs[i];
            }
            eld.MapFile = new FileInfo(textMap.Text);
            eld.GTController = textGTController.Text;
        }
        private void SetWidgetData(GamePreset gp) {
            for(int i = 0; i < gp.Races.Length; i++) {
                widgets[i].PlayerType = gp.PlayerTypes[i];
                widgets[i].Race = gp.Races[i];
                widgets[i].Scheme = gp.Colors[i];
            }
            game.LoadScreen.ImageFile = gp.LoadImage;
            textGTController.Text = gp.GameType;
            textMap.Text = gp.Map;
            gp.InputTypes.CopyTo(inputTypes, 0);
            gp.InputInitArgs.CopyTo(inputInitArgs, 0);
        }

        void OnKeyPressed(object sender, KeyEventArgs args) {
            switch(args.KeyCode) {
                case Keys.Enter:
                    if(!DevConsole.IsActivated)
                        State = ScreenState.ChangeNext;
                    break;
                case Keys.P:
                case Keys.Back:
                    if(!DevConsole.IsActivated)
                        State = ScreenState.ChangePrevious;
                    break;
                case DevConsole.ACTIVATION_KEY:
                    if(DevConsole.IsActivated)
                        DevConsole.Deactivate();
                    else
                        DevConsole.Activate();
                    break;
            }
        }
        void OnMP(Vector2 pos, MouseButton button) {
            Point p = new Point((int)pos.X, (int)pos.Y);
            string gps = menuPresets.GetSelection(p.X, p.Y);
            if(gps != null) {
                foreach(var gp in gPresets) {
                    if(gp.Name.Equals(gps)) {
                        SetWidgetData(gp);
                        return;
                    }
                }
            }
        }
        void DevConsole_OnNewCommand(string obj) {
            Match m;
            if((m = rgxLoadGame.Match(obj)).Success) {
                FileInfo fiLoadGame = RegexHelper.ExtractFile(m);
                if(fiLoadGame.Exists) {
                    game.LoadScreen.LoadFile = fiLoadGame;
                    State = ScreenState.ChangeNext;
                }
            }
        }
    }
}
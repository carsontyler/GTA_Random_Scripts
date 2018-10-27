using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GTA;
using GTA.Native;
using GTA.Math;

namespace GTA_Random_Scripts
{
    public class Class1 : Script
    {
        bool isNeverWantedOn;
        Keys noWantedLevelKey;
        Keys resetLevelKey;
        Keys spawnCarKey;
        Keys increaseLevelKey;
        Keys decreaseLevelKey;
        Keys spawnCustomCar;
        Keys spawnRandomPed;
        int spawnCarMult = 0; // Determines how far away a car spawns. Used to prevent cars from spawning on top of each other
        int currentLevel = 0; // Tracks current wanted level

        List<VehicleHash> cars = new List<VehicleHash>(); // List of possible car models
        List<PedHash> peds = new List<PedHash>(); // List of possible ped models
        public Class1()
        {
            LoadSeetings();
            Interval = 10;

            Tick += OnTick;
            this.KeyDown += OnKeyDown;
        }

        private void OnTick(object sender, EventArgs e)
        {
            // Continually resets wanted level to 0 depsite it rising. Interval could be shorter
            if (isNeverWantedOn)
            {
                Game.Player.WantedLevel = 0;
            }
            currentLevel = Game.Player.WantedLevel;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != spawnCarKey)
                spawnCarMult = 0;

            if (Function.Call<bool>(Hash.IS_PED_IN_ANY_VEHICLE, Game.Player, true))
                spawnCarMult = 0;
            // Resets wanted level to 0
            if (e.KeyCode == resetLevelKey)
            {
                if (Game.Player.WantedLevel == 0)
                    UI.ShowSubtitle("You have no wanted level!", 3000);
                else
                {
                    Game.Player.WantedLevel = 0;
                    UI.ShowSubtitle("Your wanted level is now 0!", 3000);
                }
            }
            // Increases wanted level by 1
            if (e.KeyCode == increaseLevelKey)
            {
                currentLevel++;
                if (Game.Player.WantedLevel >= 5)
                {
                    UI.ShowSubtitle("You've reached the max level!", 3000);
                    currentLevel = 5;
                }
                else
                {
                    Game.Player.WantedLevel = currentLevel;
                    UI.ShowSubtitle("Your wanted level is now " + currentLevel + "!", 3000);
                }
            }
            // Decreases wanted level by 1
            if (e.KeyCode == decreaseLevelKey)
            {
                currentLevel--;
                if (Game.Player.WantedLevel <= 0)
                {
                    UI.ShowSubtitle("You've reached the lowest level!", 3000);
                    currentLevel = 0;
                }
                else
                {
                    Game.Player.WantedLevel = currentLevel;
                    UI.ShowSubtitle("Your wanted level is now " + currentLevel + "!", 3000);

                }
            }
            // Starts/Ends Never Wanted
            if (e.KeyCode == noWantedLevelKey)
            {
                isNeverWantedOn = !isNeverWantedOn;
                if (isNeverWantedOn)
                    UI.ShowSubtitle("Never Wanted Turned On", 3000);
                else
                    UI.ShowSubtitle("Never Wanted Turned Off", 3000);
            }
            // Spawns a random car
            if (e.KeyCode == spawnCarKey)
            {
                spawnCarMult++;
                SpawnCar("");
            }
            // Spawns a user-inputted car
            if (e.KeyCode == spawnCustomCar)
            {
                WindowTitle temp = new WindowTitle();
                var carModel = Game.GetUserInput(temp, "Enter a Car Model", 15);
                spawnCarMult++;
                SpawnCar(carModel);
            }
            // Spawns a random ped
            if (e.KeyCode == spawnRandomPed)
            {
                SpawnPed();
            }
        }

        void SpawnCar(string carModel)
        {
            Random rnd = new Random();
            Vector3 spawnPos = Game.Player.Character.GetOffsetInWorldCoords(new Vector3(0, (5 * spawnCarMult), 0));
            Vehicle vehicle = null;

            vehicle = World.CreateVehicle(cars[rnd.Next(0, cars.Count())], spawnPos);

            while (vehicle.DisplayName == "CARNOTFOUND")
                vehicle = World.CreateVehicle(cars[rnd.Next(0, cars.Count())], spawnPos);

            vehicle.PlaceOnGround();

            vehicle.NumberPlate = "CARSON#1";
            //vehicle.CanTiresBurst = false;
            vehicle.CustomPrimaryColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));
            vehicle.CustomSecondaryColor = Color.FromArgb(rnd.Next(256), rnd.Next(256), rnd.Next(256));

            Function.Call(Hash.SET_VEHICLE_MOD_KIT, vehicle.Handle, 0);
            //try
            //{
            //    vehicle.SetMod(VehicleMod.Armor, 4, true);
            //    vehicle.SetMod(VehicleMod.Brakes, 2, true);
            //    vehicle.SetMod(VehicleMod.FrontBumper, rnd.Next(1, 7), true);
            //    vehicle.SetMod(VehicleMod.RearBumper, rnd.Next(1, 7), true);
            //    vehicle.SetMod(VehicleMod.Engine, 3, true);
            //    vehicle.SetMod(VehicleMod.Hood, rnd.Next(1, 7), true);
            //    vehicle.SetMod(VehicleMod.Suspension, rnd.Next(1, 7), true);
            //    vehicle.SetMod(VehicleMod.Spoilers, rnd.Next(1, 7), true);
            //    vehicle.SetMod(VehicleMod.SideSkirt, rnd.Next(1, 7), true);
            //}
            //catch (Exception e)
            //{
            //    UI.Notify(e.Message);
            //}

            foreach (VehicleMod mod in Enum.GetValues(typeof(VehicleMod)))
            {
                try
                {
                    vehicle.SetMod(mod, rnd.Next(1, 7), true);
                }
                catch
                {
                    continue;
                }
            }
            UI.ShowSubtitle(vehicle.DisplayName + " spawned");
        }

        void SpawnPed()
        {
            try
            {
                UI.ShowSubtitle("CAUTION: Spamming will crash your game!");
                Random rnd = new Random();
                Vector3 spawnPos = Game.Player.Character.GetOffsetInWorldCoords(new Vector3(0, 2, 0));
                var ped = World.CreatePed(peds[rnd.Next(peds.Count)], spawnPos);
                ped.IsEnemy = false;
                UI.Notify(ped.ToString() + " spawned");

            }
            catch (Exception e)
            {
                UI.Notify("WARNING: ERROR");
                UI.Notify(e.Message);
            }
        }

        void LoadSeetings()
        {
            try
            {
                // Load from INI file
                ScriptSettings config = ScriptSettings.Load(@".\scripts\INI.ini");

                noWantedLevelKey = config.GetValue<Keys>("SECTION", "NOWANTEDLEVEL", Keys.L);
                resetLevelKey = config.GetValue<Keys>("SECTION", "RESETLEVELKEY", Keys.J);
                spawnCarKey = config.GetValue<Keys>("SECTION", "SPAWNCARKEY", Keys.U);
                increaseLevelKey = config.GetValue<Keys>("SECTION", "INCREASELEVELKEY", Keys.Right);
                decreaseLevelKey = config.GetValue<Keys>("SECTION", "DECREASELEVELKEY", Keys.Left);
                spawnCustomCar = config.GetValue<Keys>("SECTION", "SPAWNCUSTOMCAR", Keys.LControlKey);
                spawnRandomPed = config.GetValue<Keys>("SECTION", "SPAWNRANDOMPED", Keys.I);

                string[] exclude = { /*Planes*/"AlphaZ1", "Blimp", "Blimp2", "Avenger", "Besra", "CargoPlane", "Cuban800", "Dodo", "Duster", "Howard", "Hydra", "Jet", "Starling", "Luxor", "Luxor2", "Mammatus", "Miljet", "Mogul", "Nimbus", "Nokota", "Pyro", "Bombushka", "Rogue", "Seabreeze", "Shamal", "Titan", "Tula", "Molotok", "Velum", "Velum2", "Vestra", "Volatol","Lazer",
                    /*Helicopters*/"Akula","Annihilator","Buzzard","Buzzard2","Cargobob","Cargobob2","Cargobob3","Cargobob4","Hunter","Frogger","Frogger2","Havok","Maverick","Savage","SeaSparrow","Skylift","Supervolito","Supervolito2","Swift","Swift2","Valkyrie","Valkyrie2","Volatus","Polmav",
                    /*Boats*/"Dinghy","Dinghy2","Dinghy3","Dinghy4","Jetmax","Marquis","Seashark","Seashark2","Seashark3","Speeder","Speeder2","Squalo","Submersible","Submersible2","Suntrap","Toro","Toro2","Tropic","Tropic2","Tug","Predator",
                    /*Trailers*/"ArmyTrailer","ArmyTrailer2","ArmyTanker","Baletrailer","BoatTrailer","DockTrailer","GrainTrailer","PropTrailer","RakeTrailer","TrailerSmall","TrailerLarge","TrailerLogs","Trailers3","TrailerSmall2","TVTrailer","Trailers2","Trailers4","Trailers","FreightTrailer","Flatbed","FeightCont1","FreightCont2","Freight","Trailer" };

                // Populate possible vehicle models. Excludes models in array above
                foreach (VehicleHash car in Enum.GetValues(typeof(VehicleHash)))
                {
                    if (!exclude.Contains(car.ToString()))
                    {
                        cars.Add(car);
                    }
                }
                // Populate possible ped models. No models excluded so far.
                foreach (PedHash ped in Enum.GetValues(typeof(PedHash)))
                {
                    peds.Add(ped);
                }
            }
            catch (Exception e)
            {
                UI.Notify(e.Message);
            }
        }
    }
}
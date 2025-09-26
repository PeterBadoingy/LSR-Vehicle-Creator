using Rage;
using Rage.Attributes;
using Rage.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Xml.Linq;

[assembly: Plugin("Los Santos Red Vehicle Creator", Description = "Creates Vehicle XML Output", Author = "PeterBadoingy")]

namespace VehicleDataExtractor
{
    public class DispatchableVehicle
    {
        public string DebugName { get; set; }
        public string ModelName { get; set; }
        public string RequiredPedGroup { get; set; } = "";
        public string GroupName { get; set; } = "";
        public int MinOccupants { get; set; } = 1;
        public int MaxOccupants { get; set; }
        public int AmbientSpawnChance { get; set; } = 75;
        public int WantedSpawnChance { get; set; } = 75;
        public int MinWantedLevelSpawn { get; set; } = 0;
        public int MaxWantedLevelSpawn { get; set; } = 6;
        public int AdditionalSpawnChanceOffRoad { get; set; } = 0;
        public int RequiredPrimaryColorID { get; set; } = -1;
        public int RequiredSecondaryColorID { get; set; } = -1;
        public int RequiredPearlescentColorID { get; set; } = -1;
        public int RequiredWindowTintID { get; set; } = -1;
        public int RequiredInteriorColorID { get; set; } = -1;
        public int RequiredDashColorID { get; set; } = -1;
        public int RequiredWheelColorID { get; set; } = -1;
        public string RequiredLiveries { get; set; } = "";
        public float MaxRandomDirtLevel { get; set; } = 5f;
        public int ForcedPlateType { get; set; } = -1;
        public VehicleVariation RequiredVariation { get; set; }
        public bool RequiresDLC { get; set; }
        public int FirstPassengerIndex { get; set; } = 0;
        public bool RequiredGroupIsDriverOnly { get; set; } = false;
        public bool MatchDashColorToBaseColor { get; set; } = false;
        public bool MatchInteriorColorToBaseColor { get; set; } = false;
        public int WheelType { get; set; } = -1;
        public bool SetRandomCustomization { get; set; } = false;
        public int RandomCustomizationPercentage { get; set; } = 0;
    }

    public class VehicleVariation
    {
        public int PrimaryColor { get; set; }
        public int SecondaryColor { get; set; }
        public bool IsPrimaryColorCustom { get; set; } = false;
        public string CustomPrimaryColor { get; set; } = "";
        public bool IsSecondaryColorCustom { get; set; } = false;
        public string CustomSecondaryColor { get; set; } = "";
        public int PearlescentColor { get; set; }
        public int InteriorColor { get; set; }
        public int DashboardColor { get; set; }
        public int WheelColor { get; set; }
        public int Mod1PaintType { get; set; } = 0;
        public int Mod1Color { get; set; } = 0;
        public int Mod1PearlescentColor { get; set; } = 0;
        public int Mod2PaintType { get; set; } = 0;
        public int Mod2Color { get; set; } = 0;
        public int Livery { get; set; } = -1;
        public int Livery2 { get; set; } = -1;
        public int WheelType { get; set; }
        public int WindowTint { get; set; }
        public bool HasCustomWheels { get; set; } = false;
        public List<VehicleExtra> VehicleExtras { get; set; } = new List<VehicleExtra>();
        public List<VehicleToggle> VehicleToggles { get; set; } = new List<VehicleToggle>();
        public List<VehicleMod> VehicleMods { get; set; } = new List<VehicleMod>();
        public string VehicleNeons { get; set; } = "";
        public float FuelLevel { get; set; } = 65f;
        public float DirtLevel { get; set; } = 0f;
        public bool HasInvicibleTires { get; set; } = false;
        public bool IsTireSmokeColorCustom { get; set; } = false;
        public int TireSmokeColorR { get; set; } = 0;
        public int TireSmokeColorG { get; set; } = 0;
        public int TireSmokeColorB { get; set; } = 0;
        public int NeonColorR { get; set; } = 0;
        public int NeonColorG { get; set; } = 0;
        public int NeonColorB { get; set; } = 0;
        public int XenonLightColor { get; set; } = -1;
    }

    public class VehicleExtra
    {
        public int ID { get; set; }
        public bool IsTurnedOn { get; set; }
        public VehicleExtra() { }
        public VehicleExtra(int id, bool isTurnedOn)
        {
            ID = id;
            IsTurnedOn = isTurnedOn;
        }
    }

    public class VehicleToggle
    {
        public int ID { get; set; }
        public bool IsTurnedOn { get; set; }
        public VehicleToggle() { }
        public VehicleToggle(int id, bool isTurnedOn)
        {
            ID = id;
            IsTurnedOn = isTurnedOn;
        }
    }

    public class VehicleMod
    {
        public int ID { get; set; }
        public int Output { get; set; }
        public VehicleMod() { }
        public VehicleMod(int id, int output)
        {
            ID = id;
            Output = output;
        }
    }

    public static class VehicleDataExtractor
    {
        private static bool isRunning = true;
        private static readonly string outputPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins", "LosSantosRED", "VehicleData.xml");

        [STAThread]
        public static void Main()
        {
            Initialize();
        }

        public static void Initialize()
        {
            Game.DisplayNotification("Los Santos Red Vehicle Creator Loaded. Press F9 to extract data.");
            GameFiber.StartNew(() =>
            {
                while (isRunning)
                {
                    if (Game.IsKeyDown(System.Windows.Forms.Keys.F9))
                    {
                        try
                        {
                            string output = ExtractVehicleData();
                            if (!string.IsNullOrEmpty(output))
                            {
                                try
                                {
                                    AppendVehicleDataToFile(output);
                                    Game.DisplayNotification($"Vehicle data appended to {outputPath}");
                                }
                                catch (Exception ex)
                                {
                                    Game.LogTrivial($"File Write Error: {ex.Message}\n{ex.StackTrace}");
                                    Game.DisplayNotification($"Error saving to file: {ex.Message}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Game.LogTrivial($"VehicleDataExtractor Error: {ex.Message}\n{ex.StackTrace}");
                            Game.DisplayNotification($"Error extracting vehicle data: {ex.Message}");
                        }
                    }
                    GameFiber.Yield();
                }
            }, "VehicleDataExtractor");
        }

        private static void AppendVehicleDataToFile(string vehicleData)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
            XDocument doc;
            if (File.Exists(outputPath))
            {
                try
                {
                    doc = XDocument.Load(outputPath);
                }
                catch
                {
                    doc = new XDocument(new XElement("DispatchableVehicles"));
                }
            }
            else
            {
                doc = new XDocument(new XElement("DispatchableVehicles"));
            }

            var newVehicle = XDocument.Parse(vehicleData).Root;
            doc.Root.Add(newVehicle);
            doc.Save(outputPath);
        }

        private static bool IsVehicleValid(Vehicle vehicle)
        {
            return vehicle != null && vehicle.Exists() && vehicle.IsValid() && NativeFunction.Natives.IS_ENTITY_A_VEHICLE<bool>(vehicle) && vehicle.Handle != 0;
        }

        private static string ExtractVehicleData()
        {
            var player = Game.LocalPlayer.Character;
            if (!player.IsInAnyVehicle(false))
            {
                Game.DisplayNotification("You must be in a vehicle to extract data.");
                return "";
            }

            var vehicle = player.CurrentVehicle;
            if (!IsVehicleValid(vehicle))
            {
                Game.DisplayNotification("No valid vehicle found.");
                return "";
            }

            // Yield to stabilize native calls
            GameFiber.Yield();

            int primaryColor = 0, secondaryColor = 0;
            try
            {
                NativeFunction.Natives.GET_VEHICLE_COLOURS<int>(vehicle, ref primaryColor, ref secondaryColor);
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"Failed to get vehicle colors: {ex.Message}");
                primaryColor = secondaryColor = -1;
            }

            int interiorColor = 0;
            try
            {
                if (IsVehicleValid(vehicle))
                {
                    NativeFunction.Natives.GET_VEHICLE_EXTRA_COLOUR_5<int>(vehicle, ref interiorColor);
                }
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"Failed to get interior color: {ex.Message}");
                interiorColor = -1;
            }

            int dashboardColor = 0;
            try
            {
                if (IsVehicleValid(vehicle))
                {
                    NativeFunction.Natives.GET_VEHICLE_EXTRA_COLOUR_6<int>(vehicle, ref dashboardColor);
                }
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"Failed to get dashboard color: {ex.Message}");
                dashboardColor = -1;
            }

            int pearlescentColor = 0, wheelColor = 0;
            try
            {
                if (IsVehicleValid(vehicle))
                {
                    NativeFunction.Natives.GET_VEHICLE_EXTRA_COLOURS<int>(vehicle, ref pearlescentColor, ref wheelColor);
                }
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"Failed to get extra colors: {ex.Message}");
                pearlescentColor = wheelColor = -1;
            }

            int primaryColorID = MapPaintIndexToColorID(primaryColor);
            int secondaryColorID = MapPaintIndexToColorID(secondaryColor);
            int interiorColorID = MapPaintIndexToColorID(interiorColor);
            int dashboardColorID = MapPaintIndexToColorID(dashboardColor);
            int wheelColorID = MapPaintIndexToColorID(wheelColor);
            int pearlescentColorID = MapPaintIndexToColorID(pearlescentColor);

            bool hasInvicibleTires = false;
            try
            {
                if (IsVehicleValid(vehicle))
                {
                    hasInvicibleTires = !NativeFunction.Natives.GET_VEHICLE_TYRES_CAN_BURST<bool>(vehicle);
                }
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"Failed to get tire burst status: {ex.Message}");
            }

            int seatCount = 0;
            try
            {
                seatCount = NativeFunction.Natives.GET_VEHICLE_MODEL_NUMBER_OF_SEATS<int>(vehicle.Model.Hash);
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"Failed to get seat count: {ex.Message}");
            }

            int livery = -1, livery2 = -1;
            try
            {
                livery = NativeFunction.Natives.GET_VEHICLE_LIVERY<int>(vehicle);
                livery2 = NativeFunction.Natives.GET_VEHICLE_LIVERY2<int>(vehicle);
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"Failed to get livery: {ex.Message}");
            }

            int tireSmokeR = 0, tireSmokeG = 0, tireSmokeB = 0;
            bool isTireSmokeColorCustom = false;
            try
            {
                if (IsVehicleValid(vehicle))
                {
                    int tireSmokeMod = NativeFunction.Natives.GET_VEHICLE_MOD<int>(vehicle, 20);
                    if (tireSmokeMod >= 0)
                    {
                        NativeFunction.Natives.GET_VEHICLE_TYRE_SMOKE_COLOR<int>(vehicle, ref tireSmokeR, ref tireSmokeG, ref tireSmokeB);
                        isTireSmokeColorCustom = tireSmokeR != 0 || tireSmokeG != 0 || tireSmokeB != 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"Failed to get tire smoke color: {ex.Message}");
                tireSmokeR = tireSmokeG = tireSmokeB = 0;
                isTireSmokeColorCustom = false;
            }

            string vehicleNeons = "";
            try
            {
                if (IsVehicleValid(vehicle) && vehicle.Handle != 0)
                {
                    bool hasNeons = false;
                    try
                    {
                        hasNeons = NativeFunction.Natives.DOES_VEHICLE_HAVE_NEON_LIGHTS<bool>(vehicle);
                    }
                    catch
                    {
                        hasNeons = false;
                    }
                    if (hasNeons)
                    {
                        bool anyNeonEnabled = false;
                        for (int i = 0; i <= 3; i++)
                        {
                            if (NativeFunction.Natives.IS_VEHICLE_NEON_LIGHT_ENABLED<bool>(vehicle, i))
                            {
                                anyNeonEnabled = true;
                                break;
                            }
                        }
                        if (anyNeonEnabled)
                        {
                            vehicleNeons = "Left,Right,Front,Back";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"Failed to get neon lights: {ex.Message}");
            }

            int neonR = 0, neonG = 0, neonB = 0;
            try
            {
                if (!string.IsNullOrEmpty(vehicleNeons) && vehicle.Handle != 0)
                {
                    NativeFunction.Natives.GET_VEHICLE_NEON_LIGHTS_COLOUR<int>(vehicle, ref neonR, ref neonG, ref neonB);
                }
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"Failed to get neon colors: {ex.Message}");
            }

            int xenonLightColor = -1;
            try
            {
                if (IsVehicleValid(vehicle) && vehicle.Handle != 0)
                {
                    bool hasXenon = false;
                    try
                    {
                        hasXenon = NativeFunction.Natives.DOES_VEHICLE_HAVE_XENON_LIGHTS<bool>(vehicle);
                    }
                    catch
                    {
                        hasXenon = false;
                    }
                    if (hasXenon)
                    {
                        xenonLightColor = NativeFunction.Natives.GET_VEHICLE_XENON_LIGHTS_COLOR<int>(vehicle);
                    }
                }
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"Failed to get xenon light color: {ex.Message}");
            }

            float dirtLevel = 0f;
            try
            {
                dirtLevel = NativeFunction.Natives.GET_VEHICLE_DIRT_LEVEL<float>(vehicle);
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"Failed to get dirt level: {ex.Message}");
            }

            int windowTint = -1;
            try
            {
                windowTint = NativeFunction.Natives.GET_VEHICLE_WINDOW_TINT<int>(vehicle);
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"Failed to get window tint: {ex.Message}");
            }

            int wheelType = -1;
            try
            {
                wheelType = NativeFunction.Natives.GET_VEHICLE_WHEEL_TYPE<int>(vehicle);
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"Failed to get wheel type: {ex.Message}");
            }

            var data = new DispatchableVehicle
            {
                DebugName = $"{vehicle.Model.Name}_CustomVehicle_{DateTime.Now:yyyyMMdd_HHmmss}",
                ModelName = vehicle.Model.Name.ToUpper(),
                MinOccupants = 1,
                MaxOccupants = seatCount,
                RequiresDLC = IsDLCVehicle(vehicle.Model.Hash),
                RequiredPrimaryColorID = primaryColorID,
                RequiredSecondaryColorID = secondaryColorID,
                RequiredPearlescentColorID = pearlescentColorID,
                RequiredWindowTintID = windowTint,
                RequiredInteriorColorID = interiorColorID,
                RequiredDashColorID = dashboardColorID,
                RequiredWheelColorID = wheelColorID,
                RequiredLiveries = livery != -1 ? livery.ToString() : "",
                RequiredVariation = new VehicleVariation
                {
                    PrimaryColor = primaryColorID,
                    SecondaryColor = secondaryColorID,
                    InteriorColor = interiorColorID,
                    DashboardColor = dashboardColorID,
                    WheelColor = wheelColorID,
                    PearlescentColor = pearlescentColorID,
                    WheelType = wheelType,
                    WindowTint = windowTint,
                    Livery = livery,
                    Livery2 = livery2,
                    VehicleExtras = GetVehicleExtras(vehicle),
                    VehicleToggles = GetVehicleToggles(vehicle),
                    VehicleMods = GetVehicleMods(vehicle),
                    VehicleNeons = vehicleNeons,
                    DirtLevel = dirtLevel,
                    HasInvicibleTires = hasInvicibleTires,
                    IsTireSmokeColorCustom = isTireSmokeColorCustom,
                    TireSmokeColorR = tireSmokeR,
                    TireSmokeColorG = tireSmokeG,
                    TireSmokeColorB = tireSmokeB,
                    NeonColorR = neonR,
                    NeonColorG = neonG,
                    NeonColorB = neonB,
                    XenonLightColor = xenonLightColor
                }
            };

            return FormatVehicleData(data);
        }

        private static int MapPaintIndexToColorID(int paintIndex)
        {
            var colorMap = new Dictionary<int, int>
            {
                { 0, 0 }, { 1, 1 }, { 2, 2 }, { 3, 3 }, { 4, 4 }, { 5, 5 }, { 6, 6 }, { 7, 7 }, { 8, 8 }, { 9, 9 },
                { 10, 10 }, { 11, 11 }, { 12, 12 }, { 13, 13 }, { 14, 14 }, { 15, 15 }, { 16, 16 }, { 17, 17 },
                { 18, 18 }, { 19, 19 }, { 20, 20 }, { 21, 21 }, { 22, 22 }, { 23, 23 }, { 24, 24 }, { 25, 25 },
                { 26, 26 }, { 27, 27 }, { 28, 28 }, { 29, 29 }, { 30, 30 }, { 31, 31 }, { 32, 32 }, { 33, 33 },
                { 34, 34 }, { 35, 35 }, { 36, 36 }, { 37, 37 }, { 38, 38 }, { 39, 39 }, { 40, 40 }, { 41, 41 },
                { 42, 42 }, { 43, 43 }, { 44, 44 }, { 45, 45 }, { 46, 46 }, { 47, 47 }, { 48, 48 }, { 49, 49 },
                { 50, 50 }, { 51, 51 }, { 52, 52 }, { 53, 53 }, { 54, 54 }, { 55, 55 }, { 56, 56 }, { 57, 57 },
                { 58, 58 }, { 59, 59 }, { 60, 60 }, { 61, 61 }, { 62, 62 }, { 63, 63 }, { 64, 64 }, { 65, 65 },
                { 66, 66 }, { 67, 67 }, { 68, 68 }, { 69, 69 }, { 70, 70 }, { 71, 71 }, { 72, 72 }, { 73, 73 },
                { 74, 74 }, { 75, 75 }, { 76, 76 }, { 77, 77 }, { 78, 78 }, { 79, 79 }, { 80, 80 }, { 81, 81 },
                { 82, 82 }, { 83, 83 }, { 84, 84 }, { 85, 85 }, { 86, 86 }, { 87, 87 }, { 88, 88 }, { 89, 89 },
                { 90, 90 }, { 91, 91 }, { 92, 92 }, { 93, 93 }, { 94, 94 }, { 95, 95 }, { 96, 96 }, { 97, 97 },
                { 98, 98 }, { 99, 99 }, { 100, 100 }, { 101, 101 }, { 102, 102 }, { 103, 103 }, { 104, 104 },
                { 105, 105 }, { 106, 106 }, { 107, 107 }, { 108, 108 }, { 109, 109 }, { 110, 110 }, { 111, 111 },
                { 112, 112 }, { 113, 113 }, { 114, 114 }, { 115, 115 }, { 116, 116 }, { 117, 117 }, { 118, 118 },
                { 119, 119 }, { 120, 120 }, { 121, 121 }, { 122, 122 }, { 123, 123 }, { 124, 124 }, { 125, 125 },
                { 126, 126 }, { 127, 127 }, { 128, 128 }, { 129, 129 }, { 130, 130 }, { 131, 131 }, { 132, 132 },
                { 133, 133 }, { 134, 134 }, { 135, 135 }, { 136, 136 }, { 137, 137 }, { 138, 138 }, { 139, 139 },
                { 140, 140 }, { 141, 141 }, { 142, 142 }, { 143, 143 }, { 144, 144 }, { 145, 145 }, { 146, 146 },
                { 147, 147 }, { 148, 148 }, { 149, 149 }, { 150, 150 }, { 151, 151 }, { 152, 152 }, { 153, 153 },
                { 154, 154 }, { 155, 155 }, { 156, 156 }, { 157, 157 }, { 158, 158 }, { 159, 159 }, { 160, 160 }
            };
            return colorMap.ContainsKey(paintIndex) ? colorMap[paintIndex] : -1;
        }

        private static bool IsDLCVehicle(uint vehicleHash)
        {
            try
            {
                int numDLCVehicles = NativeFunction.Natives.GET_NUM_DLC_VEHICLES<int>();
                for (int i = 0; i < numDLCVehicles; i++)
                {
                    IntPtr outData = Marshal.AllocHGlobal(24);
                    try
                    {
                        bool success = NativeFunction.Natives.GET_DLC_VEHICLE_DATA<bool>(i, outData);
                        if (success)
                        {
                            uint dlcHash = (uint)Marshal.ReadInt64(outData, 8);
                            if (dlcHash == vehicleHash)
                            {
                                return true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Game.LogTrivial($"Failed to check DLC vehicle: {ex.Message}");
                    }
                    finally
                    {
                        if (outData != IntPtr.Zero)
                            Marshal.FreeHGlobal(outData);
                    }
                }
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"Failed to check DLC vehicles: {ex.Message}");
            }
            return false;
        }

        private static List<VehicleExtra> GetVehicleExtras(Vehicle vehicle)
        {
            var extras = new List<VehicleExtra>();
            try
            {
                if (IsVehicleValid(vehicle))
                {
                    for (int i = 1; i <= 12; i++)
                    {
                        if (NativeFunction.Natives.DOES_EXTRA_EXIST<bool>(vehicle, i) &&
                            NativeFunction.Natives.IS_VEHICLE_EXTRA_TURNED_ON<bool>(vehicle, i))
                        {
                            extras.Add(new VehicleExtra(i, true));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"Failed to get vehicle extras: {ex.Message}");
            }
            return extras;
        }

        private static List<VehicleToggle> GetVehicleToggles(Vehicle vehicle)
        {
            var toggles = new List<VehicleToggle>();
            try
            {
                if (IsVehicleValid(vehicle))
                {
                    for (int i = 17; i <= 22; i++)
                    {
                        bool isToggledOn = NativeFunction.Natives.IS_TOGGLE_MOD_ON<bool>(vehicle, i);
                        if (isToggledOn)
                        {
                            toggles.Add(new VehicleToggle(i, isToggledOn));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"Failed to get vehicle toggles: {ex.Message}");
            }
            return toggles;
        }

        private static List<VehicleMod> GetVehicleMods(Vehicle vehicle)
        {
            var mods = new List<VehicleMod>();
            try
            {
                if (IsVehicleValid(vehicle))
                {
                    for (int i = 0; i <= 48; i++)
                    {
                        if (i >= 17 && i <= 22) continue;
                        int modIndex = NativeFunction.Natives.GET_VEHICLE_MOD<int>(vehicle, i);
                        if (modIndex >= 0)
                        {
                            mods.Add(new VehicleMod(i, modIndex));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Game.LogTrivial($"Failed to get vehicle mods: {ex.Message}");
            }
            return mods;
        }

        private static string FormatVehicleData(DispatchableVehicle data)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<DispatchableVehicle>");
            sb.AppendLine($"    <DebugName>{SecurityElement.Escape(data.DebugName)}</DebugName>");
            sb.AppendLine($"    <ModelName>{SecurityElement.Escape(data.ModelName)}</ModelName>");
            sb.AppendLine(data.RequiredPedGroup == "" ? "    <RequiredPedGroup />" : $"    <RequiredPedGroup>{SecurityElement.Escape(data.RequiredPedGroup)}</RequiredPedGroup>");
            sb.AppendLine(data.GroupName == "" ? "    <GroupName />" : $"    <GroupName>{SecurityElement.Escape(data.GroupName)}</GroupName>");
            sb.AppendLine($"    <MinOccupants>{data.MinOccupants}</MinOccupants>");
            sb.AppendLine($"    <MaxOccupants>{data.MaxOccupants}</MaxOccupants>");
            sb.AppendLine($"    <AmbientSpawnChance>{data.AmbientSpawnChance}</AmbientSpawnChance>");
            sb.AppendLine($"    <WantedSpawnChance>{data.WantedSpawnChance}</WantedSpawnChance>");
            sb.AppendLine($"    <MinWantedLevelSpawn>{data.MinWantedLevelSpawn}</MinWantedLevelSpawn>");
            sb.AppendLine($"    <MaxWantedLevelSpawn>{data.MaxWantedLevelSpawn}</MaxWantedLevelSpawn>");
            //sb.AppendLine($"    <AdditionalSpawnChanceOffRoad>{data.AdditionalSpawnChanceOffRoad}</AdditionalSpawnChanceOffRoad>");
            //sb.AppendLine($"    <RequiredPrimaryColorID>{data.RequiredPrimaryColorID}</RequiredPrimaryColorID>");
            //sb.AppendLine($"    <RequiredSecondaryColorID>{data.RequiredSecondaryColorID}</RequiredSecondaryColorID>");
            //sb.AppendLine($"    <RequiredPearlescentColorID>{data.RequiredPearlescentColorID}</RequiredPearlescentColorID>");
            //sb.AppendLine($"    <RequiredWindowTintID>{data.RequiredWindowTintID}</RequiredWindowTintID>");
            //sb.AppendLine($"    <RequiredInteriorColorID>{data.RequiredVariation.InteriorColor}</RequiredInteriorColorID>");
            //sb.AppendLine($"    <RequiredDashColorID>{data.RequiredVariation.DashboardColor}</RequiredDashColorID>");
            //sb.AppendLine($"    <RequiredWheelColorID>{data.RequiredWheelColorID}</RequiredWheelColorID>");
            //sb.AppendLine($"    <RequiredLiveries>{SecurityElement.Escape(data.RequiredLiveries)}</RequiredLiveries>");
            //sb.AppendLine("    <VehicleExtras />");
            //sb.AppendLine("    <VehicleMods />");
            //sb.AppendLine($"    <MaxRandomDirtLevel>{data.MaxRandomDirtLevel}</MaxRandomDirtLevel>");
            //sb.AppendLine($"    <ForcedPlateType>{data.ForcedPlateType}</ForcedPlateType>");
            sb.AppendLine("    <RequiredVariation>");
            sb.AppendLine($"        <PrimaryColor>{data.RequiredVariation.PrimaryColor}</PrimaryColor>");
            sb.AppendLine($"        <SecondaryColor>{data.RequiredVariation.SecondaryColor}</SecondaryColor>");
            sb.AppendLine($"        <IsPrimaryColorCustom>{data.RequiredVariation.IsPrimaryColorCustom.ToString().ToLower()}</IsPrimaryColorCustom>");
            sb.AppendLine($"        <CustomPrimaryColor>{SecurityElement.Escape(data.RequiredVariation.CustomPrimaryColor)}</CustomPrimaryColor>");
            sb.AppendLine($"        <IsSecondaryColorCustom>{data.RequiredVariation.IsSecondaryColorCustom.ToString().ToLower()}</IsSecondaryColorCustom>");
            sb.AppendLine($"        <CustomSecondaryColor>{SecurityElement.Escape(data.RequiredVariation.CustomSecondaryColor)}</CustomSecondaryColor>");
            sb.AppendLine($"        <PearlescentColor>{data.RequiredVariation.PearlescentColor}</PearlescentColor>");
            sb.AppendLine($"        <InteriorColor>{data.RequiredVariation.InteriorColor}</InteriorColor>");
            sb.AppendLine($"        <DashboardColor>{data.RequiredVariation.DashboardColor}</DashboardColor>");
            sb.AppendLine($"        <WheelColor>{data.RequiredVariation.WheelColor}</WheelColor>");
            sb.AppendLine($"        <Mod1PaintType>{data.RequiredVariation.Mod1PaintType}</Mod1PaintType>");
            sb.AppendLine($"        <Mod1Color>{data.RequiredVariation.Mod1Color}</Mod1Color>");
            sb.AppendLine($"        <Mod1PearlescentColor>{data.RequiredVariation.Mod1PearlescentColor}</Mod1PearlescentColor>");
            sb.AppendLine($"        <Mod2PaintType>{data.RequiredVariation.Mod2PaintType}</Mod2PaintType>");
            sb.AppendLine($"        <Mod2Color>{data.RequiredVariation.Mod2Color}</Mod2Color>");
            sb.AppendLine($"        <Livery>{data.RequiredVariation.Livery}</Livery>");
            sb.AppendLine($"        <Livery2>{data.RequiredVariation.Livery2}</Livery2>");
            sb.AppendLine($"        <WheelType>{data.RequiredVariation.WheelType}</WheelType>");
            sb.AppendLine($"        <WindowTint>{data.RequiredVariation.WindowTint}</WindowTint>");
            sb.AppendLine($"        <HasCustomWheels>{data.RequiredVariation.HasCustomWheels.ToString().ToLower()}</HasCustomWheels>");
            sb.AppendLine(data.RequiredVariation.VehicleExtras.Any() ? "        <VehicleExtras>" : "        <VehicleExtras />");
            foreach (var extra in data.RequiredVariation.VehicleExtras)
            {
                sb.AppendLine("            <VehicleExtra>");
                sb.AppendLine($"                <ID>{extra.ID}</ID>");
                sb.AppendLine($"                <IsTurnedOn>{extra.IsTurnedOn.ToString().ToLower()}</IsTurnedOn>");
                sb.AppendLine("            </VehicleExtra>");
            }
            if (data.RequiredVariation.VehicleExtras.Any())
                sb.AppendLine("        </VehicleExtras>");
            sb.AppendLine(data.RequiredVariation.VehicleToggles.Any() ? "        <VehicleToggles>" : "        <VehicleToggles />");
            foreach (var toggle in data.RequiredVariation.VehicleToggles)
            {
                sb.AppendLine("            <VehicleToggle>");
                sb.AppendLine($"                <ID>{toggle.ID}</ID>");
                sb.AppendLine($"                <IsTurnedOn>{toggle.IsTurnedOn.ToString().ToLower()}</IsTurnedOn>");
                sb.AppendLine("            </VehicleToggle>");
            }
            if (data.RequiredVariation.VehicleToggles.Any())
                sb.AppendLine("        </VehicleToggles>");
            sb.AppendLine(data.RequiredVariation.VehicleMods.Any() ? "        <VehicleMods>" : "        <VehicleMods />");
            foreach (var mod in data.RequiredVariation.VehicleMods)
            {
                sb.AppendLine("            <VehicleMod>");
                sb.AppendLine($"                <ID>{mod.ID}</ID>");
                sb.AppendLine($"                <Output>{mod.Output}</Output>");
                sb.AppendLine("            </VehicleMod>");
            }
            if (data.RequiredVariation.VehicleMods.Any())
                sb.AppendLine("        </VehicleMods>");
            sb.AppendLine($"        <VehicleNeons>{SecurityElement.Escape(data.RequiredVariation.VehicleNeons)}</VehicleNeons>");
            sb.AppendLine($"        <FuelLevel>{data.RequiredVariation.FuelLevel}</FuelLevel>");
            sb.AppendLine($"        <DirtLevel>{data.RequiredVariation.DirtLevel}</DirtLevel>");
            sb.AppendLine($"        <HasInvicibleTires>{data.RequiredVariation.HasInvicibleTires.ToString().ToLower()}</HasInvicibleTires>");
            sb.AppendLine($"        <IsTireSmokeColorCustom>{data.RequiredVariation.IsTireSmokeColorCustom.ToString().ToLower()}</IsTireSmokeColorCustom>");
            sb.AppendLine($"        <TireSmokeColorR>{data.RequiredVariation.TireSmokeColorR}</TireSmokeColorR>");
            sb.AppendLine($"        <TireSmokeColorG>{data.RequiredVariation.TireSmokeColorG}</TireSmokeColorG>");
            sb.AppendLine($"        <TireSmokeColorB>{data.RequiredVariation.TireSmokeColorB}</TireSmokeColorB>");
            sb.AppendLine($"        <NeonColorR>{data.RequiredVariation.NeonColorR}</NeonColorR>");
            sb.AppendLine($"        <NeonColorG>{data.RequiredVariation.NeonColorG}</NeonColorG>");
            sb.AppendLine($"        <NeonColorB>{data.RequiredVariation.NeonColorB}</NeonColorB>");
            sb.AppendLine($"        <XenonLightColor>{data.RequiredVariation.XenonLightColor}</XenonLightColor>");
            sb.AppendLine("    </RequiredVariation>");
            sb.AppendLine($"    <RequiresDLC>{data.RequiresDLC.ToString().ToLower()}</RequiresDLC>");
            sb.AppendLine($"    <FirstPassengerIndex>{data.FirstPassengerIndex}</FirstPassengerIndex>");
            sb.AppendLine($"    <RequiredGroupIsDriverOnly>{data.RequiredGroupIsDriverOnly.ToString().ToLower()}</RequiredGroupIsDriverOnly>");
            sb.AppendLine($"    <MatchDashColorToBaseColor>{data.MatchDashColorToBaseColor.ToString().ToLower()}</MatchDashColorToBaseColor>");
            sb.AppendLine($"    <MatchInteriorColorToBaseColor>{data.MatchInteriorColorToBaseColor.ToString().ToLower()}</MatchInteriorColorToBaseColor>");
            //sb.AppendLine($"    <WheelType>{data.WheelType}</WheelType>");
            sb.AppendLine($"    <SetRandomCustomization>{data.SetRandomCustomization.ToString().ToLower()}</SetRandomCustomization>");
            sb.AppendLine($"    <RandomCustomizationPercentage>{data.RandomCustomizationPercentage}</RandomCustomizationPercentage>");
            sb.AppendLine("</DispatchableVehicle>");
            return sb.ToString();
        }

        public static void Stop()
        {
            isRunning = false;
        }
    }
}
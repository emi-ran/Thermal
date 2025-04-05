using System;
using System.Linq;
using LibreHardwareMonitor.Hardware;

namespace Thermal.Monitoring
{
    internal class HardwareMonitor : IDisposable
    {
        private Computer? computer;
        private readonly UpdateVisitor updateVisitor;

        public HardwareMonitor()
        {
            updateVisitor = new UpdateVisitor();
        }

        public bool Initialize()
        {
            try
            {
                computer = new Computer
                {
                    IsCpuEnabled = true,
                    IsGpuEnabled = true,
                    IsMemoryEnabled = false,
                    IsMotherboardEnabled = false,
                    IsControllerEnabled = false,
                    IsNetworkEnabled = false,
                    IsStorageEnabled = false
                };
                computer.Open();
                Console.WriteLine("HardwareMonitor: LibreHardwareMonitor Açıldı.");
                UpdateSensors(); // İlk okumayı yap
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"HardwareMonitor: Başlatma hatası: {ex.Message}");
                MessageBox.Show($"Donanım bilgileri okunurken hata oluştu: {ex.Message}", "Hardware Monitor Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                computer = null;
                return false;
            }
        }

        public void UpdateSensors()
        {
            computer?.Accept(updateVisitor);
        }

        public float GetCpuTemperature()
        {
            if (computer == null) return 0;

            float packageTemp = 0;
            float coreMaxTemp = 0;
            float highestCoreTemp = 0;
            bool packageFound = false;
            bool coreMaxFound = false;

            IHardware? cpu = computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);
            if (cpu == null) return 0;

            foreach (var sensor in cpu.Sensors)
            {
                if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue)
                {
                    if (sensor.Name.Contains("Package")) { packageTemp = sensor.Value.Value; packageFound = true; break; }
                    else if (sensor.Name.Contains("Core Max")) { coreMaxTemp = Math.Max(coreMaxTemp, sensor.Value.Value); coreMaxFound = true; }
                    else if (sensor.Name.Contains("Core") && !sensor.Name.Contains("Distance")) { highestCoreTemp = Math.Max(highestCoreTemp, sensor.Value.Value); }
                }
            }
            if (packageFound) return packageTemp;
            if (coreMaxFound) return coreMaxTemp;
            return highestCoreTemp;
        }

        public float GetGpuTemperature()
        {
            if (computer == null) return 0;

            IHardware? activeGpu = null;
            float highestTemp = 0;

            var gpus = computer.Hardware.Where(h => h.HardwareType == HardwareType.GpuNvidia || h.HardwareType == HardwareType.GpuAmd || h.HardwareType == HardwareType.GpuIntel).ToList();
            if (!gpus.Any()) return 0;

            IHardware? nvidiaGpu = gpus.FirstOrDefault(h => h.HardwareType == HardwareType.GpuNvidia);
            if (nvidiaGpu != null) { highestTemp = GetGpuTempFromHardware(nvidiaGpu); if (highestTemp > 0) activeGpu = nvidiaGpu; }
            if (activeGpu == null) { IHardware? intelGpu = gpus.FirstOrDefault(h => h.HardwareType == HardwareType.GpuIntel); if (intelGpu != null) { float intelTemp = GetGpuTempFromHardware(intelGpu); if (intelTemp > 0) { activeGpu = intelGpu; highestTemp = intelTemp; } } }
            if (activeGpu == null) { IHardware? amdGpu = gpus.FirstOrDefault(h => h.HardwareType == HardwareType.GpuAmd); if (amdGpu != null) { float amdTemp = GetGpuTempFromHardware(amdGpu); if (amdTemp > 0) { activeGpu = amdGpu; highestTemp = amdTemp; } } }
            return highestTemp;
        }

        private float GetGpuTempFromHardware(IHardware gpu)
        {
            if (gpu == null) return 0;
            float coreTemp = 0; float hotSpotTemp = 0; float genericTemp = 0;
            bool coreFound = false; bool hotSpotFound = false; bool genericFound = false;
            foreach (var sensor in gpu.Sensors) { if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue) { if (gpu.HardwareType == HardwareType.GpuIntel && sensor.Name.Equals("GPU Temperature", StringComparison.OrdinalIgnoreCase)) { genericTemp = Math.Max(genericTemp, sensor.Value.Value); genericFound = true; } else if (sensor.Name.Contains("GPU Core", StringComparison.OrdinalIgnoreCase)) { coreTemp = Math.Max(coreTemp, sensor.Value.Value); coreFound = true; } else if (sensor.Name.Contains("Hot Spot", StringComparison.OrdinalIgnoreCase) || sensor.Name.Contains("Junction", StringComparison.OrdinalIgnoreCase)) { hotSpotTemp = Math.Max(hotSpotTemp, sensor.Value.Value); hotSpotFound = true; } else if (!coreFound && !hotSpotFound && !genericFound) { genericTemp = Math.Max(genericTemp, sensor.Value.Value); genericFound = true; } } }
            if (hotSpotFound && hotSpotTemp > 0) return hotSpotTemp;
            if (coreFound && coreTemp > 0) return coreTemp;
            if (genericFound && genericTemp > 0) return genericTemp;
            return 0;
        }

        public void Dispose()
        {
            computer?.Close();
            Console.WriteLine("HardwareMonitor: Kapatıldı.");
        }
    }

    // UpdateVisitor sınıfı buraya veya ayrı bir dosyaya taşınabilir.
    // Şimdilik burada kalsın.
    public class UpdateVisitor : IVisitor
    {
        public void VisitComputer(IComputer computer)
        {
            computer.Traverse(this);
        }

        public void VisitHardware(IHardware hardware)
        {
            hardware.Update();
            foreach (var subHardware in hardware.SubHardware)
            {
                subHardware.Accept(this);
            }
        }

        public void VisitSensor(ISensor sensor) { }
        public void VisitParameter(IParameter parameter) { }
    }
}
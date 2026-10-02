using System;
using System.Linq;
using LibreHardwareMonitor.Hardware;

namespace Thermal.Monitoring
{
    public class HardwareMonitor : IDisposable
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
                try
                {
                    computer?.Close();
                }
                catch (Exception closeEx)
                {
                    Console.WriteLine($"HardwareMonitor: Kapatma hatası: {closeEx.Message}");
                }
                computer = null;
                return false;
            }
        }

        public void UpdateSensors()
        {
            computer?.Accept(updateVisitor);
        }

        public List<string> GetCpuNames()
        {
            if (computer == null) return new List<string>();
            return computer.Hardware
                .Where(h => h.HardwareType == HardwareType.Cpu)
                .Select(h => h.Name)
                .Distinct()
                .ToList();
        }

        public List<string> GetGpuNames()
        {
            if (computer == null) return new List<string>();
            return computer.Hardware
                .Where(h => h.HardwareType == HardwareType.GpuNvidia || h.HardwareType == HardwareType.GpuAmd || h.HardwareType == HardwareType.GpuIntel)
                .Select(h => h.Name)
                .Distinct()
                .ToList();
        }

        public float GetCpuTemperature(string preferredCpuName = "", int sensorPreference = 0)
        {
            if (computer == null) return 0;

            IHardware? cpu = null;
            if (!string.IsNullOrEmpty(preferredCpuName))
            {
                cpu = computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu && h.Name.Equals(preferredCpuName, StringComparison.OrdinalIgnoreCase));
            }
            if (cpu == null)
            {
                cpu = computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);
            }
            if (cpu == null) return 0;

            float packageTemp = 0;
            float coreMaxTemp = 0;
            float highestCoreTemp = 0;
            bool packageFound = false;
            bool coreMaxFound = false;
            bool highestCoreFound = false;

            foreach (var sensor in cpu.Sensors)
            {
                if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue && sensor.Value.Value > 0)
                {
                    float val = sensor.Value.Value;
                    if (sensor.Name.Contains("Package", StringComparison.OrdinalIgnoreCase)) { packageTemp = val; packageFound = true; }
                    else if (sensor.Name.Contains("Core Max", StringComparison.OrdinalIgnoreCase)) { coreMaxTemp = Math.Max(coreMaxTemp, val); coreMaxFound = true; }
                    else if (sensor.Name.Contains("Core", StringComparison.OrdinalIgnoreCase) && !sensor.Name.Contains("Distance", StringComparison.OrdinalIgnoreCase)) { highestCoreTemp = Math.Max(highestCoreTemp, val); highestCoreFound = true; }
                }
            }

            // Sensor preferences: 0 = Package (default), 1 = Core Max, 2 = Max of any Core sensor
            if (sensorPreference == 0 && packageFound) return packageTemp;
            if (sensorPreference == 1 && coreMaxFound) return coreMaxTemp;
            if (highestCoreFound) return highestCoreTemp;
            if (coreMaxFound) return coreMaxTemp;
            if (packageFound) return packageTemp;

            return 0;
        }

        public float GetGpuTemperature(string preferredGpuName = "", int sensorPreference = 0)
        {
            if (computer == null) return 0;

            IHardware? gpu = null;
            if (!string.IsNullOrEmpty(preferredGpuName))
            {
                gpu = computer.Hardware.FirstOrDefault(h => 
                    (h.HardwareType == HardwareType.GpuNvidia || h.HardwareType == HardwareType.GpuAmd || h.HardwareType == HardwareType.GpuIntel) && 
                    h.Name.Equals(preferredGpuName, StringComparison.OrdinalIgnoreCase));
            }

            if (gpu != null)
            {
                return GetGpuTempFromHardware(gpu, sensorPreference);
            }

            // Auto-detect among all available GPUs
            var gpus = computer.Hardware.Where(h => h.HardwareType == HardwareType.GpuNvidia || h.HardwareType == HardwareType.GpuAmd || h.HardwareType == HardwareType.GpuIntel).ToList();
            if (!gpus.Any()) return 0;

            // Prefer Nvidia first, then AMD, then Intel
            foreach (var activeGpu in gpus.OrderBy(g => g.HardwareType == HardwareType.GpuNvidia ? 0 : g.HardwareType == HardwareType.GpuAmd ? 1 : 2))
            {
                float temp = GetGpuTempFromHardware(activeGpu, sensorPreference);
                if (temp > 0) return temp;
            }

            return 0;
        }

        private float GetGpuTempFromHardware(IHardware gpu, int sensorPreference)
        {
            if (gpu == null) return 0;
            
            float coreTemp = 0; 
            float hotSpotTemp = 0; 
            float genericTemp = 0;
            bool coreFound = false; 
            bool hotSpotFound = false; 
            bool genericFound = false;

            foreach (var sensor in gpu.Sensors) 
            { 
                if (sensor.SensorType == SensorType.Temperature && sensor.Value.HasValue && sensor.Value.Value > 0) 
                { 
                    float val = sensor.Value.Value;
                    if (gpu.HardwareType == HardwareType.GpuIntel && sensor.Name.Equals("GPU Temperature", StringComparison.OrdinalIgnoreCase)) 
                    { 
                        genericTemp = Math.Max(genericTemp, val); 
                        genericFound = true; 
                    } 
                    else if (sensor.Name.Contains("GPU Core", StringComparison.OrdinalIgnoreCase) || sensor.Name.Equals("GPU Temperature", StringComparison.OrdinalIgnoreCase)) 
                    { 
                        coreTemp = Math.Max(coreTemp, val); 
                        coreFound = true; 
                    } 
                    else if (sensor.Name.Contains("Hot Spot", StringComparison.OrdinalIgnoreCase) || sensor.Name.Contains("Junction", StringComparison.OrdinalIgnoreCase)) 
                    { 
                        hotSpotTemp = Math.Max(hotSpotTemp, val); 
                        hotSpotFound = true; 
                    } 
                    else 
                    { 
                        genericTemp = Math.Max(genericTemp, val); 
                        genericFound = true; 
                    } 
                } 
            }

            // Sensor preference: 0 = Core (default), 1 = Hot Spot
            if (sensorPreference == 0)
            {
                if (coreFound && coreTemp > 0) return coreTemp;
                if (genericFound && genericTemp > 0) return genericTemp;
                if (hotSpotFound && hotSpotTemp > 0) return hotSpotTemp;
            }
            else // Hot Spot preference
            {
                if (hotSpotFound && hotSpotTemp > 0) return hotSpotTemp;
                if (coreFound && coreTemp > 0) return coreTemp;
                if (genericFound && genericTemp > 0) return genericTemp;
            }

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
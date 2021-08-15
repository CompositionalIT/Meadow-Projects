using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Distance.Test
{
    /// <summary>
    /// Represents the Vl53l0x distance sensor
    /// </summary>
    public class Vl53l1x : ByteCommsSensorBase<Length>, IRangeFinder
    {
        public const byte DefaultI2cAddress = 0x29;

        const uint IDENTIFICATION__MODEL_ID = 0x010F;
        const uint Vl53l1x_ID = 0xEACC;

        /// <summary>
        /// The distance to the measured object.
        /// </summary>
        public Length? Distance => throw new NotImplementedException();

        readonly IDigitalOutputPort shutdownPort;
        public bool IsShutdown
        {
            get
            {
                if (shutdownPort != null)
                {
                    return !shutdownPort.State;
                }
                else
                {
                    return false;
                }
            }
        }

        public event EventHandler<IChangeResult<Length>> DistanceUpdated;

        public Vl53l1x(
            IDigitalOutputController device, II2cBus i2cBus,
            byte address = DefaultI2cAddress) : this(device, i2cBus, null, address)
        {
        }

        /// <param name="i2cBus">I2C bus</param>
        /// <param name="address">VL53L0X address</param>
        /// <param name="units">Unit of measure</param>
        public Vl53l1x(
            IDigitalOutputController device, II2cBus i2cBus, IPin shutdownPin,
            byte address = DefaultI2cAddress) : base(i2cBus, address)
        {
            if (shutdownPin != null)
            {
                device.CreateDigitalOutputPort(shutdownPin, true);
            }
            Initialize().Wait();
        }

        protected async Task Initialize()
        {
            if (IsShutdown)
            {
                await ShutDown(false);
            }

            if (Read16(IDENTIFICATION__MODEL_ID) != Vl53l1x_ID)
            {
                throw new Exception($"Failed to find expected ID register values.");
            }
            else
            {
                Console.WriteLine("Sensor found!!!");
            }
        }

        protected override Task<Length> ReadSensor()
        {
            throw new NotImplementedException();
        }

        protected byte Read(byte address)
        {
            var result = Peripheral.ReadRegister(address);
            return result;
        }

        protected int Read16(uint address)
        {
            var addressSpan = new Span<byte>(BitConverter.GetBytes(address));
            Peripheral.Exchange(addressSpan, ReadBuffer.Span[0..2]);
            Console.WriteLine($"\n0: {ReadBuffer.Span[0]}\n1: {ReadBuffer.Span[1]}");
            return (ReadBuffer.Span[0] << 8) | ReadBuffer.Span[1];
        }

        protected int Read16(byte address)
        {
            Peripheral.ReadRegister(address, ReadBuffer.Span[0..2]);
            return (ReadBuffer.Span[0] << 8) | ReadBuffer.Span[1];
        }

        /// <summary>
        /// Set the Shutdown state of the device
        /// </summary>
        /// <param name="state">true = off/shutdown. false = on</param>
        public async Task ShutDown(bool state)
        {
            if (shutdownPort == null)
            {
                return;
            }

            shutdownPort.State = !state;
            await Task.Delay(2).ConfigureAwait(false);

            if (state == false)
            {
                await Initialize();
                // TODO: is this still needed? the previous line wasn't awaited before.
                await Task.Delay(2).ConfigureAwait(false);
            }
        }
    }
}

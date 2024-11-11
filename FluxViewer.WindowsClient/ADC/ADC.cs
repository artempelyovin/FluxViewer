using System.IO.Ports;
namespace FluxViewer.WindowsClient.ADC;

public struct PhysicalChannelConfiguration
{
    public byte Kabel;   // Номер кабеля
    public byte Addres;  // Адрес устройства
    public byte Range;   // Диапазон для приведения
}

public class ModbusADC
{
    public byte ADC_ID;
    public string ModuleInfo = "";

    private SerialPort _comPort;
    private const int MaxChannels = 8;
    public PhysicalChannel[] PhysicalChannels;

    public ModbusADC(byte adcID, SerialPort comPort, PhysicalChannelConfiguration[] physicalChannels) {
        _comPort = comPort;
        try {
            _comPort.Open();

            Send(_comPort, ADC_ID, 3, 0xD2, 1);
            Thread.Sleep(150);
            byte[] response = new byte[7];
            _comPort.Read(response, 0, 7);
            ModuleInfo = $"{response[3]:X2}{response[4]:X2}";
        }

        catch  (Exception ex) {
            throw new ModbusComInitExpection($"Error while init com port: {ex.Message}");
        }
        ADC_ID = adcID;
        UpdateChannels(physicalChannels);
    }

    public void UpdateChannels(PhysicalChannelConfiguration[] physicalChannels){
        if (physicalChannels.Length > MaxChannels) {
            throw new ArgumentException($"The maximum number of channels is {MaxChannels}.");
        }
        PhysicalChannel[] physChs = new PhysicalChannel[physicalChannels.Length];
        for (int i = 0; i < physicalChannels.Length; i++){
            physChs[i] = new PhysicalChannel(i, physicalChannels[i], _comPort);
        }
        PhysicalChannels = physChs;
    }

    private static void Send(SerialPort comPort, byte id, byte comm, byte adr, byte data) {
         byte[] send = new byte[8];
         send[0] = id;
         send[1] = comm;
         send[2] = 0x00;
         send[3] = adr;
         send[4] = 0x00;
         send[5] = data;

         ushort csum = 0xFFFF;
         for (int i = 0; i < 6; i++) {
             csum ^= send[i];
             for (int j = 0; j < 8; j++) {
                 bool CF = (csum & 1) != 0;
                 csum >>= 1;
                 if (CF) {
                     csum ^= 0xA001;
                 }
             }
         }

         send[6] = (byte)(csum & 0xFF);
         send[7] = (byte)((csum & 0xFF00) >> 8);

         comPort.Write(send, 0, 8);
    }

    public class PhysicalChannel {
        public PhysicalChannelConfiguration Configuration;
        public int ChannelIndex;
        private SerialPort _comPort;

        public PhysicalChannel(int channelIndex, PhysicalChannelConfiguration configuration, SerialPort comPort) {
            ChannelIndex = channelIndex;
            Configuration = configuration;
            _comPort = comPort;
        }

        public int Read(byte register) {
            byte[] readData = new byte[21];
            Array.Clear(readData, 0, readData.Length);

            try {
                // Очистка буфера ввода и вывода
                _comPort.DiscardInBuffer();
                _comPort.DiscardOutBuffer();

                // Отправляем команду на чтение регистра
                Send(_comPort, Configuration.Kabel, 3, (byte)(Configuration.Addres + register), 1);
                Thread.Sleep(30);

                // Чтение первых двух байт ответа
                _comPort.Read(readData, 0, 2);

                // Проверка на ошибку: если второй байт равен 0, то произошла ошибка
                if (readData[1] == 0) {
                    throw new ModbusErrorCodeException($"Modbus Error: Code {readData[2]}");
                }

                // Чтение количества байт данных и оставшейся части пакета
                _comPort.Read(readData, 2, 1);
                _comPort.Read(readData, 3, readData[2] + 2);

                // Проверка CRC
                ushort receivedCrc = (ushort)((readData[6] << 8) | readData[5]);
                if (receivedCrc != CalculateCRC16(readData, 5)) {
                    throw new ModbusCrcException("Modbus CRC Error: Checksum mismatch");
                }

                // Если это специальный регистр, возвращаем данные напрямую
                if (register == 200) {
                    return (readData[3] << 8) | readData[4];
                }

                // Обработка данных для стандартных регистров
                return DataRange(readData[3], readData[4]);
            }

            catch (TimeoutException) {
                throw new ModbusException("Timeout while reading from Modbus device.");
            }
            catch (IOException ioEx) {
                throw new ModbusException($"IO error in Modbus communication: {ioEx.Message}");
            }
        }



        // Перевод диапазпонов в мВ в зависимости от установленного диапазона значений.
        private int DataRange(byte msb, byte lsb) {
            int value = (msb << 8) | lsb;

            return Configuration.Range switch
            {
                0x07 => value * 20000 / 65535,
                0x08 => (value - 32768) * 20000 / 65535,
                0x09 => (value - 32768) * 10000 / 65535,
                0x0A => (value - 32768) * 2000 / 65535,
                0x0B => (value - 32768) * 1000 / 65535,
                0x0C => (value - 32768) * 3000 / 65535,
                0x0D => (value - 32768) * 40000 / 65535,
                0x15 => (value - 32768) * 30000 / 65535,
                0x48 => value * 10000 / 65535,
                0x49 => value * 5000 / 65535,
                0x4A => value * 1000 / 65535,
                0x4B => value * 500 / 65535,
                0x4C => value * 150 / 65535,
                0x4D => value * 20000 / 65535,
                0x55 => value * 15000 / 65535,
                _ => throw new ModbusOutOfRangeException($"Range № {Configuration.Range} not found for conversion.")
            };
        }

         // Метод для вычисления CRC-16
        private static ushort CalculateCRC16(byte[] array, int count){
            ushort csum = 0xFFFF;

            for (int i = 0; i < count; i++) {
                csum ^= array[i];
                for (int j = 0; j < 8; j++) {
                    bool cf = (csum & 1) != 0;
                    csum >>= 1;
                    if (cf) {
                        csum ^= 0xA001;
                    }
                }
            }
            return csum;
        }
    }
}

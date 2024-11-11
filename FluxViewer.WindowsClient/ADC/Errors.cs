namespace FluxViewer.WindowsClient.ADC;

public class ModbusException : Exception {
    public ModbusException(string message) : base(message) { }
}

public class ModbusCrcException : ModbusException {
    public ModbusCrcException(string message) : base(message) { }
}

public class ModbusErrorCodeException : ModbusException {
    public ModbusErrorCodeException(string message) : base(message) { }
}

public class ModbusComInitExpection: ModbusException {
    public ModbusComInitExpection(string message): base(message) {}
}

public class ModbusOutOfRangeException: ModbusException {
    public ModbusOutOfRangeException(string message): base(message) {}
}
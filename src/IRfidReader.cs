using Insky.Rfid.Models;

namespace Insky.Rfid;

public interface IRfidReader
{
    RfidResult Connect(string comPort, int baudRate);
    void Disconnect();
    bool IsConnected();
    RfidResult SetPower(byte power, byte freq);
    RfidResult Read(int memBank, int wordPtr, int wordCnt);
    RfidResult Write(int memBank, int wordPtr, int wordCnt, string value);
    ReaderInfo GetReaderInfo(ref byte fComAdr);
}
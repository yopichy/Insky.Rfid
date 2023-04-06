using Insky.Rfid.Models;
using Insky.Rfid.Utils;
using ReaderB;
using System;

namespace Insky.Rfid.Readers;

public class CP210X : IRfidReader
{
    private bool _comOpen;
    private int _fCmdRet = 30;
    private byte _fComAdr = 0xff;
    private int _fErrorCode;
    private int _frmComPortIndex;

    public RfidResult Connect(string comPort, int baudRate)
    {
        var port = Convert.ToInt32(comPort.Substring(3, comPort.Length - 3));

        var result = new RfidResult();
        try
        {
            var baud = 0;

            if (_comOpen)
            {
                return result;
            }

            switch (baudRate)
            {
                case 9600:
                    baud = 1;
                    break;
                case 19200:
                    baud = 2;
                    break;
                case 38400:
                    baud = 3;
                    break;
                case 57600:
                    baud = 4;
                    break;
                case 115200:
                    baud = 5;
                    break;
            }

            if (baud > 2) baud += 2;

            var status = StaticClassReaderB.AutoOpenComPort(ref port, ref _fComAdr, Convert.ToByte(baud), ref _frmComPortIndex);
            switch (status)
            {
                case 0:
                {
                    result.Code = ResponseCode.Success;
                    var reader = GetReaderInfo(ref _fComAdr);

                    result.Message = $"Reader {reader} has been connected on {comPort}";
                    _comOpen = true;

                    if (_fCmdRet == 0x35 || _fCmdRet == 0x30)
                    {
                        _comOpen = false;
                        result.Code = ResponseCode.Error;
                        result.Message = "Serial Communication Error or Occupied";
                        StaticClassReaderB.CloseSpecComPort(_frmComPortIndex);
                    }
                    break;
                }
                case 48:
                    result.Code = ResponseCode.Error;
                    result.Message = "Serial Communication Error or Occupied, try another baud rate";
                    break;
            }

            return result;
        }
        catch (Exception ex)
        {
            if (_comOpen)
                StaticClassReaderB.CloseSpecComPort(port);

            return new RfidResult(ResponseCode.Error, ex.Message);
        }
    }

    public void Disconnect()
    {
        try
        {
            if (!IsConnected()) return;

            StaticClassReaderB.CloseComPort();
            _comOpen = false;
        }
        catch (Exception)
        {
            // ignored
        }
    }

    public bool IsConnected()
    {
        return _comOpen;
    }

    public RfidResult SetPower(byte power, byte freq)
    {
        return null;
    }

    public RfidResult Write(int memBank, int wordPtr, int wordCnt, string value)
    {
        var result = new RfidResult();

        var epc = HexConverter.HexStringToByteArray(value);

        var writeEpcLen = Convert.ToByte(value.Length / 2);

        _fCmdRet = StaticClassReaderB.WriteEPC_G2(ref _fComAdr, HexConverter.HexStringToByteArray("00000000"), epc, writeEpcLen, ref _fErrorCode, _frmComPortIndex);

        if (_fCmdRet != 0) return result;

        result.Code = ResponseCode.Success;
        result.Message = $"Success write with ID : {value}";
        result.Data = value;

        return result;
    }

    public RfidResult Read(int memBank, int wordPtr, int wordCnt)
    {
        var result = new RfidResult();

        byte adrTid = 0;
        byte lenTid = 0;
        byte tidFlag = 0;
        var epc = new byte[5000];
        var totalLen = 0;
        var cardNum = 0;

        _fCmdRet = StaticClassReaderB.Inventory_G2(ref _fComAdr, adrTid, lenTid, tidFlag, epc, ref totalLen, ref cardNum, _frmComPortIndex);

        var daw = new byte[totalLen];
        Array.Copy(epc, daw, totalLen);
        var temps = HexConverter.ByteArrayToHexString(daw);

        if (daw.Length == 0) return result;

        int epcLen = daw[0];
        var sEpc = temps.Substring(0 * 2 + 2, epcLen * 2);
        if (sEpc.Length != epcLen * 2)
        {
            result.Code = ResponseCode.Error;
            result.Data = "Data not valid";
        }
        else
        {
            result.Code = ResponseCode.Success;
            result.Data = sEpc;
        }

        return result;
    }

    public ReaderInfo GetReaderInfo(ref byte fComAdr)
    {
        var trType = new byte[2];
        var versionInfo = new byte[2];
        byte readerType = 0;
        byte scanTime = 0;
        byte dMaxFre = 0;
        byte dMinFre = 0;
        byte powerDbm = 0;

        _fCmdRet = StaticClassReaderB.GetReaderInformation(ref fComAdr, versionInfo, ref readerType, trType, ref dMaxFre, ref dMinFre, ref powerDbm, ref scanTime, _frmComPortIndex);
        if (_fCmdRet != 0) return null;

        var freBand = Convert.ToByte((dMaxFre & 0xc0) >> 4 | dMinFre >> 6);

        switch (freBand)
        {
            case 0:
                var minFre = 902.6 + (dMinFre & 0x3F) * 0.4;
                var maxFre = 902.6 + (dMaxFre & 0x3F) * 0.4;

                return new ReaderInfo
                {
                    BandFreq = freBand,
                    MinFreq = minFre,
                    MaxFreq = maxFre
                };
        }

        return null;
    }
}
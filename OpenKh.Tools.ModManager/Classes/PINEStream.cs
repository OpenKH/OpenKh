using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Net.Sockets;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace OpenKh.Tools.ModManager.Classes
{
    public sealed class PINEStream : Stream
    {
        struct PINE_MESSAGE
        {
            public PINE_COMMAND Command;
            public uint Address;
            public object? Data;

            public uint Length => 0x05 + (Data == null ? 0x00 : (uint)Marshal.SizeOf(Data));

            public byte[] ToArray()
            {
                var _fetchData = new byte[Length];
                var _fetchSpan = _fetchData.AsSpan(0x05, Data == null ? 0x00 : Marshal.SizeOf(Data));

                _fetchData[0] = (byte)Command;

                BinaryPrimitives.WriteUInt32LittleEndian(_fetchData.AsSpan(0x01, 0x04), Address);

                switch (Data)
                {
                    case null:
                        break;

                    case byte value:
                        _fetchData[0x05] = value;
                        break;

                    case ushort value:
                        BinaryPrimitives.WriteUInt16LittleEndian(
                            _fetchSpan,
                            value);
                        break;

                    case uint value:
                        BinaryPrimitives.WriteUInt32LittleEndian(
                            _fetchSpan,
                            value);
                        break;

                    case ulong value:
                        BinaryPrimitives.WriteUInt64LittleEndian(
                            _fetchSpan,
                            value);
                        break;

                    default:
                        throw new NotSupportedException(
                            $"Data type {Data?.GetType().Name} is not supported.");
                }

                return _fetchData.ToArray();
            }
        }

        enum PINE_COMMAND : byte
        {
            READ_INT08,
            READ_INT16,
            READ_INT32,
            READ_INT64,

            WRITE_INT08,
            WRITE_INT16,
            WRITE_INT32,
            WRITE_INT64
        };

        enum PINE_RESPONSE : byte
        {
            IPC_OK,
            IPC_FAIL = 0xFF
        };

        const int MAX_SEND_LENGTH = 640000;
        const int MAX_RECEIVE_LENGTH = 440000;

        readonly object _syncLock = new();
        readonly string _socketPath;

        Socket? _mainSocket;

        uint _currPosition;

        bool _isValid;
        bool _hasDisposed;

        public PINEStream(int slot = 28011)
        {
            if (OperatingSystem.IsLinux())
            {
                TRY_SOCK_CONNECT:

                if (slot < 1 || slot > 65535)
                    throw new ArgumentOutOfRangeException(nameof(slot));

                var _fetchRuntimePath = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");

                _socketPath = String.IsNullOrEmpty(_fetchRuntimePath) ? "/tmp/pcsx2.sock" : Path.Combine(_fetchRuntimePath, "pcsx2.sock");

                if (slot != 28011)
                    _socketPath += "." + slot;

                var _fetchSocket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

                try
                {
                    var _fetchEndPoint = new UnixDomainSocketEndPoint(_socketPath);
                    _fetchSocket.Connect(_fetchEndPoint);

                    _mainSocket = _fetchSocket;
                }

                catch
                {
                    _fetchSocket.Dispose();
                    _fetchSocket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

                    _socketPath = "/tmp/pcsx2.sock";

                    if (slot != 28011)
                        _socketPath += "." + slot;

                    try
                    {
                        var _fetchEndPoint = new UnixDomainSocketEndPoint(_socketPath);
                        _fetchSocket.Connect(_fetchEndPoint);

                        _mainSocket = _fetchSocket;
                    }

                    catch 
                    {
                        _fetchSocket.Dispose();
                        goto TRY_SOCK_CONNECT;
                    }
                }
            }

            else
            {
                if (slot < 1 || slot > 65535)
                    throw new ArgumentOutOfRangeException(nameof(slot));

                try
                {
                    var socket = new Socket(
                        AddressFamily.InterNetwork,
                        SocketType.Stream,
                        ProtocolType.Tcp
                    );

                    socket.Connect(IPAddress.Loopback, slot);

                    _mainSocket = socket;
                    _isValid = true;
                }
                catch (SocketException)
                {
                    _mainSocket = null;
                    _isValid = false;
                }
            }

            _isValid = _mainSocket != null;
        }

        public override bool CanRead => !_hasDisposed && _isValid;
        public override bool CanSeek => !_hasDisposed && _isValid;
        public override bool CanWrite => !_hasDisposed && _isValid;

        public override long Length => UInt32.MaxValue + 1L;

        public override long Position
        {
            get
            {
                lock (_syncLock)
                    return _currPosition;
            }

            set => Seek(value, SeekOrigin.Begin);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            lock (_syncLock)
            {
                if (origin == SeekOrigin.End)
                    throw new ArgumentException("Invalid seek origin.", nameof(origin));

                var _calcPosition = origin == SeekOrigin.Begin ? (uint)offset : (uint)(_currPosition + offset);

                if (_calcPosition >= UInt32.MaxValue || _calcPosition < 0x00)
                    throw new ArgumentOutOfRangeException();

                return _currPosition = _calcPosition;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (_syncLock)
            {
                uint _initialPosition = _currPosition;
                uint _pastByteLength = 0x00;

                if (_hasDisposed)
                    throw new ObjectDisposedException(nameof(PINEStream));

                if (!_isValid)
                    throw new InvalidOperationException("PINE stream is not valid.");

                if (buffer == null)
                    throw new ArgumentNullException(nameof(buffer));

                if (offset < 0 || count <= 0 || offset > buffer.Length - count || count > Length - _currPosition)
                    throw new ArgumentOutOfRangeException();

                for (uint _packageLength = 0x00, _packageBytes = 0x00, _packageReceive = 0x00; _packageBytes < count;)
                {
                    var _makePacket = new List<byte>();
                    var _messageList = new List<PINE_MESSAGE>();

                    for (; _packageLength < MAX_SEND_LENGTH - 0x04 && _packageReceive < MAX_RECEIVE_LENGTH - 0x04 && _packageBytes < count;)
                    {
                        var _byteRemain = count - _packageBytes;

                        var _makeMessage = new PINE_MESSAGE
                        {
                            Command = _byteRemain >= 0x08 ? PINE_COMMAND.READ_INT64 :
                                      _byteRemain >= 0x04 ? PINE_COMMAND.READ_INT32 :
                                      _byteRemain >= 0x02 ? PINE_COMMAND.READ_INT16 : PINE_COMMAND.READ_INT08,

                            Address = _initialPosition + _packageBytes
                        };

                        _messageList.Add(_makeMessage);
                        _makePacket.AddRange(_makeMessage.ToArray());

                        var _currLength = _byteRemain >= 0x08 ? 0x08U :
                                          _byteRemain >= 0x04 ? 0x04U :
                                          _byteRemain >= 0x02 ? 0x02U : 0x01U;

                        _packageBytes += _currLength;
                        _packageReceive += _currLength;

                        _packageLength += _makeMessage.Length;
                    }

                    var _finalSize = _makePacket.Count + 0x04;
                    _makePacket.InsertRange(0x00, BitConverter.GetBytes(_finalSize));

                    int _fetchSend = _mainSocket.Send(_makePacket.ToArray(), SocketFlags.None);

                    var _fetchHeaderResponse = new byte[0x04];
                    int _fetchResponse = _mainSocket.Receive(_fetchHeaderResponse, SocketFlags.None);

                    if (_fetchSend <= 0x00 || _fetchResponse <= 0x00)
                        throw new IOException("PINE socket closed.");

                    var _lengthResponse = BinaryPrimitives.ReadUInt32LittleEndian(_fetchHeaderResponse);

                    if (_lengthResponse < 0x05 || _lengthResponse > 0x6DDD0)
                        throw new IOException("Invalid PINE response.");

                    var _fetchDataResponse = new byte[_lengthResponse];
                    int _fetchResult = _mainSocket.Receive(_fetchDataResponse, SocketFlags.None);

                    if (_fetchResult <= 0)
                        throw new IOException("PINE socket closed.");

                    var _fetchCodeResponse = _fetchDataResponse[0x00];

                    if (_fetchCodeResponse == (byte)PINE_RESPONSE.IPC_FAIL)
                        return (int) _packageBytes;

                    if (_fetchCodeResponse != (byte)PINE_RESPONSE.IPC_OK)
                        throw new IOException($"Unknown PINE response: 0x{_fetchCodeResponse.ToString("X2")}");

                    for (int s = 0; s < _fetchDataResponse.Length - 0x05;)
                    {
                        var _fetchOffset = offset + s + (int)_pastByteLength;
                        var _byteRemain = _fetchDataResponse.Length - 0x05 - s;

                        var _readLength = _byteRemain >= 0x08 ? 0x08 :
                                          _byteRemain >= 0x04 ? 0x04 :
                                          _byteRemain >= 0x02 ? 0x02 : 0x01;

                        var _fetchSpanRead = _fetchDataResponse.AsSpan(0x01 + s, _readLength);
                        var _fetchSpanWrite = buffer.AsSpan(_fetchOffset, _readLength);

                        switch (_readLength)
                        {
                            case 0x08:
                            {
                                var _fetchData = BinaryPrimitives.ReadUInt64LittleEndian(_fetchSpanRead);
                                BinaryPrimitives.WriteUInt64LittleEndian(_fetchSpanWrite, _fetchData);
                            }
                            break;

                            case 0x04:
                            {
                                var _fetchData = BinaryPrimitives.ReadUInt32LittleEndian(_fetchSpanRead);
                                BinaryPrimitives.WriteUInt32LittleEndian(_fetchSpanWrite, _fetchData);
                            }
                            break;

                            case 0x02:
                            {
                                var _fetchData = BinaryPrimitives.ReadUInt16LittleEndian(_fetchSpanRead);
                                BinaryPrimitives.WriteUInt16LittleEndian(_fetchSpanWrite, _fetchData);
                            }
                            break;

                            default:
                                buffer[_fetchOffset] = _fetchDataResponse[0x05];
                                break;
                        }

                        s += _readLength;
                    }

                    _pastByteLength = _packageBytes;
                    _currPosition = _initialPosition + _packageBytes;

                    _packageReceive = 0x00;
                    _packageLength = 0x00;
                }

                return count;
            }
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            lock (_syncLock)
            {
                uint _initialPosition = _currPosition;

                if (_hasDisposed || !_isValid)
                    throw new ObjectDisposedException(nameof(PINEStream));

                if (buffer == null)
                    throw new ArgumentNullException();

                if (offset < 0 || count <= 0 || offset > buffer.Length - count || count > Length - _initialPosition)
                    throw new ArgumentOutOfRangeException();

                for (uint _packageLength = 0x00, _packageBytes = 0x00; _packageBytes < count;)
                {
                    var _makePacket = new List<byte>();
                    var _messageList = new List<PINE_MESSAGE>();

                    for (; _packageLength < MAX_SEND_LENGTH - 0x04 && _packageBytes < count;)
                    {
                        var _makeMessage = new PINE_MESSAGE();

                        var _fetchRemain = count - _packageBytes;
                        var _fetchOffset = (int) (offset + _packageBytes);

                        _makeMessage.Address = _initialPosition + _packageBytes;

                        if (_fetchRemain >= 0x08)
                        {
                            _makeMessage.Command = PINE_COMMAND.WRITE_INT64;

                            var _fetchSpan = buffer.AsSpan(_fetchOffset, 0x08);
                            _makeMessage.Data = BinaryPrimitives.ReadUInt64LittleEndian(_fetchSpan);

                            _packageBytes += 0x08;
                        }

                        else if (_fetchRemain >= 0x04)
                        {
                            _makeMessage.Command = PINE_COMMAND.WRITE_INT32;

                            var _fetchSpan = buffer.AsSpan(_fetchOffset, 0x04);
                            _makeMessage.Data = BinaryPrimitives.ReadUInt32LittleEndian(_fetchSpan);

                            _packageBytes += 0x04;
                        }

                        else if (_fetchRemain >= 0x02)
                        {
                            _makeMessage.Command = PINE_COMMAND.WRITE_INT16;

                            var _fetchSpan = buffer.AsSpan(_fetchOffset, 0x02);
                            _makeMessage.Data = BinaryPrimitives.ReadUInt16LittleEndian(_fetchSpan);


                            _packageBytes += 0x02;
                        }

                        else
                        {
                            _makeMessage.Command = PINE_COMMAND.WRITE_INT08;
                            _makeMessage.Data = buffer[_fetchOffset];

                            _packageBytes++;
                        }

                        _messageList.Add(_makeMessage);
                        _makePacket.AddRange(_makeMessage.ToArray());

                        _packageLength += _makeMessage.Length;
                    }

                    var _finalSize = _makePacket.Count + 0x04;
                    _makePacket.InsertRange(0x00, BitConverter.GetBytes(_finalSize));

                    int _fetchSend = _mainSocket.Send(_makePacket.ToArray(), SocketFlags.None);

                    var _fetchHeaderResponse = new byte[0x04];
                    int _fetchResponse = _mainSocket.Receive(_fetchHeaderResponse, SocketFlags.None);

                    if (_fetchSend <= 0x00 || _fetchResponse <= 0x00)
                        throw new IOException("PINE socket closed.");

                    var _lengthResponse = BinaryPrimitives.ReadUInt32LittleEndian(_fetchHeaderResponse);

                    if (_lengthResponse < 0x05 || _lengthResponse > 0x6DDD0)
                        throw new IOException("Invalid PINE response.");

                    var _fetchDataResponse = new byte[_lengthResponse];
                    int _fetchResult = _mainSocket.Receive(_fetchDataResponse, SocketFlags.None);

                    if (_fetchResult <= 0)
                        throw new IOException("PINE socket closed.");

                    var _fetchCodeResponse = _fetchDataResponse[0x04];

                    if (_fetchCodeResponse == (byte)PINE_RESPONSE.IPC_FAIL)
                        return;

                    if (_fetchCodeResponse != (byte)PINE_RESPONSE.IPC_OK)
                        throw new IOException($"Unknown PINE response: 0x{_fetchCodeResponse.ToString("X2")}");

                    _currPosition = _initialPosition + _packageBytes;
                    _packageLength = 0x00;
                }
            }
        }

        public override void Flush() { }

        public override void SetLength(long value) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (!_hasDisposed)
            {
                lock (_syncLock)
                {
                    var _fetchSocket = _mainSocket;
                    _mainSocket = null;

                    if (_fetchSocket != null)
                    {
                        try
                        { _fetchSocket.Shutdown(SocketShutdown.Both); }
                        catch { }

                        _fetchSocket.Dispose();
                    }

                    _hasDisposed = true;
                    _isValid = false;
                }
            }

            base.Dispose(disposing);
        }
    }
}

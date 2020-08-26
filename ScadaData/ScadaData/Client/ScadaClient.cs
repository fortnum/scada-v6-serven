﻿/*
 * Copyright 2020 Mikhail Shiryaev
 * 
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *     http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * 
 * 
 * Product  : Rapid SCADA
 * Module   : ScadaData
 * Summary  : Represents a TCP client which interacts with the Server service
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2020
 * Modified : 2020
 */

using Scada.Data.Models;
using Scada.Data.Tables;
using Scada.Protocol;
using System;
using System.Collections.Generic;
using static Scada.BinaryConverter;
using static Scada.Protocol.ProtocolUtils;

namespace Scada.Client
{
    /// <summary>
    /// Represents a TCP client which interacts with the Server service.
    /// <para>Представляет TCP-клиента, который взаимодействует со службой Сервера.</para>
    /// </summary>
    public class ScadaClient : BaseClient
    {
        /// <summary>
        /// The ID of the last command received.
        /// </summary>
        protected long lastCommandID;


        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public ScadaClient(ConnectionOptions connectionOptions)
            : base(connectionOptions)
        {
            lastCommandID = 0;
        }


        /// <summary>
        /// Throws an exception when data size in a data packet does not match.
        /// </summary>
        protected void ThrowDataSizeException()
        {
            throw new ProtocolException(ErrorCode.IllegalFunctionArguments, Locale.IsRussian ?
                "Неверный размер данных." :
                "Invalid data size.");
        }

        /// <summary>
        /// Gets the current data.
        /// </summary>
        public CnlData[] GetCurrentData(ref long cnlListID)
        {
            RestoreConnection();

            DataPacket request = CreateRequest(FunctionID.GetCurrentData);
            CopyInt64(cnlListID, outBuf, ArgumentIndex);
            request.ArgumentLength = 8;
            SendRequest(request);

            DataPacket response = ReceiveResponse(request);
            int index = ArgumentIndex;
            cnlListID = GetInt64(inBuf, ref index);
            return cnlListID > 0 ? GetCnlDataArray(inBuf, ref index) : null;
        }

        /// <summary>
        /// Gets the current data.
        /// </summary>
        public CnlData[] GetCurrentData(int[] cnlNums, bool useCache, out long cnlListID)
        {
            if (cnlNums == null)
                throw new ArgumentNullException("cnlNums");

            RestoreConnection();

            DataPacket request = CreateRequest(FunctionID.GetCurrentData);
            int index = ArgumentIndex;
            CopyInt64(0, outBuf, ref index);
            CopyIntArray(cnlNums, outBuf, ref index);
            CopyBool(useCache, outBuf, ref index);
            request.BufferLength = index;
            SendRequest(request);

            DataPacket response = ReceiveResponse(request);
            index = ArgumentIndex;
            cnlListID = GetInt64(inBuf, ref index);
            CnlData[] cnlData = GetCnlDataArray(inBuf, ref index);

            if (cnlData.Length != cnlNums.Length)
                ThrowDataSizeException();

            return cnlData;
        }

        /// <summary>
        /// Gets the trends of the specified input channels.
        /// </summary>
        public TrendBundle GetTrends(int[] cnlNums, DateTime startTime, DateTime endTime, int archiveBit)
        {
            return null;
        }

        /// <summary>
        /// Gets the slice of the specified input channels at the timestamp.
        /// </summary>
        public Slice GetSlice(int[] cnlNums, DateTime timestamp, int archiveBit)
        {
            RestoreConnection();

            DataPacket request = CreateRequest(FunctionID.GetEventByID);
            int index = ArgumentIndex;
            CopyIntArray(cnlNums, outBuf, ref index);
            CopyTime(timestamp, outBuf, ref index);
            CopyByte((byte)archiveBit, outBuf, ref index);
            request.BufferLength = index;
            SendRequest(request);

            DataPacket response = ReceiveResponse(request);
            index = ArgumentIndex;
            CnlData[] cnlData = GetCnlDataArray(inBuf, ref index);

            if (cnlData.Length != cnlNums.Length)
                ThrowDataSizeException();

            return new Slice(timestamp, cnlNums, cnlData);
        }

        /// <summary>
        /// Gets the event by ID.
        /// </summary>
        public Event GetEventByID(long eventID, int archiveBit)
        {
            RestoreConnection();

            DataPacket request = CreateRequest(FunctionID.GetEventByID);
            int index = ArgumentIndex;
            CopyInt64(eventID, outBuf, ref index);
            CopyByte((byte)archiveBit, outBuf, ref index);
            request.BufferLength = index;
            SendRequest(request);

            DataPacket response = ReceiveResponse(request);
            index = ArgumentIndex;
            return GetEvent(inBuf, ref index);
        }

        /// <summary>
        /// Gets the events.
        /// </summary>
        public ICollection<Event> GetEvents(DateTime startTime, DateTime endTime, DataFilter filter, int archiveBit)
        {
            return null;
        }

        /// <summary>
        /// Writes the current data.
        /// </summary>
        public void WriteCurrentData(int deviceNum, int[] cnlNums, CnlData[] cnlData, bool applyFormulas)
        {
            if (cnlNums == null)
                throw new ArgumentNullException("cnlNums");
            if (cnlData == null)
                throw new ArgumentNullException("cnlData");

            RestoreConnection();

            DataPacket request = CreateRequest(FunctionID.WriteCurrentData);
            int index = ArgumentIndex;
            CopyInt32(deviceNum, outBuf, ref index);

            int cnlCnt = cnlNums.Length;
            CopyInt32(cnlCnt, outBuf, ref index);

            for (int i = 0, idx1 = index, idx2 = index + cnlCnt * 4; i < cnlCnt; i++)
            {
                CnlData cnlDataElem = cnlData[i];
                CopyInt32(cnlNums[i], outBuf, ref idx1);
                CopyDouble(cnlDataElem.Val, outBuf, ref idx2);
                CopyUInt16((ushort)cnlDataElem.Stat, outBuf, ref idx2);
            }

            index += cnlCnt * 14;
            CopyBool(applyFormulas, outBuf, ref index);
            request.BufferLength = index;
            SendRequest(request);
            ReceiveResponse(request);
        }

        /// <summary>
        /// Writes the historical data.
        /// </summary>
        public void WriteHistoricalData(int deviceNum, Slice slice, int archiveMask, bool applyFormulas)
        {
            if (slice == null)
                throw new ArgumentNullException("slice");

            RestoreConnection();

            DataPacket request = CreateRequest(FunctionID.WriteHistoricalData);
            int index = ArgumentIndex;
            CopyInt32(deviceNum, outBuf, ref index);
            CopyTime(slice.Timestamp, outBuf, ref index);

            int cnlCnt = slice.CnlNums.Length;
            CopyInt32(cnlCnt, outBuf, ref index);

            for (int i = 0, idx1 = index, idx2 = index + cnlCnt * 4; i < cnlCnt; i++)
            {
                CnlData cnlDataElem = slice.CnlData[i];
                CopyInt32(slice.CnlNums[i], outBuf, ref idx1);
                CopyDouble(cnlDataElem.Val, outBuf, ref idx2);
                CopyUInt16((ushort)cnlDataElem.Stat, outBuf, ref idx2);
            }

            index += cnlCnt * 14;
            CopyInt32(archiveMask, outBuf, ref index);
            CopyBool(applyFormulas, outBuf, ref index);
            request.BufferLength = index;
            SendRequest(request);
            ReceiveResponse(request);
        }

        /// <summary>
        /// Writes the event.
        /// </summary>
        public void WriteEvent(Event ev, int archiveMask)
        {
            if (ev == null)
                throw new ArgumentNullException("ev");

            RestoreConnection();

            DataPacket request = CreateRequest(FunctionID.WriteEvent);
            int index = ArgumentIndex;
            CopyEvent(ev, outBuf, ref index);
            request.BufferLength = index;

            SendRequest(request);
            ReceiveResponse(request);
        }

        /// <summary>
        /// Sends the telecontrol command.
        /// </summary>
        public void SendCommand(TeleCommand command, out CommandResult commandResult)
        {
            RestoreConnection();

            DataPacket request = CreateRequest(FunctionID.SendCommand);
            int index = ArgumentIndex;
            CopyInt32(command.UserID, outBuf, ref index);
            CopyInt32(command.OutCnlNum, outBuf, ref index);
            CopyDouble(command.CmdVal, outBuf, ref index);
            CopyByteArray(command.CmdData, outBuf, ref index);
            request.BufferLength = index;
            SendRequest(request);

            DataPacket response = ReceiveResponse(request);
            commandResult = new CommandResult
            {
                IsSuccessful = inBuf[ArgumentIndex] > 0,
                ErrorMessage = GetString(inBuf, ArgumentIndex + 1)
            };
        }

        /// <summary>
        /// Gets a telecontrol command from the server queue.
        /// </summary>
        public TeleCommand GetCommand()
        {
            RestoreConnection();

            DataPacket request = CreateRequest(FunctionID.GetCommand);
            CopyInt64(lastCommandID, outBuf, ArgumentIndex);
            request.ArgumentLength = 8;
            SendRequest(request);

            DataPacket response = ReceiveResponse(request);
            int index = ArgumentIndex;
            lastCommandID = GetInt64(inBuf, ref index);

            if (lastCommandID > 0)
            {
                return new TeleCommand
                {
                    CommandID = lastCommandID,
                    CreationTime = GetTime(inBuf, ref index),
                    UserID = GetInt32(inBuf, ref index),
                    OutCnlNum = GetInt32(inBuf, ref index),
                    CmdTypeID = GetInt32(inBuf, ref index),
                    ObjNum = GetInt32(inBuf, ref index),
                    DeviceNum = GetInt32(inBuf, ref index),
                    CmdNum = GetInt32(inBuf, ref index),
                    CmdCode = GetString(inBuf, ref index),
                    CmdVal = GetDouble(inBuf, ref index),
                    CmdData = GetByteArray(inBuf, ref index)
                };
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Disables getting commands for the client.
        /// </summary>
        public void DisableGettingCommands()
        {
            RestoreConnection();
            DataPacket request = CreateRequest(FunctionID.DisableGettingCommands, 10);
            SendRequest(request);
            ReceiveResponse(request);
        }
    }
}

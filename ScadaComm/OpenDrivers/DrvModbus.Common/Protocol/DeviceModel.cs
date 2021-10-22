﻿// Copyright (c) Rapid Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Scada.Comm.Drivers.DrvModbus.Protocol
{
    /// <summary>
    /// Represents a Modbus device model.
    /// <para>Представляет модель устройства Modbus.</para>
    /// </summary>
    public class DeviceModel
    {
        private Dictionary<int, ModbusCmd> cmdByNum; // the commands accessed by number

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public DeviceModel()
        {
            cmdByNum = null;

            ElemGroups = new List<ElemGroup>();
            Cmds = new List<ModbusCmd>();
        }


        /// <summary>
        /// Gets the element groups.
        /// </summary>
        public List<ElemGroup> ElemGroups { get; private set; }

        /// <summary>
        /// Gets the commands.
        /// </summary>
        public List<ModbusCmd> Cmds { get; private set; }


        /// <summary>
        /// Creates a new group of Modbus elements.
        /// </summary>
        public virtual ElemGroup CreateElemGroup(DataBlock dataBlock)
        {
            return new ElemGroup(dataBlock);
        }

        /// <summary>
        /// Creates a new Modbus command.
        /// </summary>
        public virtual ModbusCmd CreateModbusCmd(DataBlock dataBlock, bool multiple)
        {
            return new ModbusCmd(dataBlock, multiple);
        }

        /// <summary>
        /// Initializes a command map.
        /// </summary>
        public void InitCmdMap()
        {
            cmdByNum = new Dictionary<int, ModbusCmd>();

            foreach (ModbusCmd cmd in Cmds)
            { 
                if (cmdByNum.ContainsKey(cmd.CmdNum))
                    cmdByNum.Add(cmd.CmdNum, cmd); 
            };
        }

        /// <summary>
        /// Gets a command by the specified number.
        /// </summary>
        public ModbusCmd GetCmd(int cmdNum)
        {
            return cmdByNum != null && cmdByNum.TryGetValue(cmdNum, out ModbusCmd cmd) ? cmd : null;
        }
    }
}

﻿/*
 * Copyright 2021 Rapid Software LLC
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
 * Module   : ScadaAgentEngine
 * Summary  : Controls an instance that includes of one or more applications
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2018
 * Modified : 2021
 */

using Scada.Agent.Config;
using Scada.Lang;
using Scada.Log;
using Scada.Protocol;
using System;
using System.IO;
using System.Text;

namespace Scada.Agent.Engine
{
    /// <summary>
    /// Controls an instance that includes of one or more applications.
    /// <para>Управляет экземпляром, включающим из одно или несколько приложений.</para>
    /// </summary>
    internal class ScadaInstance
    {
        private readonly ILog log;                        // the application log
        private readonly InstanceOptions instanceOptions; // the instance options
        private readonly PathBuilder pathBuilder;         // builds file paths


        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public ScadaInstance(ILog log, InstanceOptions instanceOptions)
        {
            this.log = log ?? throw new ArgumentNullException(nameof(log));
            this.instanceOptions = instanceOptions ?? throw new ArgumentNullException(nameof(instanceOptions));
            pathBuilder = new PathBuilder(instanceOptions.Directory);
        }


        /// <summary>
        /// Gets the instance name.
        /// </summary>
        public string Name => instanceOptions.Name;


        /// <summary>
        /// Gets the file name that contains the service status.
        /// </summary>
        private string GetStatusFileName(ServiceApp serviceApp)
        {
            switch (serviceApp)
            {
                case ServiceApp.Server:
                    return "ScadaServer.txt";
                case ServiceApp.Comm:
                    return "ScadaComm.txt";
                default:
                    throw new ArgumentException("Service not supported.");
            }
        }


        /// <summary>
        /// Validates the username and password.
        /// </summary>
        public bool ValidateUser(string username, string password, out int userID, out int roleID, out string errMsg)
        {
            userID = 0;
            roleID = AgentRoleID.Disabled;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                errMsg = Locale.IsRussian ?
                    "Имя пользователя или пароль не может быть пустым" :
                    "Username or password can not be empty";
                return false;
            }

            if (string.Equals(instanceOptions.AdminUser.Username, username, StringComparison.OrdinalIgnoreCase) &&
                instanceOptions.AdminUser.Password == password)
            {
                roleID = AgentRoleID.Administrator;
                errMsg = "";
                return true;
            }
            else
            {
                errMsg = Locale.IsRussian ?
                    "Неверное имя пользователя или пароль" :
                    "Invalid username or password";
                return false;
            }
        }

        /// <summary>
        /// Gets the current status of the specified service.
        /// </summary>
        public bool GetServiceStatus(ServiceApp serviceApp, out ServiceStatus serviceStatus)
        {
            try
            {
                string fileName = pathBuilder.GetAbsolutePath(
                    new RelativePath(serviceApp, AppFolder.Log, GetStatusFileName(serviceApp)));

                if (File.Exists(fileName))
                {
                    using (FileStream stream = 
                        new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                        {
                            const int MaxLineCount = 10;
                            int lineCount = 0;

                            while (reader.Peek() >= 0 && lineCount < MaxLineCount)
                            {
                                string line = reader.ReadLine();
                                lineCount++;

                                if (line.StartsWith("Status", StringComparison.Ordinal) ||
                                    line.StartsWith("Статус", StringComparison.Ordinal))
                                {
                                    int colonIdx = line.IndexOf(':');

                                    if (colonIdx >= 0)
                                    {
                                        string s = line.Substring(colonIdx + 1).Trim();
                                        serviceStatus = ScadaUtils.ParseServiceStatus(s);
                                        return true;
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.WriteError(ex, Locale.IsRussian ?
                   "Ошибка при получении статуса службы" :
                   "Error getting service status");
            }

            serviceStatus = ServiceStatus.Undefined;
            return false;
        }
    }
}

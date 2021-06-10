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
 * Module   : ScadaWebCommon
 * Summary  : Contains information about user access rights
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2015
 * Modified : 2021
 */

using Scada.Data.Models;
using Scada.Lang;
using System;

namespace Scada.Web.Users
{
    /// <summary>
    /// Contains information about user access rights.
    /// <para>Содержит информацию о правах доступа пользователя.</para>
    /// </summary>
    public class UserRights
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public UserRights()
        {
            SetToDefault();
        }


        /// <summary>
        /// Gets or sets rights accessed by object.
        /// </summary>
        protected RightMatrix.RightByObj RightByObj { get; set; }

        /// <summary>
        /// Gets the role ID.
        /// </summary>
        public int RoleID { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether the role is one of the built-in roles.
        /// </summary>
        public bool RoleIsBuiltIn { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether a user has full administrator rights.
        /// </summary>
        public bool Full { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether a user can view data of any object.
        /// </summary>
        public bool ViewAll { get; protected set; }

        /// <summary>
        /// Gets a value indicating whether a user can send telecontrol commands to any object.
        /// </summary>
        public bool ControlAll { get; protected set; }

        /// <summary>
        /// Gets the default rights according to the user role.
        /// </summary>
        public Right DefaultRight { get; protected set; }


        /// <summary>
        /// Sets the default values.
        /// </summary>
        protected void SetToDefault()
        {
            RightByObj = null;
            RoleID = Data.Const.RoleID.Disabled;
            RoleIsBuiltIn = false;
            Full = false;
            ViewAll = false;
            ControlAll = false;
            DefaultRight = Right.Empty;
        }

        /// <summary>
        /// Defines the rights according to the user role.
        /// </summary>
        protected void DefineRightsByRole(int roleID)
        {
            SetToDefault();
            RoleID = roleID;

            switch (RoleID)
            {
                case Data.Const.RoleID.Administrator:
                    RoleIsBuiltIn = true;
                    Full = true;
                    ViewAll = true;
                    ControlAll = true;
                    break;

                case Data.Const.RoleID.Dispatcher:
                    RoleIsBuiltIn = true;
                    ViewAll = true;
                    ControlAll = true;
                    break;

                case Data.Const.RoleID.Guest:
                    RoleIsBuiltIn = true;
                    ViewAll = true;
                    break;

                case Data.Const.RoleID.Application:
                case Data.Const.RoleID.Disabled:
                    RoleIsBuiltIn = true;
                    break;
            }

            DefaultRight = new Right(ViewAll, ControlAll);
        }


        /// <summary>
        /// Initializes the user rights.
        /// </summary>
        public void Init(RightMatrix rightMatrix, int roleID)
        {
            if (rightMatrix == null)
                throw new ArgumentNullException(nameof(rightMatrix));

            try
            {
                DefineRightsByRole(roleID);

                if (!RoleIsBuiltIn)
                    RightByObj = rightMatrix.GetRightByObj(roleID);
            }
            catch (Exception ex)
            {
                throw new ScadaException(Locale.IsRussian ?
                    "Ошибка при инициализации прав пользователя" :
                    "Error initializing user rights", ex);
            }
        }

        /// <summary>
        /// Gets the access rights on the specified object.
        /// </summary>
        public Right GetRightByObj(int objID)
        {
            if (RoleIsBuiltIn)
            {
                return DefaultRight;
            }
            else
            {
                return RightByObj != null && RightByObj.TryGetValue(objID, out Right right)
                    ? right
                    : Right.Empty;
            }
        }
    }
}

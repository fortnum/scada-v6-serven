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
 * Module   : ModArcBasic
 * Summary  : Specifies the data writing modes
 * 
 * Author   : Mikhail Shiryaev
 * Created  : 2020
 * Modified : 2020
 */

namespace Scada.Server.Modules.ModArcBasic.Logic
{
    /// <summary>
    /// Specifies the data writing modes.
    /// <para>Задает режимы записи данных.</para>
    /// </summary>
    internal enum WritingMode
    {
        /// <summary>
        /// Appends current data to a historical archive with a specified period.
        /// </summary>
        Auto,

        /// <summary>
        /// Writes data to a historical archive only on explicit request.
        /// </summary>
        OnRequest
    }
}

﻿/* Copyright (c) 2017 Rick (rick 'at' gibbed 'dot' us)
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 * 
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 * 
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using Gibbed.MassEffectAndromeda.FileFormats;
using Newtonsoft.Json;

namespace Gibbed.MassEffectAndromeda.SaveFormats.Agents
{
    // ServerJournalSystem
    [JsonObject(MemberSerialization.OptIn)]
    [Agent(_AgentName)]
    public class JournalSystemAgent : Agent
    {
        private const string _AgentName = "ServerJournalSystemSaveGameAgent";

        internal override string AgentName
        {
            get { return _AgentName; }
        }

        #region Fields
        private readonly Data.JournalUnknown0 _Unknown;
        #endregion

        public JournalSystemAgent()
            : base(2)
        {
            this._Unknown = new Data.JournalUnknown0();
        }

        #region Properties
        [JsonProperty("unknown")]
        public Data.JournalUnknown0 Unknown
        {
            get { return this._Unknown; }
        }
        #endregion

        internal override void Read5(IBitReader reader)
        {
            base.Read5(reader);
            this._Unknown.Read(reader);
        }

        internal override void Write5(IBitWriter writer)
        {
            base.Write5(writer);
            this._Unknown.Write(writer);
        }
    }
}
